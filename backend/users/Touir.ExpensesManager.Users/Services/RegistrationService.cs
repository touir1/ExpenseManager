using System.Net.Mail;
using System.Text.Json;
using System.Web;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Touir.ExpensesManager.Users.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUserRoleAssignmentService _userRoleAssignmentService;
        private readonly string _verifyEmailUrl;
        private readonly int _verificationExpiryHours;

        public RegistrationService(
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IUserRepository userRepository,
            IOutboxRepository outboxRepository,
            IUserRoleAssignmentService userRoleAssignmentService)
        {
            _userRepository = userRepository;
            _outboxRepository = outboxRepository;
            _userRoleAssignmentService = userRoleAssignmentService;
            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
            _verificationExpiryHours = authServiceOptions.Value.EmailVerificationExpiryInHours;
        }

        public async Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email, string? applicationCode)
        {
            List<string> errors = [];

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                if (user.IsEmailValidated)
                {
                    errors.Add("email is already used");
                }
                else
                {
                    await ResendVerificationEmailAsync(email, applicationCode);
                }
            }
            else
            {
                await CreateAndRegisterUserAsync(firstname, lastname, email, applicationCode);
            }

            return errors;
        }

        public async Task<bool> ValidateEmailAsync(string emailVerificationHash, string email)
        {
            if (!IsValidEmail(email))
                return false;
            if (!Guid.TryParse(emailVerificationHash, out _))
                return false;

            var user = await _userRepository.ValidateEmailAsync(emailVerificationHash, email);
            if (user == null)
                return false;

            await _outboxRepository.EnqueueAsync(new OutboxEvent
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = UserEventType.Created,
                Payload = JsonSerializer.Serialize(new UserEventMessage
                {
                    EventType = UserEventType.Created,
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    FamilyId = user.FamilyId,
                    IsAdmin = false
                }),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            });

            return true;
        }

        public async Task<ResendResult> ResendVerificationEmailAsync(string email, string? applicationCode)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.IsEmailValidated)
                return ResendResult.NotFound;

            var newHash = await GenerateUniqueEmailValidationHashAsync();
            var expiresAt = DateTime.UtcNow.AddHours(_verificationExpiryHours);
            await _userRepository.UpdateEmailValidationHashAsync(user.Id, newHash, expiresAt);

            await EnqueueVerificationEmailAsync(user.Id, email, newHash, applicationCode);

            return ResendResult.Sent;
        }

        private async Task<string> GenerateUniqueEmailValidationHashAsync()
        {
            HashSet<string> existingHashes = new(await _userRepository.GetUsedEmailValidationHashesAsync());
            string hash;
            do
            {
                hash = Guid.NewGuid().ToString();
            }
            while (existingHashes.Contains(hash));
            return hash;
        }

        private async Task CreateAndRegisterUserAsync(string firstname, string lastname, string email, string? applicationCode)
        {
            var emailValidationHash = await GenerateUniqueEmailValidationHashAsync();

            User user = await _userRepository.CreateUserAsync(new User
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsEmailValidated = false,
                IsDisabled = false,
                LastUpdatedAt = DateTime.UtcNow,
                EmailValidationHash = emailValidationHash,
                EmailValidationHashExpiresAt = DateTime.UtcNow.AddHours(_verificationExpiryHours)
            });

            await _userRoleAssignmentService.TryAssignDefaultRoleAsync(applicationCode, user);

            try
            {
                await EnqueueVerificationEmailAsync(user.Id, email, emailValidationHash, applicationCode);
            }
            catch (Exception exception)
            {
                await _userRepository.DeleteUserAsync(user);
                Console.WriteLine(exception.ToString());
                throw;
            }
        }

        private async Task EnqueueVerificationEmailAsync(int userId, string email, string emailValidationHash, string? applicationCode)
        {
            string verificationLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(emailValidationHash)}&s={HttpUtility.UrlEncode(email)}&app_code={HttpUtility.UrlEncode(applicationCode)}";
            await _outboxRepository.EnqueueAsync(new OutboxEvent
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = UserEventType.EmailVerificationRequested,
                Payload = JsonSerializer.Serialize(new UserNotificationEventMessage
                {
                    EventType = UserEventType.EmailVerificationRequested,
                    UserId = userId,
                    Email = email,
                    VerificationLink = verificationLink,
                    AppCode = applicationCode
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
