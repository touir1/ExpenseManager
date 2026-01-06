using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Touir.ExpensesManager.Users.Repositories
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
            var lowerEmail = email.ToLowerInvariant();
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == lowerEmail);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            user.Email = user.Email?.ToLowerInvariant();
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
            var lowerEmail = email.ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == lowerEmail &&
                    u.EmailValidationHash == emailValidationHash);
            if (user != null)
            {
                user.IsEmailValidated = true;
                await _context.SaveChangesAsync();
            }

            return user != null;
        }
    }
}
