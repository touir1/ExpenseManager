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
    public class RegistrationService : IRegistrationService
    {
        private readonly IEmailHelper _emailHelper;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleAssignmentService _userRoleAssignmentService;
        private readonly string _verifyEmailUrl;

        public RegistrationService(
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IEmailHelper emailHelper,
            IUserRepository userRepository,
            IUserRoleAssignmentService userRoleAssignmentService)
        {
            _emailHelper = emailHelper;
            _userRepository = userRepository;
            _userRoleAssignmentService = userRoleAssignmentService;
            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
        }

        public async Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email, string? applicationCode)
        {
            List<string> errors = [];

            if (email != null && !_emailHelper.VerifyEmail(email))
                errors.Add("email format is invalid");

            if (errors.Count > 0)
                return errors;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                errors.Add(user.IsEmailValidated
                    ? "email is already used"
                    : "there's already a registration process ongoing, please check you mailbox to validate the email");
            }
            else
            {
                await CreateAndRegisterUserAsync(firstname, lastname, email, applicationCode);
            }

            return errors;
        }

        public async Task<bool> ValidateEmailAsync(string emailVerificationHash, string email)
        {
            if (!_emailHelper.VerifyEmail(email))
                return false;
            if (!Guid.TryParse(emailVerificationHash, out _))
                return false;

            return await _userRepository.ValidateEmail(emailVerificationHash, email);
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

            var user = await _userRepository.CreateUserAsync(new User
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsEmailValidated = false,
                IsDisabled = false,
                LastUpdatedAt = DateTime.UtcNow,
                EmailValidationHash = emailValidationHash
            });

            await _userRoleAssignmentService.TryAssignDefaultRoleAsync(applicationCode, user);

            try
            {
                string verificationLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(emailValidationHash)}&s={HttpUtility.UrlEncode(email)}&app_code={HttpUtility.UrlEncode(applicationCode)}";
                Console.WriteLine($"user: {user?.FirstName} {user?.LastName}, verifLink: {verificationLink}");
                string emailVerificationHtml = _emailHelper.GetEmailTemplate(EmailHtmlTemplate.EmailVerification.Key, new Dictionary<string, string> {
                    { EmailHtmlTemplate.EmailVerification.Variables.VerificationLink, verificationLink },
                });
                _emailHelper.SendEmail(recipientTo: email, emailSubject: "[Expenses Manager] Email Verification", isHTML: true, emailBody: emailVerificationHtml);
            }
            catch (Exception exception)
            {
                await _userRepository.DeleteUserAsync(user);
                Console.WriteLine(exception.ToString()); // to change later: logging implementation
                throw;
            }
        }
    }
}
