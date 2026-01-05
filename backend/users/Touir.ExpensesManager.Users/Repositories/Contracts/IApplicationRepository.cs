using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IApplicationRepository
    {
        Task<Application> GetApplicationByCodeAsync(string applicationCode);
    }
}
