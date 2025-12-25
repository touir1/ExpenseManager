using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace com.touir.expenses.Users.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly UsersAppDbContext _context;
        public AuthenticationRepository(UsersAppDbContext context)
        {
            _context = context;
        }

        public async Task<Authentication?> GetAuthenticationByIdAsync(int id)
        {
            return await _context.Authentications
                .FirstOrDefaultAsync(a => a.UserId == id);
        }

        public async Task<bool> CreateAuthenticationAsync(Authentication authentication, bool resetHash = false)
        {
            if (authentication == null || authentication.User == null) 
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Authentications.Add(authentication);

                if (resetHash)
                {
                    authentication.User.EmailValidationHash = null;
                    _context.Users.Update(authentication.User);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateAuthenticationAsync(Authentication authentication, bool resetHash = false)
        {
            if (authentication == null || authentication.User == null)
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Authentications.Update(authentication);

                if (resetHash)
                {
                    authentication.User.EmailValidationHash = null;
                    _context.Users.Update(authentication.User);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
