using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;
using System.Web;

namespace Touir.ExpensesManager.Users.Services
{
    public class PasswordManagementService : IPasswordManagementService
    {
        private readonly IEmailHelper _emailHelper;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly string _verifyEmailUrl;

        public PasswordManagementService(
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IEmailHelper emailHelper,
            ICryptographyHelper cryptographyHelper,
            IUserRepository userRepository,
            IAuthenticationRepository authenticationRepository)
        {
            _emailHelper = emailHelper;
            _cryptographyHelper = cryptographyHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
        }

        public async Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if (auth == null)
                return false;

            if (!_cryptographyHelper.VerifyPasswordHash(oldPassword, auth.HashPasswordBytes, auth.HashSaltBytes))
                return false;

            auth.HashSaltBytes = _cryptographyHelper.GenerateRandomSalt();
            auth.HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, auth.HashSaltBytes);
            return await _authenticationRepository.UpdateAuthenticationAsync(auth);
        }

        public async Task<bool> ResetPasswordAsync(string email, string verificationHash, string newPassword)
        {
            if (!_emailHelper.VerifyEmail(email))
                return false;
            if (!Guid.TryParse(verificationHash, out _))
                return false;
            if (!await _userRepository.ValidateEmail(verificationHash, email))
                return false;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);

            byte[] salt = _cryptographyHelper.GenerateRandomSalt();

            if (auth == null)
            {
                auth = new Authentication
                {
                    User = user,
                    HashSaltBytes = salt,
                    HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, salt),
                    IsTemporaryPassword = false
                };
                return await _authenticationRepository.CreateAuthenticationAsync(auth, resetHash: true);
            }

            auth.IsTemporaryPassword = false;
            auth.HashSaltBytes = salt;
            auth.HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, salt);

            return await _authenticationRepository.UpdateAuthenticationAsync(auth, resetHash: true);
        }

        public async Task<bool> RequestPasswordResetAsync(string email, string appCode)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || !user.IsEmailValidated)
                return false;
            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if (auth == null)
                return false;
            string resetHash = Guid.NewGuid().ToString();
            auth.PasswordResetHash = resetHash;
            auth.PasswordResetRequestedAt = DateTime.UtcNow;
            if (!await _authenticationRepository.UpdateAuthenticationAsync(auth))
                return false;
            try
            {
                string resetLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(resetHash)}&s={HttpUtility.UrlEncode(email)}&app_code={HttpUtility.UrlEncode(appCode)}";
                string emailResetHtml = _emailHelper.GetEmailTemplate(EmailHtmlTemplate.PasswordReset.Key, new Dictionary<string, string> {
                    { EmailHtmlTemplate.PasswordReset.Variables.ResetLink, resetLink },
                });
                _emailHelper.SendEmail(recipientTo: email, emailSubject: "[Expenses Manager] Password Reset", isHTML: true, emailBody: emailResetHtml);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString()); // to change later: logging implementation
                return false;
            }
            return true;
        }
    }
}
