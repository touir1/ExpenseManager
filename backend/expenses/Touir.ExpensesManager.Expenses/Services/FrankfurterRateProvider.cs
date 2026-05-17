using System.Globalization;
using System.Text.Json;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class FrankfurterRateProvider : IRateProvider
    {
        private const string BaseUrl = "https://api.frankfurter.app/";
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
            var response = await _http.GetAsync($"{dateStr}?from={sourceCurrencyCode}", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var rates = doc.RootElement.GetProperty("rates");

            var result = new Dictionary<string, decimal>();
            foreach (var prop in rates.EnumerateObject())
                result[prop.Name] = prop.Value.GetDecimal();

            return result;
        }

        public async Task<Dictionary<DateOnly, Dictionary<string, decimal>>> FetchRatesRangeAsync(
            string sourceCurrencyCode, DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var fromStr = from.ToString(DateFormat, CultureInfo.InvariantCulture);
            var toStr = to.ToString(DateFormat, CultureInfo.InvariantCulture);
            var response = await _http.GetAsync($"{fromStr}..{toStr}?from={sourceCurrencyCode}", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var ratesByDate = doc.RootElement.GetProperty("rates");

            var result = new Dictionary<DateOnly, Dictionary<string, decimal>>();
            foreach (var dateProp in ratesByDate.EnumerateObject())
            {
                var date = DateOnly.ParseExact(dateProp.Name, DateFormat, CultureInfo.InvariantCulture);
                var dayRates = new Dictionary<string, decimal>();
                foreach (var currProp in dateProp.Value.EnumerateObject())
                    dayRates[currProp.Name] = currProp.Value.GetDecimal();
                result[date] = dayRates;
            }

            return result;
        }
    }
}
