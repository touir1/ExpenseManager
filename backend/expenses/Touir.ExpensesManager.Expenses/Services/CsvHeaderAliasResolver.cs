namespace Touir.ExpensesManager.Expenses.Services
{
    public static class CsvHeaderAliasResolver
    {
        public const int MaxColumns = 20;

        public static readonly string[] CanonicalFields =
            ["date", "amount", "currency_code", "category", "subcategory", "description", "tags", "families"];

        public static readonly string[] RequiredCanonicalFields = ["date", "amount", "currency_code"];

        private static readonly Dictionary<string, string[]> AliasTable = new(StringComparer.OrdinalIgnoreCase)
        {
            ["date"] = ["date", "transaction_date", "txn_date"],
            ["amount"] = ["amount", "amt", "sum", "value", "price"],
            ["currency_code"] = ["currency_code", "currency", "cur", "ccy"],
            ["category"] = ["category", "cat"],
            ["subcategory"] = ["subcategory", "subcat", "sub_category"],
            ["description"] = ["description", "desc", "note", "memo"],
            ["tags"] = ["tags", "tag"],
            ["families"] = ["families", "family", "fam"],
        };

        /// <summary>
        /// Suggests a rawHeader -> canonicalField mapping using the alias table.
        /// First-match-wins: each canonical field is suggested for at most one raw header.
        /// </summary>
        public static Dictionary<string, string> SuggestMapping(IEnumerable<string> rawHeaders)
        {
            var suggestions = new Dictionary<string, string>();
            var usedCanonical = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in rawHeaders)
            {
                var trimmed = raw?.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                foreach (var (canonical, aliases) in AliasTable)
                {
                    if (usedCanonical.Contains(canonical))
                        continue;

                    if (aliases.Any(a => string.Equals(a, trimmed, StringComparison.OrdinalIgnoreCase)))
                    {
                        suggestions[raw!] = canonical;
                        usedCanonical.Add(canonical);
                        break;
                    }
                }
            }

            return suggestions;
        }

        public static bool IsExactHeaderMatch(IEnumerable<string> rawHeaders)
        {
            var headers = rawHeaders as ICollection<string> ?? rawHeaders.ToList();
            return RequiredCanonicalFields.All(h => headers.Contains(h, StringComparer.OrdinalIgnoreCase));
        }
    }
}
