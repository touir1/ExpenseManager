using Microsoft.IdentityModel.Tokens;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(int userId, string userEmail, string? userFirstName, string? userLastName);
        TokenValidationResult ValidateToken(string token);
    }
}
