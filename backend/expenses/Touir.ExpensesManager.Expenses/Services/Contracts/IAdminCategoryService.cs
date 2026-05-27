using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IAdminCategoryService
    {
        Task<IEnumerable<AdminCategoryDto>> GetAllAsync();
        Task<AdminCategoryDto> AddCategoryAsync(string name, string? description);
        Task<AdminCategoryDto> UpdateCategoryAsync(int id, string name, string? description);
        Task ArchiveCategoryAsync(int id);
        Task UnarchiveCategoryAsync(int id);
        Task<AdminSubcategoryDto> AddSubcategoryAsync(int parentId, string name, string? description);
        Task<AdminSubcategoryDto> UpdateSubcategoryAsync(int id, string name, string? description);
        Task ArchiveSubcategoryAsync(int id);
        Task UnarchiveSubcategoryAsync(int id);
    }
}
