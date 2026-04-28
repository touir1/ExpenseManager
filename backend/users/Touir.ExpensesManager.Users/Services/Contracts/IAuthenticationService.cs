using Touir.ExpensesManager.Users.Controllers.EO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<UserEo?> AuthenticateAsync(string email, string password);
    }
}
