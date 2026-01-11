using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public partial class ApplicationRepositoryTests
    {
        [Fact]
        public async Task GetApplicationByCodeAsync_ReturnsCorrectApplication()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Applications.AddRange(
                new Application { Id = 100, Code = "EXPENSES_MANAGER_2", Name = "Expenses Manager" },
                new Application { Id = 200, Code = "ANOTHER_APP", Name = "Another App" }
            );
            db.Context.SaveChanges();
            
            var repo = new ApplicationRepository(db.Context);
            var result = await repo.GetApplicationByCodeAsync("EXPENSES_MANAGER_2");
            
            Assert.NotNull(result);
            Assert.Equal("EXPENSES_MANAGER_2", result.Code);
            Assert.Equal("Expenses Manager", result.Name);
        }

        [Fact]
        public async Task GetApplicationByCodeAsync_ReturnsNull_WhenNotFound()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Applications.Add(
                new Application { Id = 100, Code = "EXPENSES_MANAGER_2", Name = "Expenses Manager" }
            );
            db.Context.SaveChanges();
            
            var repo = new ApplicationRepository(db.Context);
            var result = await repo.GetApplicationByCodeAsync("NON_EXISTENT");
            
            Assert.Null(result);
        }
    }
}
