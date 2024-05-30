using com.touir.expenses.Users.Models;
using Microsoft.IdentityModel.Tokens;

namespace com.touir.expenses.Users.Services.Contracts
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        string GenerateJwtToken(int userId, string userEmail);
        TokenValidationResult ValidateToken(string token);
        Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email);
        Task<bool> VerifyEmailAsync(string emailVerificationHash, string email);
        Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string verificationHash, string newPassword);

    }
}
