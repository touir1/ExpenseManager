using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class CurrencyRateService : ICurrencyRateService
    {
        private const int RateSourceAuto = 1;
        private const int RateSourceManual = 2;
        private const int ConflictStatusPending = 1;
        private const int ConflictStatusResolved = 2;

        private readonly ICurrencyRateRepository _rateRepo;
        private readonly ICurrencyRepository _currencyRepo;
        private readonly IRateProvider _rateProvider;
        private readonly ILookupCacheService _lookupCache;

        public CurrencyRateService(
            ICurrencyRateRepository rateRepo,
            ICurrencyRepository currencyRepo,
            IRateProvider rateProvider,
            ILookupCacheService lookupCache)
        {
            _rateRepo = rateRepo;
            _currencyRepo = currencyRepo;
            _rateProvider = rateProvider;
            _lookupCache = lookupCache;
        }

        public async Task<decimal?> ResolveRateAsync(int sourceCurrencyId, int destCurrencyId, DateOnly date)
        {
            if (sourceCurrencyId == destCurrencyId)
                return 1m;

            var exact = await _rateRepo.GetExactAsync(sourceCurrencyId, destCurrencyId, date);
            if (exact is not null)
                return exact.Rate;

            var recent = await _rateRepo.GetMostRecentBeforeAsync(sourceCurrencyId, destCurrencyId, date);
            if (recent is not null)
                return recent.Rate;

            var fallback = await _rateRepo.GetDefaultAsync(sourceCurrencyId, destCurrencyId);
            return fallback?.Rate;
        }

        public async Task<IEnumerable<RateDto>> GetRateHistoryAsync(int sourceCurrencyId, int destCurrencyId)
        {
            var rates = await _rateRepo.GetHistoryAsync(sourceCurrencyId, destCurrencyId);
            return rates.Select(r => MapToRateDto(r));
        }

        public async Task<RateDto> AddManualRateAsync(AddRateRequest request, int adminUserId)
        {
            var existing = await _rateRepo.GetExactAsync(
                request.SourceCurrencyId, request.DestinationCurrencyId, request.Date);

            if (existing is not null)
            {
                await _rateRepo.AddConflictAsync(new CurrencyRateConflict
                {
                    SourceCurrencyId = request.SourceCurrencyId,
                    DestinationCurrencyId = request.DestinationCurrencyId,
                    Date = request.Date,
                    AutomaticRate = existing.Rate,
                    ManualRate = request.Rate,
                    StatusId = ConflictStatusPending
                });

                return MapToRateDto(existing);
            }

            var rate = new CurrencyDailyRate
            {
                SourceCurrencyId = request.SourceCurrencyId,
                DestinationCurrencyId = request.DestinationCurrencyId,
                Date = request.Date,
                Rate = request.Rate,
                RateSourceId = RateSourceManual
            };

            await _rateRepo.AddRateAsync(rate);
            return MapToRateDto(rate, "Manual");
        }

        public async Task BulkAddManualRatesAsync(BulkAddRatesRequest request, int adminUserId)
        {
            foreach (var entry in request.Rates)
                await AddManualRateAsync(entry, adminUserId);
        }

        public async Task SetDefaultFallbackAsync(SetDefaultRateRequest request, int adminUserId)
        {
            await _rateRepo.SetDefaultAsync(new CurrencyPairDefault
            {
                SourceCurrencyId = request.SourceCurrencyId,
                DestinationCurrencyId = request.DestinationCurrencyId,
                Rate = request.Rate
            });
        }

        public async Task ResolveConflictAsync(int conflictId, ResolveConflictRequest request, int adminUserId)
        {
            var conflict = await _rateRepo.GetConflictByIdAsync(conflictId)
                ?? throw new KeyNotFoundException($"Conflict {conflictId} not found.");

            var resolutionId = await _lookupCache.GetIdAsync<ConflictResolution>(request.Resolution);

            conflict.StatusId = ConflictStatusResolved;
            conflict.ResolutionId = resolutionId;
            conflict.ResolvedAt = DateTime.UtcNow;
            conflict.ResolvedById = adminUserId;

            if (resolutionId == 1) // AcceptAuto
            {
                var existingRate = await _rateRepo.GetExactAsync(
                    conflict.SourceCurrencyId, conflict.DestinationCurrencyId, conflict.Date);
                if (existingRate is not null)
                {
                    existingRate.Rate = conflict.AutomaticRate;
                    await _rateRepo.UpdateRateAsync(existingRate);
                }
            }
            else if (resolutionId == 3) // Custom
            {
                if (request.CustomRate is null)
                    throw new ArgumentException("CustomRate required for Custom resolution.");

                conflict.CustomRate = request.CustomRate;
                var existingRate = await _rateRepo.GetExactAsync(
                    conflict.SourceCurrencyId, conflict.DestinationCurrencyId, conflict.Date);
                if (existingRate is not null)
                {
                    existingRate.Rate = request.CustomRate.Value;
                    await _rateRepo.UpdateRateAsync(existingRate);
                }
            }
            // KeepManual (2): existing rate stays as is — no rate update needed

            await _rateRepo.UpdateConflictAsync(conflict);
        }

        public async Task<IEnumerable<RateConflictDto>> GetPendingConflictsAsync()
        {
            var conflicts = await _rateRepo.GetPendingConflictsAsync();
            return conflicts.Select(MapToConflictDto);
        }

        public async Task RefreshRatesFromAsync(DateOnly from, int? sourceCurrencyId = null, int? destinationCurrencyId = null, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = (await _currencyRepo.GetAllAsync()).ToList();
            var codeToId = currencies.ToDictionary(c => c.Code, c => c.Id);

            var sourceCurrencies = sourceCurrencyId.HasValue
                ? currencies.Where(c => c.Id == sourceCurrencyId.Value).ToList()
                : currencies;

            foreach (var source in sourceCurrencies)
            {
                Dictionary<DateOnly, Dictionary<string, decimal>> ratesByDate;
                try
                {
                    ratesByDate = await _rateProvider.FetchRatesRangeAsync(source.Code, from, today, ct);
                }
                catch
                {
                    continue;
                }

                foreach (var (date, dayRates) in ratesByDate)
                {
                    foreach (var (destCode, rate) in dayRates)
                    {
                        if (!codeToId.TryGetValue(destCode, out var destCurrencyId))
                            continue;

                        if (destinationCurrencyId.HasValue && destCurrencyId != destinationCurrencyId.Value)
                            continue;

                        var existing = await _rateRepo.GetExactAsync(source.Id, destCurrencyId, date);

                        if (existing is not null && existing.RateSourceId == RateSourceManual)
                        {
                            await _rateRepo.AddConflictAsync(new CurrencyRateConflict
                            {
                                SourceCurrencyId = source.Id,
                                DestinationCurrencyId = destCurrencyId,
                                Date = date,
                                AutomaticRate = rate,
                                ManualRate = existing.Rate,
                                StatusId = ConflictStatusPending
                            });
                        }
                        else if (existing is null)
                        {
                            await _rateRepo.AddRateAsync(new CurrencyDailyRate
                            {
                                SourceCurrencyId = source.Id,
                                DestinationCurrencyId = destCurrencyId,
                                Date = date,
                                Rate = rate,
                                RateSourceId = RateSourceAuto
                            });
                        }
                    }
                }
            }
        }

        public async Task RunDailyUpdateAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currencies = (await _currencyRepo.GetAllAsync()).ToList();
            var codeToId = currencies.ToDictionary(c => c.Code, c => c.Id);

            foreach (var source in currencies)
            {
                Dictionary<string, decimal> fetchedRates;
                try
                {
                    fetchedRates = await _rateProvider.FetchRatesAsync(source.Code, today, ct);
                }
                catch
                {
                    continue;
                }

                foreach (var (destCode, rate) in fetchedRates)
                {
                    if (!codeToId.TryGetValue(destCode, out var destCurrencyId))
                        continue;

                    var existing = await _rateRepo.GetExactAsync(source.Id, destCurrencyId, today);

                    if (existing is not null && existing.RateSourceId == RateSourceManual)
                    {
                        await _rateRepo.AddConflictAsync(new CurrencyRateConflict
                        {
                            SourceCurrencyId = source.Id,
                            DestinationCurrencyId = destCurrencyId,
                            Date = today,
                            AutomaticRate = rate,
                            ManualRate = existing.Rate,
                            StatusId = ConflictStatusPending
                        });
                    }
                    else if (existing is null)
                    {
                        await _rateRepo.AddRateAsync(new CurrencyDailyRate
                        {
                            SourceCurrencyId = source.Id,
                            DestinationCurrencyId = destCurrencyId,
                            Date = today,
                            Rate = rate,
                            RateSourceId = RateSourceAuto
                        });
                    }
                }
            }
        }

        private static RateDto MapToRateDto(CurrencyDailyRate r, string? sourceName = null) => new()
        {
            Id = r.Id,
            SourceCurrencyId = r.SourceCurrencyId,
            DestinationCurrencyId = r.DestinationCurrencyId,
            Date = r.Date,
            Rate = r.Rate,
            RateSource = sourceName ?? r.RateSource?.Name ?? string.Empty
        };

        private static RateConflictDto MapToConflictDto(CurrencyRateConflict c) => new()
        {
            Id = c.Id,
            SourceCurrencyId = c.SourceCurrencyId,
            DestinationCurrencyId = c.DestinationCurrencyId,
            Date = c.Date,
            AutomaticRate = c.AutomaticRate,
            ManualRate = c.ManualRate,
            Status = c.Status?.Name ?? string.Empty,
            ResolvedAt = c.ResolvedAt
        };
    }
}
