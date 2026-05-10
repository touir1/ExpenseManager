using Touir.ExpensesManager.Users.Services;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRegistrationService
    {
        Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email, string? applicationCode);
        Task<bool> ValidateEmailAsync(string emailVerificationHash, string email);
        Task<ResendResult> ResendVerificationEmailAsync(string email, string? applicationCode);
    }
}
