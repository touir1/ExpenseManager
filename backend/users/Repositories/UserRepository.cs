using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace com.touir.expenses.Users.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersAppDbContext _context;
        public UserRepository(UsersAppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool?> DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IList<string>> GetUsedEmailValidationHashesAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.EmailValidationHash != null && u.IsEmailValidated == false)
                .Select(s => s.EmailValidationHash)
                .ToListAsync();
        }

        public async Task<bool> ValidateEmail(string emailValidationHash, string email)
        {
            var updatedRows = await _context.Users
                .Where(u =>
                    u.Email == email &&
                    u.EmailValidationHash == emailValidationHash &&
                    !u.IsEmailValidated)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsEmailValidated, true)
                    .SetProperty(x => x.EmailValidationHash, (string)null)
                );

            return updatedRows > 0;
        }
    }
}
