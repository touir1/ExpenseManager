using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit.Abstractions;

namespace Touir.ExpensesManager.Expenses.Tests
{
    public class DiagAttrTest
    {
        private readonly ITestOutputHelper _out;
        public DiagAttrTest(ITestOutputHelper o) => _out = o;
        [Fact]
        public void Check_MigrationAttribute_Properties()
        {
            var props = typeof(MigrationAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in props)
                _out.WriteLine($"  {p.Name}: {p.PropertyType.Name}");
            var attr = new MigrationAttribute("test123");
            _out.WriteLine($"  attr.Id = {attr.Id}");
        }
    }
}
