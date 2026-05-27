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
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == lowerEmail && !u.IsDeleted);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.Email = user.Email!.ToLowerInvariant();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool?> DeleteUserAsync(User user)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IList<string>> GetUsedEmailValidationHashesAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.EmailValidationHash != null && !u.IsEmailValidated && !u.IsDeleted)
                .Select(s => s.EmailValidationHash!)
                .ToListAsync();
        }

        public async Task<bool> ValidateEmail(string emailVerificationHash, string email)
        {
            var lowerEmail = email.ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == lowerEmail &&
                    u.EmailValidationHash == emailVerificationHash &&
                    !u.IsDeleted);
            if (user != null)
            {
                user.IsEmailValidated = true;
                await _context.SaveChangesAsync();
            }

            return user != null;
        }

        public async Task<User?> ValidateEmailAsync(string emailVerificationHash, string email)
        {
            var lowerEmail = email.ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == lowerEmail &&
                    u.EmailValidationHash == emailVerificationHash &&
                    !u.IsDeleted);
            if (user == null)
                return null;
            if (user.EmailValidationHashExpiresAt.HasValue && user.EmailValidationHashExpiresAt.Value < DateTime.UtcNow)
                return null;
            user.IsEmailValidated = true;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateEmailValidationHashAsync(int userId, string newHash, DateTime expiresAt)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return;
            user.EmailValidationHash = newHash;
            user.EmailValidationHashExpiresAt = expiresAt;
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(string? search, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLowerInvariant();
                query = query.Where(u =>
                    u.Email.Contains(lower) ||
                    u.FirstName.ToLower().Contains(lower) ||
                    u.LastName.ToLower().Contains(lower));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, total);
        }

        public async Task<bool> DisableAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return false;
            user.IsDisabled = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EnableAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            if (user == null) return false;
            user.IsDisabled = false;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
