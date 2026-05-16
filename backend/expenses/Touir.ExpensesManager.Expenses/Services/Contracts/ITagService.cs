using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ITagService
    {
        Task<TagListDto> GetVisibleAsync(int userId);
        Task<TagDto> UseTagAsync(string name, int userId);
        Task<bool> RemoveTagAsync(int tagId, int userId);
    }
}
