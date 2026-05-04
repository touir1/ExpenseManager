using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IApplicationService
    {
        Task<ApplicationDto?> GetApplicationByCodeAsync(string applicationCode);
    }
}
