using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IApplicationService
    {
        Task<Application> GetApplicationByCodeAsync(string applicationCode);
    }
}
