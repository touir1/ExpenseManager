using Moq;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Touir.ExpensesManager.Expenses.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class CurrencyRateServiceTests
    {
        private static CurrencyRateService CreateService(
            ICurrencyRateRepository? rateRepo = null,
            ICurrencyRepository? currencyRepo = null,
            IRateProvider? rateProvider = null,
            ILookupCacheService? lookupCache = null)
        {
            return new CurrencyRateService(
                rateRepo ?? Mock.Of<ICurrencyRateRepository>(),
                currencyRepo ?? Mock.Of<ICurrencyRepository>(),
                rateProvider ?? Mock.Of<IRateProvider>(),
                lookupCache ?? Mock.Of<ILookupCacheService>());
        }

        private static CurrencyDailyRate MakeRate(int sourceId, int destId, DateOnly date, decimal rate, int rateSourceId = 1) => new()
        {
            Id = 1,
            SourceCurrencyId = sourceId,
            DestinationCurrencyId = destId,
            Date = date,
            Rate = rate,
            RateSourceId = rateSourceId,
            RateSource = new RateSource { Id = rateSourceId, Name = rateSourceId == 1 ? "Auto" : "Manual" }
        };

        // ── ResolveRateAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task ResolveRateAsync_SameCurrency_ReturnsOne()
        {
            var result = await CreateService().ResolveRateAsync(1, 1, DateOnly.FromDateTime(DateTime.UtcNow));
            Assert.Equal(1m, result);
        }

        [Fact]
        public async Task ResolveRateAsync_ExactDateMatch_ReturnsExactRate()
        {
            var date = new DateOnly(2024, 6, 1);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date))
                .ReturnsAsync(MakeRate(1, 2, date, 0.92m));

            var result = await CreateService(rateRepo: repo.Object).ResolveRateAsync(1, 2, date);

            Assert.Equal(0.92m, result);
        }

        [Fact]
        public async Task ResolveRateAsync_NoExact_HasPriorDate_ReturnsMostRecent()
        {
            var date = new DateOnly(2024, 6, 5);
            var priorDate = new DateOnly(2024, 6, 3);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);
            repo.Setup(r => r.GetMostRecentBeforeAsync(1, 2, date))
                .ReturnsAsync(MakeRate(1, 2, priorDate, 0.91m));

            var result = await CreateService(rateRepo: repo.Object).ResolveRateAsync(1, 2, date);

            Assert.Equal(0.91m, result);
        }

        [Fact]
        public async Task ResolveRateAsync_NoDailyRate_HasDefault_ReturnsDefault()
        {
            var date = new DateOnly(2024, 6, 5);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);
            repo.Setup(r => r.GetMostRecentBeforeAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);
            repo.Setup(r => r.GetDefaultAsync(1, 2)).ReturnsAsync(new CurrencyPairDefault
            {
                SourceCurrencyId = 1,
                DestinationCurrencyId = 2,
                Rate = 0.90m
            });

            var result = await CreateService(rateRepo: repo.Object).ResolveRateAsync(1, 2, date);

            Assert.Equal(0.90m, result);
        }

        [Fact]
        public async Task ResolveRateAsync_NoRateAnywhere_ReturnsNull()
        {
            var date = new DateOnly(2024, 6, 5);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);
            repo.Setup(r => r.GetMostRecentBeforeAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);
            repo.Setup(r => r.GetDefaultAsync(1, 2)).ReturnsAsync((CurrencyPairDefault?)null);

            var result = await CreateService(rateRepo: repo.Object).ResolveRateAsync(1, 2, date);

            Assert.Null(result);
        }

        // ── AddManualRateAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task AddManualRateAsync_NoPriorRate_InsertsRate_WithManualSource()
        {
            var date = new DateOnly(2024, 6, 1);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date)).ReturnsAsync((CurrencyDailyRate?)null);

            var request = new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = date, Rate = 0.92m };
            await CreateService(rateRepo: repo.Object).AddManualRateAsync(request, adminUserId: 99);

            repo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.RateSourceId == 2 && x.Rate == 0.92m)), Times.Once);
        }

        [Fact]
        public async Task AddManualRateAsync_AutoRateExists_InsertsConflict_NotNewRate()
        {
            var date = new DateOnly(2024, 6, 1);
            var existing = MakeRate(1, 2, date, 0.91m, rateSourceId: 1);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(1, 2, date)).ReturnsAsync(existing);

            var request = new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = date, Rate = 0.92m };
            await CreateService(rateRepo: repo.Object).AddManualRateAsync(request, adminUserId: 99);

            repo.Verify(r => r.AddConflictAsync(It.Is<CurrencyRateConflict>(
                c => c.AutomaticRate == 0.91m && c.ManualRate == 0.92m && c.StatusId == 1)), Times.Once);
            repo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        [Fact]
        public async Task BulkAddManualRatesAsync_MultipleEntries_CallsAddForEach()
        {
            var date = new DateOnly(2024, 6, 1);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((CurrencyDailyRate?)null);

            var request = new BulkAddRatesRequest
            {
                Rates = [
                    new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = date, Rate = 0.92m },
                    new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 3, Date = date, Rate = 1.10m }
                ]
            };

            await CreateService(rateRepo: repo.Object).BulkAddManualRatesAsync(request, adminUserId: 1);

            repo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SetDefaultFallbackAsync_UpsertsCurrencyPairDefault()
        {
            var repo = new Mock<ICurrencyRateRepository>();
            var request = new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 0.88m };

            await CreateService(rateRepo: repo.Object).SetDefaultFallbackAsync(request, adminUserId: 1);

            repo.Verify(r => r.SetDefaultAsync(It.Is<CurrencyPairDefault>(
                p => p.SourceCurrencyId == 1 && p.DestinationCurrencyId == 2 && p.Rate == 0.88m)), Times.Once);
        }

        // ── ResolveConflictAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task ResolveConflictAsync_AcceptAuto_UpdatesRateToAutomatic_MarksResolved()
        {
            var conflict = new CurrencyRateConflict
            {
                Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            var existingRate = MakeRate(1, 2, conflict.Date, 0.92m, rateSourceId: 2);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetConflictByIdAsync(1)).ReturnsAsync(conflict);
            repo.Setup(r => r.GetExactAsync(1, 2, conflict.Date)).ReturnsAsync(existingRate);
            var cache = new Mock<ILookupCacheService>();
            cache.Setup(c => c.GetIdAsync<ConflictResolution>("AcceptAuto")).ReturnsAsync(1);

            await CreateService(rateRepo: repo.Object, lookupCache: cache.Object)
                .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "AcceptAuto" }, adminUserId: 1);

            repo.Verify(r => r.UpdateRateAsync(It.Is<CurrencyDailyRate>(x => x.Rate == 0.91m)), Times.Once);
            repo.Verify(r => r.UpdateConflictAsync(It.Is<CurrencyRateConflict>(x => x.StatusId == 2)), Times.Once);
        }

        [Fact]
        public async Task ResolveConflictAsync_KeepManual_LeavesRateUnchanged_MarksResolved()
        {
            var conflict = new CurrencyRateConflict
            {
                Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetConflictByIdAsync(1)).ReturnsAsync(conflict);
            var cache = new Mock<ILookupCacheService>();
            cache.Setup(c => c.GetIdAsync<ConflictResolution>("KeepManual")).ReturnsAsync(2);

            await CreateService(rateRepo: repo.Object, lookupCache: cache.Object)
                .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "KeepManual" }, adminUserId: 1);

            repo.Verify(r => r.UpdateRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
            repo.Verify(r => r.UpdateConflictAsync(It.Is<CurrencyRateConflict>(x => x.StatusId == 2)), Times.Once);
        }

        [Fact]
        public async Task ResolveConflictAsync_Custom_UpdatesRateToCustom_MarksResolved()
        {
            var conflict = new CurrencyRateConflict
            {
                Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            var existingRate = MakeRate(1, 2, conflict.Date, 0.92m, rateSourceId: 2);
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetConflictByIdAsync(1)).ReturnsAsync(conflict);
            repo.Setup(r => r.GetExactAsync(1, 2, conflict.Date)).ReturnsAsync(existingRate);
            var cache = new Mock<ILookupCacheService>();
            cache.Setup(c => c.GetIdAsync<ConflictResolution>("Custom")).ReturnsAsync(3);

            await CreateService(rateRepo: repo.Object, lookupCache: cache.Object)
                .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "Custom", CustomRate = 0.895m }, adminUserId: 1);

            repo.Verify(r => r.UpdateRateAsync(It.Is<CurrencyDailyRate>(x => x.Rate == 0.895m)), Times.Once);
            repo.Verify(r => r.UpdateConflictAsync(It.Is<CurrencyRateConflict>(x => x.CustomRate == 0.895m)), Times.Once);
        }

        [Fact]
        public async Task ResolveConflictAsync_ConflictNotFound_Throws()
        {
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetConflictByIdAsync(99)).ReturnsAsync((CurrencyRateConflict?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                CreateService(rateRepo: repo.Object)
                    .ResolveConflictAsync(99, new ResolveConflictRequest { Resolution = "KeepManual" }, adminUserId: 1));
        }

        [Fact]
        public async Task ResolveConflictAsync_Custom_NullCustomRate_ThrowsArgumentException()
        {
            var conflict = new CurrencyRateConflict
            {
                Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetConflictByIdAsync(1)).ReturnsAsync(conflict);
            var cache = new Mock<ILookupCacheService>();
            cache.Setup(c => c.GetIdAsync<ConflictResolution>("Custom")).ReturnsAsync(3);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                CreateService(rateRepo: repo.Object, lookupCache: cache.Object)
                    .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "Custom", CustomRate = null }, adminUserId: 1));
        }

        [Fact]
        public async Task GetRateHistoryAsync_ReturnsAllHistoryMappedToDto()
        {
            var date = new DateOnly(2024, 6, 1);
            var rates = new List<CurrencyDailyRate>
            {
                MakeRate(1, 2, date, 0.92m),
                MakeRate(1, 2, date.AddDays(-1), 0.91m)
            };
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetHistoryAsync(1, 2)).ReturnsAsync(rates);

            var result = (await CreateService(rateRepo: repo.Object).GetRateHistoryAsync(1, 2)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(0.92m, result[0].Rate);
        }

        // ── RunDailyUpdateAsync ────────────────────────────────────────────────────

        private static Currency MakeCurrency(int id, string code) => new() { Id = id, Code = code, Name = code, Symbol = code, Decimals = 2 };

        [Fact]
        public async Task RunDailyUpdateAsync_InsertsAutoRates_ForAllCurrencyPairs()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesAsync("USD", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 0.92m });
            provider.Setup(p => p.FetchRatesAsync("EUR", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["USD"] = 1.08m });
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), today))
                .ReturnsAsync((CurrencyDailyRate?)null);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RunDailyUpdateAsync();

            rateRepo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.RateSourceId == 1)), Times.Exactly(2));
        }

        [Fact]
        public async Task RunDailyUpdateAsync_ManualRateExists_CreatesConflict_NotNewRate()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesAsync("USD", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 0.92m });
            provider.Setup(p => p.FetchRatesAsync("EUR", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal>());
            var existingManual = MakeRate(1, 2, today, 0.90m, rateSourceId: 2);
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(1, 2, today)).ReturnsAsync(existingManual);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RunDailyUpdateAsync();

            rateRepo.Verify(r => r.AddConflictAsync(It.Is<CurrencyRateConflict>(
                c => c.AutomaticRate == 0.92m && c.ManualRate == 0.90m && c.StatusId == 1)), Times.Once);
            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        [Fact]
        public async Task RunDailyUpdateAsync_AutoRateAlreadyExists_Skips()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesAsync("USD", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["EUR"] = 0.92m });
            provider.Setup(p => p.FetchRatesAsync("EUR", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal>());
            var existingAuto = MakeRate(1, 2, today, 0.91m, rateSourceId: 1);
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(1, 2, today)).ReturnsAsync(existingAuto);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RunDailyUpdateAsync();

            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
            rateRepo.Verify(r => r.AddConflictAsync(It.IsAny<CurrencyRateConflict>()), Times.Never);
        }

        [Fact]
        public async Task RunDailyUpdateAsync_ProviderThrows_ContinuesToNextCurrency()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesAsync("USD", today, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("network error"));
            provider.Setup(p => p.FetchRatesAsync("EUR", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["USD"] = 1.08m });
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), today))
                .ReturnsAsync((CurrencyDailyRate?)null);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RunDailyUpdateAsync();

            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Once);
        }

        [Fact]
        public async Task RunDailyUpdateAsync_SkipsDestCode_NotInDatabase()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesAsync("USD", today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, decimal> { ["XXX"] = 1.5m });
            var rateRepo = new Mock<ICurrencyRateRepository>();

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RunDailyUpdateAsync();

            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        // ── RefreshRatesFromAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task RefreshRatesFromAsync_InsertsAutoRates_ForAllDatesAndPairs()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["EUR"] = 0.92m },
                    [from.AddDays(1)] = new() { ["EUR"] = 0.93m }
                });
            provider.Setup(p => p.FetchRatesRangeAsync("EUR", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>());
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((CurrencyDailyRate?)null);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from);

            rateRepo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.RateSourceId == 1)), Times.Exactly(2));
        }

        [Fact]
        public async Task RefreshRatesFromAsync_ManualRateExists_CreatesConflict()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["EUR"] = 0.92m }
                });
            provider.Setup(p => p.FetchRatesRangeAsync("EUR", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>());
            var existingManual = MakeRate(1, 2, from, 0.90m, rateSourceId: 2);
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(1, 2, from)).ReturnsAsync(existingManual);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from);

            rateRepo.Verify(r => r.AddConflictAsync(It.Is<CurrencyRateConflict>(
                c => c.AutomaticRate == 0.92m && c.ManualRate == 0.90m && c.StatusId == 1)), Times.Once);
            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        [Fact]
        public async Task RefreshRatesFromAsync_ProviderThrows_ContinuesToNextCurrency()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("network error"));
            provider.Setup(p => p.FetchRatesRangeAsync("EUR", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["USD"] = 1.08m }
                });
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((CurrencyDailyRate?)null);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from);

            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Once);
        }

        [Fact]
        public async Task RefreshRatesFromAsync_SkipsDestCode_NotInDatabase()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["XXX"] = 1.5m }
                });
            var rateRepo = new Mock<ICurrencyRateRepository>();

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from);

            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        [Fact]
        public async Task RefreshRatesFromAsync_SourceCurrencyFilter_OnlyFetchesForThatSource()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["EUR"] = 0.92m }
                });
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((CurrencyDailyRate?)null);

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from, sourceCurrencyId: 1);

            provider.Verify(p => p.FetchRatesRangeAsync("USD", from, today, It.IsAny<CancellationToken>()), Times.Once);
            provider.Verify(p => p.FetchRatesRangeAsync("EUR", from, today, It.IsAny<CancellationToken>()), Times.Never);
            rateRepo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.RateSourceId == 1)), Times.Once);
        }

        [Fact]
        public async Task RefreshRatesFromAsync_DestinationCurrencyFilter_OnlyInsertsMatchingDest()
        {
            var from = new DateOnly(2024, 6, 1);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = new List<Currency> { MakeCurrency(1, "USD"), MakeCurrency(2, "EUR"), MakeCurrency(3, "GBP") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            provider.Setup(p => p.FetchRatesRangeAsync(It.IsAny<string>(), from, today, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<DateOnly, Dictionary<string, decimal>>
                {
                    [from] = new() { ["EUR"] = 0.92m, ["GBP"] = 0.79m }
                });
            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.GetExactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((CurrencyDailyRate?)null);

            // Only refresh USD→EUR (destCurrencyId=2), not USD→GBP
            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from, sourceCurrencyId: 1, destinationCurrencyId: 2);

            rateRepo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.DestinationCurrencyId == 2)), Times.Once);
            rateRepo.Verify(r => r.AddRateAsync(It.Is<CurrencyDailyRate>(x => x.DestinationCurrencyId == 3)), Times.Never);
        }

        [Fact]
        public async Task RefreshRatesFromAsync_UnknownSourceCurrencyId_FetchesNothing()
        {
            var from = new DateOnly(2024, 6, 1);
            var currencies = new List<Currency> { MakeCurrency(1, "USD") };
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);
            var provider = new Mock<IRateProvider>();
            var rateRepo = new Mock<ICurrencyRateRepository>();

            await CreateService(rateRepo: rateRepo.Object, currencyRepo: currencyRepo.Object, rateProvider: provider.Object)
                .RefreshRatesFromAsync(from, sourceCurrencyId: 999);

            provider.Verify(p => p.FetchRatesRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
            rateRepo.Verify(r => r.AddRateAsync(It.IsAny<CurrencyDailyRate>()), Times.Never);
        }

        [Fact]
        public async Task GetPendingConflictsAsync_ReturnsMappedConflicts()
        {
            var conflicts = new List<CurrencyRateConflict>
            {
                new()
                {
                    Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                    Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m,
                    StatusId = 1, Status = new ConflictStatus { Id = 1, Name = "Pending" }
                }
            };
            var repo = new Mock<ICurrencyRateRepository>();
            repo.Setup(r => r.GetPendingConflictsAsync()).ReturnsAsync(conflicts);

            var result = (await CreateService(rateRepo: repo.Object).GetPendingConflictsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("Pending", result[0].Status);
        }
    }
}
