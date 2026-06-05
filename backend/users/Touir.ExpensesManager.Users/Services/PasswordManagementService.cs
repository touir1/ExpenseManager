using System.Net.Mail;
using System.Text.Json;
using System.Web;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Touir.ExpensesManager.Users.Services
{
    public class PasswordManagementService : IPasswordManagementService
    {
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly string _resetPasswordBaseUrl;
        private readonly int _passwordResetExpiryInHours;

        public PasswordManagementService(
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            ICryptographyHelper cryptographyHelper,
            IUserRepository userRepository,
            IAuthenticationRepository authenticationRepository,
            IOutboxRepository outboxRepository)
        {
            _cryptographyHelper = cryptographyHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
            _outboxRepository = outboxRepository;
            _resetPasswordBaseUrl = authServiceOptions.Value.ResetPasswordBaseUrl;
            _passwordResetExpiryInHours = authServiceOptions.Value.PasswordResetExpiryInHours;
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
            var updated = await _authenticationRepository.UpdateAuthenticationAsync(auth);

            if (updated)
                await EnqueuePasswordChangedAsync(user.Id, user.Email, user.FirstName);

            return updated;
        }

        public async Task<bool> CreatePasswordAsync(string email, string verificationHash, string newPassword)
        {
            if (!IsValidEmail(email))
                return false;
            if (!Guid.TryParse(verificationHash, out _))
                return false;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            if (!await _userRepository.ValidateEmail(verificationHash, email))
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

        public async Task<bool> ResetPasswordAsync(string email, string resetHash, string newPassword)
        {
            if (!IsValidEmail(email))
                return false;
            if (!Guid.TryParse(resetHash, out _))
                return false;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if (auth == null)
                return false;

            if (auth.PasswordResetHash != resetHash ||
                !auth.PasswordResetRequestedAt.HasValue ||
                (DateTime.UtcNow - auth.PasswordResetRequestedAt.Value).TotalHours > _passwordResetExpiryInHours)
                return false;

            byte[] salt = _cryptographyHelper.GenerateRandomSalt();
            auth.IsTemporaryPassword = false;
            auth.HashSaltBytes = salt;
            auth.HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, salt);
            auth.PasswordResetHash = null;
            auth.PasswordResetRequestedAt = null;
            var updated = await _authenticationRepository.UpdateAuthenticationAsync(auth);

            if (updated)
                await EnqueuePasswordChangedAsync(user.Id, user.Email, user.FirstName);

            return updated;
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

            string resetLink = $"{_resetPasswordBaseUrl.TrimEnd('/')}?email={HttpUtility.UrlEncode(email)}&h={HttpUtility.UrlEncode(resetHash)}";
            await _outboxRepository.EnqueueAsync(new OutboxEvent
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = UserEventType.PasswordResetRequested,
                Payload = JsonSerializer.Serialize(new UserNotificationEventMessage
                {
                    EventType = UserEventType.PasswordResetRequested,
                    UserId = user.Id,
                    Email = email,
                    FirstName = user.FirstName,
                    ResetLink = resetLink
                }),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            });

            return true;
        }

        private async Task EnqueuePasswordChangedAsync(int userId, string email, string? firstName)
        {
            await _outboxRepository.EnqueueAsync(new OutboxEvent
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = UserEventType.PasswordChanged,
                Payload = JsonSerializer.Serialize(new UserNotificationEventMessage
                {
                    EventType = UserEventType.PasswordChanged,
                    UserId = userId,
                    Email = email,
                    FirstName = firstName
                }),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            });
        }

        private static bool IsValidEmail(string email)
        {
            try { _ = new MailAddress(email); return true; }
            catch { return false; }
        }
    }
}
