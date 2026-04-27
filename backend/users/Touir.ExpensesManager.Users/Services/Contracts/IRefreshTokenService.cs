namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAsync(int userId, bool rememberMe);
        Task<(bool isValid, int userId)> ValidateAsync(string token);
        Task RevokeAsync(string token);
        Task RevokeAllForUserAsync(int userId);
    }
}
