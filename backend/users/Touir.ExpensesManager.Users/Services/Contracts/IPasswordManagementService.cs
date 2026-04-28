namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IPasswordManagementService
    {
        Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string verificationHash, string newPassword);
        Task<bool> RequestPasswordResetAsync(string email, string appCode);
    }
}
