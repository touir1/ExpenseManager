using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;

        public AuthenticationService(
            ICryptographyHelper cryptographyHelper,
            IUserRepository userRepository,
            IAuthenticationRepository authenticationRepository)
        {
            _cryptographyHelper = cryptographyHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
        }

        public async Task<UserEo?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return null;
            var authentication = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if (authentication == null ||
                !_cryptographyHelper.VerifyPasswordHash(password, authentication.HashPasswordBytes, authentication.HashSaltBytes))
                return null;

            return new UserEo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                FamilyId = user.FamilyId,
                CreatedAt = user.CreatedAt,
                LastUpdatedAt = user.LastUpdatedAt,
                IsDisabled = user.IsDisabled
            };
        }
    }
}
