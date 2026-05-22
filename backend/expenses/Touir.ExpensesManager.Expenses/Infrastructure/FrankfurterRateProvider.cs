using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class FrankfurterRateProvider : IRateProvider
    {
        private const string BaseUrl = "https://api.frankfurter.dev/";
        private const string DateFormat = "yyyy-MM-dd";

        private readonly HttpClient _http;

        public FrankfurterRateProvider(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<Dictionary<string, decimal>> FetchRatesAsync(string sourceCurrencyCode, DateOnly date, CancellationToken ct = default)
        {
            var dateStr = date.ToString(DateFormat, CultureInfo.InvariantCulture);
            var response = await _http.GetAsync($"v2/rates?date={dateStr}&base={sourceCurrencyCode}", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var result = new Dictionary<string, decimal>();
            foreach (var entry in doc.RootElement.EnumerateArray())
                result[entry.GetProperty("quote").GetString()!] = entry.GetProperty("rate").GetDecimal();

            return result;
        }

        public async Task<Dictionary<DateOnly, Dictionary<string, decimal>>> FetchRatesRangeAsync(
            string sourceCurrencyCode, DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var fromStr = from.ToString(DateFormat, CultureInfo.InvariantCulture);
            var toStr = to.ToString(DateFormat, CultureInfo.InvariantCulture);
            var response = await _http.GetAsync($"v2/rates?from={fromStr}&to={toStr}&base={sourceCurrencyCode}", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var result = new Dictionary<DateOnly, Dictionary<string, decimal>>();
            foreach (var entry in doc.RootElement.EnumerateArray())
            {
                var date = DateOnly.ParseExact(entry.GetProperty("date").GetString()!, DateFormat, CultureInfo.InvariantCulture);
                if (!result.TryGetValue(date, out var dayRates))
                {
                    dayRates = [];
                    result[date] = dayRates;
                }
                dayRates[entry.GetProperty("quote").GetString()!] = entry.GetProperty("rate").GetDecimal();
            }

            return result;
        }
    }
}
