using Touir.ExpensesManager.Users.Models;
using Microsoft.IdentityModel.Tokens;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        string GenerateJwtToken(int userId, string userEmail);
        TokenValidationResult ValidateToken(string token);
        Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email, string? applicationCode);
        Task<bool> ValidateEmailAsync(string emailVerificationHash, string email);
        Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string verificationHash, string newPassword);

        Task<bool> RequestPasswordResetAsync(string email);
    }
}
