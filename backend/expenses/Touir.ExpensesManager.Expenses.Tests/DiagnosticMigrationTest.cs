using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Xunit.Abstractions;

namespace Touir.ExpensesManager.Expenses.Tests
{
    public class DiagnosticMigrationTest
    {
        private readonly ITestOutputHelper _output;
        public DiagnosticMigrationTest(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Check_Migrations_And_Schema()
        {
            var asm = typeof(ExpensesDbContext).Assembly;
            _output.WriteLine("=== MIGRATION TYPES IN ASSEMBLY (GetTypes) ===");
            foreach (var t in asm.GetTypes().Where(t => typeof(Migration).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var attr = t.GetCustomAttribute<MigrationAttribute>();
                _output.WriteLine($"  {t.Name} -> attr={(attr?.Id ?? "NULL")} IsSubclassOf={t.IsSubclassOf(typeof(Migration))} IsGenericTypeDef={t.IsGenericTypeDefinition}");
            }

            _output.WriteLine("=== GetLoadableDefinedTypes subclass check ===");
            try {
                var definedTypes = asm.DefinedTypes;
                var constructible = definedTypes.Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);
                var migrationSubclasses = constructible.Where(t => t.IsSubclassOf(typeof(Migration))).ToList();
                _output.WriteLine($"  Count via DefinedTypes: {migrationSubclasses.Count}");
                foreach (var t in migrationSubclasses)
                    _output.WriteLine($"  {t.Name}");
            } catch (ReflectionTypeLoadException ex) {
                _output.WriteLine($"  ReflectionTypeLoadException: {ex.LoaderExceptions.FirstOrDefault()?.Message}");
            }

            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ExpensesDbContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new ExpensesDbContext(options);

            var migrationsAssembly = ctx.GetService<IMigrationsAssembly>();
            _output.WriteLine($"=== IMigrationsAssembly.Migrations count = {migrationsAssembly.Migrations.Count} ===");
            foreach (var kvp in migrationsAssembly.Migrations)
                _output.WriteLine($"  {kvp.Key} -> {kvp.Value.Name}");

            ctx.Database.Migrate();

            var applied = ctx.Database.GetAppliedMigrations().ToList();
            _output.WriteLine("=== APPLIED MIGRATIONS ===");
            foreach (var m in applied) _output.WriteLine(m);

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT MigrationId FROM \"__EFMigrationsHistory\" ORDER BY MigrationId";
            var reader = cmd.ExecuteReader();
            _output.WriteLine("=== __EFMigrationsHistory ===");
            while (reader.Read())
                _output.WriteLine($"  {reader[0]}");
            reader.Close();

            cmd.CommandText = "PRAGMA table_info(\"USR_Users\")";
            reader = cmd.ExecuteReader();
            _output.WriteLine("=== USR_Users COLUMNS ===");
            while (reader.Read())
                _output.WriteLine($"{reader[1]} ({reader[2]}) default={reader[4]}");

            Assert.True(applied.Any(m => m.Contains("AddUserIsAdmin")), "AddUserIsAdmin migration not applied");
        }
    }
}
