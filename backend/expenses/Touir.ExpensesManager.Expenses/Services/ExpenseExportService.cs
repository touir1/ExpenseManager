using System.Text;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class ExpenseExportService : IExpenseExportService
    {
        private readonly ExpensesDbContext _dbContext;

        public ExpenseExportService(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Stream> ExportCsvAsync(int userId, CancellationToken cancellationToken = default)
        {
            var expenses = await _dbContext.Expenses
                .Where(e => e.UserId == userId && !e.IsDeleted)
                .Include(e => e.Currency)
                .Include(e => e.Category)
                .Include(e => e.Subcategory)
                .Include(e => e.ExpenseTags).ThenInclude(et => et.Tag)
                .Include(e => e.ExpenseFamilyAttributions).ThenInclude(a => a.Family)
                .OrderByDescending(e => e.Date)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

            await writer.WriteLineAsync("date,amount,currency,category,subcategory,description,tags,families");

            foreach (var e in expenses)
            {
                var tags = string.Join(";", e.ExpenseTags.Select(t => t.Tag.Name));
                var families = string.Join(";", e.ExpenseFamilyAttributions
                    .Where(a => !a.Family.IsDeleted)
                    .Select(a => a.Family.Name));

                var line = string.Join(",",
                    e.Date.ToString("yyyy-MM-dd"),
                    e.Amount.ToString("F4", System.Globalization.CultureInfo.InvariantCulture),
                    CsvEscape(e.Currency.Code),
                    CsvEscape(e.Category?.Name ?? ""),
                    CsvEscape(e.Subcategory?.Name ?? ""),
                    CsvEscape(e.Description ?? ""),
                    CsvEscape(tags),
                    CsvEscape(families));

                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;
            return stream;
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
