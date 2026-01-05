namespace Touir.ExpensesManager.Users.Infrastructure.Contracts
{
    public interface ICryptographyHelper
    {
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        byte[] GeneratePasswordHash(string password, byte[] passwordSalt);
        byte[] GenerateRandomSalt();
    }
}
