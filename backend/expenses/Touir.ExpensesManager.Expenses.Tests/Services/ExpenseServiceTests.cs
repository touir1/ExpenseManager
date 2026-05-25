using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class ExpenseServiceTests
    {
        private static ExpenseService CreateService(
            IExpenseRepository? repo = null,
            IExpenseAuditService? audit = null,
            IFamilyRepository? familyRepo = null,
            ITagRepository? tagRepo = null,
            ICurrencyRateService? rateService = null,
            ICurrencyRepository? currencyRepo = null)
        {
            return new ExpenseService(
                repo ?? Mock.Of<IExpenseRepository>(),
                audit ?? Mock.Of<IExpenseAuditService>(),
                familyRepo ?? Mock.Of<IFamilyRepository>(),
                tagRepo ?? Mock.Of<ITagRepository>(),
                rateService ?? Mock.Of<ICurrencyRateService>(),
                currencyRepo ?? Mock.Of<ICurrencyRepository>());
        }

        private static Expense MakeExpense(long id = 1, int userId = 42) => new()
        {
            Id = id,
            UserId = userId,
            Amount = 100m,
            CurrencyId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            CreatedFromId = 1
        };

        // ── AddAsync ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_CallsRepositoryAdd()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>()))
                .ReturnsAsync((Expense e) => e);

            await CreateService(repo.Object).AddAsync(
                new CreateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 1, sourceId: 1);

            repo.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_WritesAudit()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>()))
                .ReturnsAsync((Expense e) => e);
            var audit = new Mock<IExpenseAuditService>();

            await CreateService(repo.Object, audit.Object).AddAsync(
                new CreateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 1, sourceId: 1);

            audit.Verify(a => a.WriteAddAuditAsync(It.IsAny<Expense>(), 1, 1, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ReturnsDtoWithCorrectAmount()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>()))
                .ReturnsAsync((Expense e) =>
                {
                    e.Currency = new Currency { Id = e.CurrencyId, Code = "X", Name = "X", Symbol = "X", Decimals = 2 };
                    return e;
                });

            var result = await CreateService(repo.Object).AddAsync(
                new CreateExpenseRequest { Amount = 75.50m, CurrencyId = 2, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 5, sourceId: 1);

            Assert.Equal(75.50m, result.Amount);
            Assert.Equal(2, result.Currency?.Id);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenExpenseNotFound()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).UpdateAsync(
                1,
                new UpdateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 1, sourceId: 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_CallsRepositoryUpdate_WhenFound()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            await CreateService(repo.Object).UpdateAsync(
                1,
                new UpdateExpenseRequest { Amount = 200m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 42, sourceId: 1);

            repo.Verify(r => r.UpdateAsync(It.IsAny<Expense>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WritesUpdateAudit()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);
            var audit = new Mock<IExpenseAuditService>();

            await CreateService(repo.Object, audit.Object).UpdateAsync(
                1,
                new UpdateExpenseRequest { Amount = 200m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) },
                userId: 42, sourceId: 1);

            audit.Verify(a => a.WriteUpdateAuditAsync(
                It.IsAny<Expense>(), It.IsAny<Expense>(), 42, 1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesModifiedFields()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).UpdateAsync(
                1,
                new UpdateExpenseRequest { Amount = 999m, CurrencyId = 3, Date = new DateOnly(2025, 1, 1) },
                userId: 42, sourceId: 1);

            Assert.NotNull(result);
            Assert.Equal(999m, result!.Amount);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).DeleteAsync(1, userId: 1, sourceId: 1);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenFound()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).DeleteAsync(1, userId: 42, sourceId: 1);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_CallsSoftDelete()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            await CreateService(repo.Object).DeleteAsync(1, userId: 42, sourceId: 1);

            repo.Verify(r => r.SoftDeleteAsync(expense), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WritesDeleteAudit()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);
            var audit = new Mock<IExpenseAuditService>();

            await CreateService(repo.Object, audit.Object).DeleteAsync(1, userId: 42, sourceId: 1);

            audit.Verify(a => a.WriteDeleteAuditAsync(expense, 42, 1, It.IsAny<string>()), Times.Once);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).GetByIdAsync(1, userId: 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto()
        {
            var expense = MakeExpense(id: 7);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(7, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).GetByIdAsync(7, userId: 42);

            Assert.NotNull(result);
            Assert.Equal(7, result!.Id);
            Assert.Equal(100m, result.Amount);
        }

        // ── WriteExpenseTagsAsync (via AddAsync) ─────────────────────────────────

        [Fact]
        public async Task AddAsync_ThrowsForbidden_WhenTagNotVisible()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync((Expense e) => e);
            var tagRepo = new Mock<ITagRepository>();
            tagRepo.Setup(r => r.IsVisibleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<FamilyForbiddenException>(() =>
                CreateService(repo: repo.Object, tagRepo: tagRepo.Object).AddAsync(
                    new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), TagIds = [99] },
                    userId: 1, sourceId: 1));
        }

        // ── WriteAttributionsAsync (via AddAsync) ────────────────────────────────

        [Fact]
        public async Task AddAsync_WithVisibleTags_AddsTagsToExpense()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync((Expense e) => e);
            var tagRepo = new Mock<ITagRepository>();
            tagRepo.Setup(r => r.IsVisibleAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            tagRepo.Setup(r => r.EnsureUserTagAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            tagRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(
                [new Touir.ExpensesManager.Expenses.Models.Tag { Id = 5, Name = "food" }]);

            var result = await CreateService(repo: repo.Object, tagRepo: tagRepo.Object).AddAsync(
                new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), TagIds = [5] },
                userId: 1, sourceId: 1);

            Assert.Single(result.Tags);
            tagRepo.Verify(r => r.EnsureUserTagAsync(1, 5), Times.Once);
        }

        [Fact]
        public async Task AddAsync_NullFamilyIds_NoDefaultFamily_SkipsAttributions()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync((Expense e) => e);
            var familyRepo = new Mock<IFamilyRepository>();
            familyRepo.Setup(r => r.GetDefaultFamilyForUserAsync(It.IsAny<int>())).ReturnsAsync((Family?)null);

            // should complete without error
            var result = await CreateService(repo: repo.Object, familyRepo: familyRepo.Object).AddAsync(
                new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), FamilyIds = null },
                userId: 1, sourceId: 1);

            Assert.NotNull(result);
            familyRepo.Verify(r => r.AddAttributionsAsync(It.IsAny<IEnumerable<ExpenseFamilyAttribution>>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_NullFamilyIds_WithDefaultFamily_AddsAttribution()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync((Expense e) => e);
            var familyRepo = new Mock<IFamilyRepository>();
            familyRepo.Setup(r => r.GetDefaultFamilyForUserAsync(1)).ReturnsAsync(new Family { Id = 10, Name = "Default", IsDefault = true });
            familyRepo.Setup(r => r.IsMemberAsync(10, 1)).ReturnsAsync(true);

            var result = await CreateService(repo: repo.Object, familyRepo: familyRepo.Object).AddAsync(
                new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), FamilyIds = null },
                userId: 1, sourceId: 1);

            Assert.NotNull(result);
            familyRepo.Verify(r => r.AddAttributionsAsync(It.Is<IEnumerable<ExpenseFamilyAttribution>>(
                attrs => attrs.Any(a => a.FamilyId == 10))), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ExplicitFamilyIds_ThrowsForbidden_WhenNotMember()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync((Expense e) => e);
            var familyRepo = new Mock<IFamilyRepository>();
            familyRepo.Setup(r => r.IsMemberAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<FamilyForbiddenException>(() =>
                CreateService(repo: repo.Object, familyRepo: familyRepo.Object).AddAsync(
                    new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow), FamilyIds = [7] },
                    userId: 1, sourceId: 1));
        }

        // ── MapToDto — non-null Category/Subcategory paths ────────────────────────

        [Fact]
        public async Task GetByIdAsync_MapsCategory_WhenSet()
        {
            var expense = MakeExpense(id: 3);
            expense.Category = new Touir.ExpensesManager.Expenses.Models.Category { Id = 5, Name = "Food" };
            expense.Subcategory = new Touir.ExpensesManager.Expenses.Models.Category { Id = 6, Name = "Lunch" };

            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(3, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).GetByIdAsync(3, userId: 42);

            Assert.NotNull(result!.Category);
            Assert.Equal(5, result.Category!.Id);
            Assert.NotNull(result.Subcategory);
            Assert.Equal(6, result.Subcategory!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_MapsTagsFromExpenseTags_WhenNoExplicitTags()
        {
            var expense = MakeExpense(id: 4);
            var tag = new Touir.ExpensesManager.Expenses.Models.Tag { Id = 7, Name = "travel" };
            expense.ExpenseTags = [new Touir.ExpensesManager.Expenses.Models.ExpenseTag { ExpenseId = 4, TagId = 7, Tag = tag }];

            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(4, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).GetByIdAsync(4, userId: 42);

            Assert.Single(result!.Tags);
            Assert.Equal("travel", result.Tags.First().Name);
        }

        // ── GetPagedAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var items = new[] { MakeExpense(1), MakeExpense(2) };
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), 42))
                .ReturnsAsync((items.AsEnumerable(), 2));

            var result = await CreateService(repo.Object).GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 10 }, userId: 42);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.TotalPages);
        }

        [Fact]
        public async Task GetPagedAsync_CalculatesTotalPagesCorrectly()
        {
            var items = Enumerable.Range(1, 5).Select(i => MakeExpense(i)).ToArray();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), It.IsAny<int>()))
                .ReturnsAsync((items.AsEnumerable(), 11));

            var result = await CreateService(repo.Object).GetPagedAsync(
                new ExpenseFilterDto { Page = 1, PageSize = 5 }, userId: 1);

            Assert.Equal(11, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
        }
    }
}
