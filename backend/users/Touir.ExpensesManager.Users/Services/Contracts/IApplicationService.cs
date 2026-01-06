using Touir.ExpensesManager.Users.Controllers.EO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IApplicationService
    {
        Task<ApplicationEo> GetApplicationByCodeAsync(string applicationCode);
    }
}
