using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class CsvHeaderAliasResolverTests
    {
        // ── SuggestMapping ────────────────────────────────────────────────────

        [Fact]
        public void SuggestMapping_ExactCanonicalHeaders_ReturnsIdentityMapping()
        {
            var result = CsvHeaderAliasResolver.SuggestMapping(["date", "amount", "currency_code"]);

            Assert.Equal("date", result["date"]);
            Assert.Equal("amount", result["amount"]);
            Assert.Equal("currency_code", result["currency_code"]);
        }

        [Fact]
        public void SuggestMapping_KnownAliases_MapsToCanonicalFields()
        {
            var result = CsvHeaderAliasResolver.SuggestMapping(["amt", "cur", "cat", "subcat", "desc", "tag", "fam", "txn_date"]);

            Assert.Equal("amount", result["amt"]);
            Assert.Equal("currency_code", result["cur"]);
            Assert.Equal("category", result["cat"]);
            Assert.Equal("subcategory", result["subcat"]);
            Assert.Equal("description", result["desc"]);
            Assert.Equal("tags", result["tag"]);
            Assert.Equal("families", result["fam"]);
            Assert.Equal("date", result["txn_date"]);
        }

        [Fact]
        public void SuggestMapping_UnknownColumn_NotIncludedInSuggestions()
        {
            var result = CsvHeaderAliasResolver.SuggestMapping(["totally_unknown_column"]);

            Assert.Empty(result);
        }

        [Fact]
        public void SuggestMapping_AmbiguousDuplicateAliases_OnlyFirstMatchSuggested()
        {
            var result = CsvHeaderAliasResolver.SuggestMapping(["amt", "sum"]);

            Assert.Equal("amount", result["amt"]);
            Assert.False(result.ContainsKey("sum"));
        }

        [Fact]
        public void SuggestMapping_CaseInsensitive()
        {
            var result = CsvHeaderAliasResolver.SuggestMapping(["AMT", "Cur"]);

            Assert.Equal("amount", result["AMT"]);
            Assert.Equal("currency_code", result["Cur"]);
        }

        // ── IsExactHeaderMatch ────────────────────────────────────────────────

        [Fact]
        public void IsExactHeaderMatch_AllRequiredHeadersPresent_ReturnsTrue()
        {
            Assert.True(CsvHeaderAliasResolver.IsExactHeaderMatch(["date", "amount", "currency_code", "category"]));
        }

        [Fact]
        public void IsExactHeaderMatch_MissingOne_ReturnsFalse()
        {
            Assert.False(CsvHeaderAliasResolver.IsExactHeaderMatch(["date", "amount"]));
        }
    }
}
