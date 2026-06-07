using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class CurrencyRateRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly CurrencyRateRepository _sut;

        public CurrencyRateRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new CurrencyRateRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task<(int source, int dest)> SeedCurrencyPairAsync()
        {
            var src = new Currency { Code = "ZZ1", Name = "Src", Symbol = "S", Decimals = 2 };
            var dst = new Currency { Code = "ZZ2", Name = "Dst", Symbol = "D", Decimals = 2 };
            _wrapper.Context.Currencies.AddRange(src, dst);
            await _wrapper.Context.SaveChangesAsync();
            return (src.Id, dst.Id);
        }

        private async Task SeedRateAsync(int sourceId, int destId, DateOnly date, decimal rate, int rateSourceId = 1)
        {
            _wrapper.Context.CurrencyDailyRates.Add(new CurrencyDailyRate
            {
                SourceCurrencyId = sourceId,
                DestinationCurrencyId = destId,
                Date = date,
                Rate = rate,
                RateSourceId = rateSourceId
            });
            await _wrapper.Context.SaveChangesAsync();
        }

        // ── GetExactAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task GetExactAsync_ExactDateExists_ReturnsRate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m);

            var result = await _sut.GetExactAsync(src, dst, date);

            Assert.NotNull(result);
            Assert.Equal(0.92m, result.Rate);
        }

        [Fact]
        public async Task GetExactAsync_NoMatch_ReturnsNull()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);

            var result = await _sut.GetExactAsync(src, dst, date);

            Assert.Null(result);
        }

        // ── GetMostRecentBeforeAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetMostRecentBeforeAsync_ReturnsMostRecentPriorDate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            await SeedRateAsync(src, dst, new DateOnly(2024, 5, 28), 0.90m);
            await SeedRateAsync(src, dst, new DateOnly(2024, 5, 30), 0.91m);

            var result = await _sut.GetMostRecentBeforeAsync(src, dst, new DateOnly(2024, 6, 1));

            Assert.NotNull(result);
            Assert.Equal(0.91m, result.Rate);
        }

        [Fact]
        public async Task GetMostRecentBeforeAsync_NoPriorDate_ReturnsNull()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            var result = await _sut.GetMostRecentBeforeAsync(src, dst, new DateOnly(2024, 6, 1));

            Assert.Null(result);
        }

        // ── GetDefaultAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetDefaultAsync_Exists_ReturnsDefault()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            _wrapper.Context.CurrencyPairDefaults.Add(new CurrencyPairDefault
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst, Rate = 0.88m
            });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDefaultAsync(src, dst);

            Assert.NotNull(result);
            Assert.Equal(0.88m, result.Rate);
        }

        [Fact]
        public async Task GetDefaultAsync_NotFound_ReturnsNull()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            var result = await _sut.GetDefaultAsync(src, dst);

            Assert.Null(result);
        }

        // ── AddRateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task AddRateAsync_PersistsRecord()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);

            await _sut.AddRateAsync(new CurrencyDailyRate
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst,
                Date = date, Rate = 0.92m, RateSourceId = 2
            });

            var stored = await _sut.GetExactAsync(src, dst, date);
            Assert.NotNull(stored);
            Assert.Equal(0.92m, stored.Rate);
        }

        // ── ManualRateExistsAsync via GetExactAsync ────────────────────────────────

        [Fact]
        public async Task GetExactAsync_ManualRatePresent_HasRateSourceId2()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m, rateSourceId: 2);

            var result = await _sut.GetExactAsync(src, dst, date);

            Assert.NotNull(result);
            Assert.Equal(2, result.RateSourceId);
        }

        [Fact]
        public async Task GetExactAsync_AutoRatePresent_HasRateSourceId1()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.91m, rateSourceId: 1);

            var result = await _sut.GetExactAsync(src, dst, date);

            Assert.NotNull(result);
            Assert.Equal(1, result.RateSourceId);
        }

        // ── AddConflictAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task AddConflictAsync_PersistsRecord()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            await _sut.AddConflictAsync(new CurrencyRateConflict
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            });

            var pending = (await _sut.GetPendingConflictsAsync()).ToList();
            Assert.Single(pending);
            Assert.Equal(0.91m, pending[0].AutomaticRate);
        }

        // ── GetPendingConflictsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetPendingConflictsAsync_ReturnsOnlyPendingStatus()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            _wrapper.Context.CurrencyRateConflicts.AddRange(
                new CurrencyRateConflict
                {
                    SourceCurrencyId = src, DestinationCurrencyId = dst,
                    Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1 // Pending
                },
                new CurrencyRateConflict
                {
                    SourceCurrencyId = src, DestinationCurrencyId = dst,
                    Date = new DateOnly(2024, 6, 2), AutomaticRate = 0.90m, ManualRate = 0.89m, StatusId = 2 // Resolved
                });
            await _wrapper.Context.SaveChangesAsync();

            var pending = (await _sut.GetPendingConflictsAsync()).ToList();

            Assert.Single(pending);
            Assert.Equal(1, pending[0].StatusId);
        }

        // ── GetHistoryAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetHistoryAsync_ReturnsAllRatesForPair_OrderedByDateDesc()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            await SeedRateAsync(src, dst, new DateOnly(2024, 6, 1), 0.92m);
            await SeedRateAsync(src, dst, new DateOnly(2024, 6, 2), 0.93m);

            var (items, total) = await _sut.GetHistoryAsync(src, dst, 1, 50);

            Assert.Equal(2, total);
            Assert.Equal(new DateOnly(2024, 6, 2), items[0].Date);
        }

        [Fact]
        public async Task GetHistoryAsync_EmptyForUnknownPair_ReturnsEmpty()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            var (items, total) = await _sut.GetHistoryAsync(src, dst, 1, 50);

            Assert.Equal(0, total);
            Assert.Empty(items);
        }

        // ── UpdateRateAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateRateAsync_ChangesRate_Persists()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m);
            _wrapper.Context.ChangeTracker.Clear();

            var rate = await _sut.GetExactAsync(src, dst, date);
            Assert.NotNull(rate);

            rate.Rate = 0.95m;
            await _sut.UpdateRateAsync(rate);

            var updated = await _sut.GetExactAsync(src, dst, date);
            Assert.NotNull(updated);
            Assert.Equal(0.95m, updated.Rate);
        }

        // ── GetConflictByIdAsync / UpdateConflictAsync ────────────────────────────

        [Fact]
        public async Task GetConflictByIdAsync_ExistingId_ReturnsConflict()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var conflict = new CurrencyRateConflict
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            _wrapper.Context.CurrencyRateConflicts.Add(conflict);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetConflictByIdAsync(conflict.Id);

            Assert.NotNull(result);
            Assert.Equal(0.91m, result.AutomaticRate);
        }

        [Fact]
        public async Task GetConflictByIdAsync_UnknownId_ReturnsNull()
        {
            var result = await _sut.GetConflictByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateConflictAsync_ChangesStatus_Persists()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var conflict = new CurrencyRateConflict
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst,
                Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1
            };
            _wrapper.Context.CurrencyRateConflicts.Add(conflict);
            await _wrapper.Context.SaveChangesAsync();
            _wrapper.Context.ChangeTracker.Clear();

            var fetched = await _sut.GetConflictByIdAsync(conflict.Id);
            Assert.NotNull(fetched);
            fetched.StatusId = 2;
            await _sut.UpdateConflictAsync(fetched);

            var updated = await _sut.GetConflictByIdAsync(conflict.Id);
            Assert.NotNull(updated);
            Assert.Equal(2, updated.StatusId);
        }

        // ── SetDefaultAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task SetDefaultAsync_Insert_NewPair()
        {
            var (src, dst) = await SeedCurrencyPairAsync();

            await _sut.SetDefaultAsync(new CurrencyPairDefault
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst, Rate = 0.88m
            });

            var result = await _sut.GetDefaultAsync(src, dst);
            Assert.NotNull(result);
            Assert.Equal(0.88m, result.Rate);
        }

        [Fact]
        public async Task SetDefaultAsync_Upsert_ExistingPair_UpdatesRate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            _wrapper.Context.CurrencyPairDefaults.Add(new CurrencyPairDefault
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst, Rate = 0.88m
            });
            await _wrapper.Context.SaveChangesAsync();

            await _sut.SetDefaultAsync(new CurrencyPairDefault
            {
                SourceCurrencyId = src, DestinationCurrencyId = dst, Rate = 0.85m
            });

            var result = await _sut.GetDefaultAsync(src, dst);
            Assert.NotNull(result);
            Assert.Equal(0.85m, result.Rate);
        }

        [Fact]
        public void CurrencyRateConflict_Resolution_Setter()
        {
            var conflict = new CurrencyRateConflict();
            var resolution = new ConflictResolution { Id = 1, Name = "AcceptAuto" };
            conflict.Resolution = resolution;
            Assert.Same(resolution, conflict.Resolution);
        }

        // ── GetExistingOnDateAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetExistingOnDateAsync_ReturnsEmpty_WhenNoneExist()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);

            var result = await _sut.GetExistingOnDateAsync(src, date);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExistingOnDateAsync_ReturnsCorrectMap_ForSourceAndDate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m, rateSourceId: 1);

            var result = await _sut.GetExistingOnDateAsync(src, date);

            Assert.Single(result);
            Assert.True(result.ContainsKey(dst));
            Assert.Equal(0.92m, result[dst].Rate);
            Assert.Equal(1, result[dst].RateSourceId);
        }

        [Fact]
        public async Task GetExistingOnDateAsync_DoesNotReturnOtherSourceOrDate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m);
            await SeedRateAsync(src, dst, date.AddDays(1), 0.93m); // different date
            await SeedRateAsync(dst, src, date, 1.08m);             // different source

            var result = await _sut.GetExistingOnDateAsync(src, date);

            Assert.Single(result);
            Assert.Equal(0.92m, result[dst].Rate);
        }

        // ── GetExistingInRangeAsync ────────────────────────────────────────────────

        [Fact]
        public async Task GetExistingInRangeAsync_ReturnsAllRatesInRange()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var d1 = new DateOnly(2024, 6, 1);
            var d2 = new DateOnly(2024, 6, 2);
            await SeedRateAsync(src, dst, d1, 0.92m);
            await SeedRateAsync(src, dst, d2, 0.93m);

            var result = await _sut.GetExistingInRangeAsync(src, d1, d2);

            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey((dst, d1)));
            Assert.True(result.ContainsKey((dst, d2)));
        }

        [Fact]
        public async Task GetExistingInRangeAsync_ExcludesOutsideRange()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var d1 = new DateOnly(2024, 6, 1);
            var d3 = new DateOnly(2024, 6, 3);
            await SeedRateAsync(src, dst, d1, 0.92m);
            await SeedRateAsync(src, dst, d3, 0.94m); // outside [d1, d2]

            var result = await _sut.GetExistingInRangeAsync(src, d1, d1.AddDays(1));

            Assert.Single(result);
            Assert.True(result.ContainsKey((dst, d1)));
        }

        [Fact]
        public async Task GetExistingInRangeAsync_KeyIncludesDestAndDate()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m, rateSourceId: 2);

            var result = await _sut.GetExistingInRangeAsync(src, date, date);

            Assert.True(result.ContainsKey((dst, date)));
            Assert.Equal(0.92m, result[(dst, date)].Rate);
            Assert.Equal(2, result[(dst, date)].RateSourceId);
        }

        // ── GetExistingForPairsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetExistingForPairsAsync_ReturnsEmpty_WhenNoneMatch()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);

            var result = await _sut.GetExistingForPairsAsync([(src, dst, date)]);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExistingForPairsAsync_ReturnsOnlyMatchingPairs()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            await SeedRateAsync(src, dst, date, 0.92m, rateSourceId: 1);
            await SeedRateAsync(src, dst, date.AddDays(1), 0.93m); // different date — not in query

            var result = await _sut.GetExistingForPairsAsync([(src, dst, date)]);

            Assert.Single(result);
            Assert.True(result.ContainsKey((src, dst, date)));
            Assert.Equal(0.92m, result[(src, dst, date)].Rate);
            Assert.Equal(1, result[(src, dst, date)].RateSourceId);
        }

        // ── AddRatesBatchAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task AddRatesBatchAsync_EmptyList_DoesNotThrow()
        {
            await _sut.AddRatesBatchAsync([]);
            Assert.Empty(_wrapper.Context.CurrencyDailyRates);
        }

        [Fact]
        public async Task AddRatesBatchAsync_InsertsAllRates()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var date = new DateOnly(2024, 6, 1);
            var rates = new List<CurrencyDailyRate>
            {
                new() { SourceCurrencyId = src, DestinationCurrencyId = dst, Date = date, Rate = 0.92m, RateSourceId = 1 },
                new() { SourceCurrencyId = dst, DestinationCurrencyId = src, Date = date, Rate = 1.08m, RateSourceId = 1 }
            };

            await _sut.AddRatesBatchAsync(rates);

            Assert.Equal(2, _wrapper.Context.CurrencyDailyRates.Count());
        }

        // ── IsUsedInRatesAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task IsUsedInRatesAsync_ReturnsTrue_WhenUsedAsSource()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            await SeedRateAsync(src, dst, new DateOnly(2024, 1, 1), 1.1m);

            var result = await _sut.IsUsedInRatesAsync(src);

            Assert.True(result);
        }

        [Fact]
        public async Task IsUsedInRatesAsync_ReturnsTrue_WhenUsedAsDestination()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            await SeedRateAsync(src, dst, new DateOnly(2024, 1, 1), 1.1m);

            var result = await _sut.IsUsedInRatesAsync(dst);

            Assert.True(result);
        }

        [Fact]
        public async Task IsUsedInRatesAsync_ReturnsFalse_WhenNotUsed()
        {
            var result = await _sut.IsUsedInRatesAsync(9999);

            Assert.False(result);
        }

        // ── AddConflictsBatchAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AddConflictsBatchAsync_InsertsAllConflicts()
        {
            var (src, dst) = await SeedCurrencyPairAsync();
            var conflicts = new List<CurrencyRateConflict>
            {
                new() { SourceCurrencyId = src, DestinationCurrencyId = dst, Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, StatusId = 1 },
                new() { SourceCurrencyId = src, DestinationCurrencyId = dst, Date = new DateOnly(2024, 6, 2), AutomaticRate = 0.90m, ManualRate = 0.89m, StatusId = 1 }
            };

            await _sut.AddConflictsBatchAsync(conflicts);

            Assert.Equal(2, _wrapper.Context.CurrencyRateConflicts.Count());
        }
    }
}
