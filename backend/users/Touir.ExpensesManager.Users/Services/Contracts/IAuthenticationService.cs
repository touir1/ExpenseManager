using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<UserDto?> AuthenticateAsync(string email, string password);
    }
}
