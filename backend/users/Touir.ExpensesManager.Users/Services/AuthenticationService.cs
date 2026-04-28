using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Touir.ExpensesManager.Users.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        private readonly IEmailHelper _emailHelper;
        private readonly ICryptographyHelper _cryptographyHelper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IUserRoleAssignmentService _userRoleAssignmentService;

        private readonly string _verifyEmailUrl;

        public AuthenticationService(
            IOptions<JwtAuthOptions> jwtAuthOptions,
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IEmailHelper emailHelper,
            ICryptographyHelper cryptographyHelper,
            IUserRepository userRepository,
            IAuthenticationRepository authenticationRepository,
            IUserRoleAssignmentService userRoleAssignmentService)
        {
            _secretKey = jwtAuthOptions.Value.SecretKey;
            _issuer = jwtAuthOptions.Value.Issuer;
            _audience = jwtAuthOptions.Value.Audience;
            _expiryInMinutes = jwtAuthOptions.Value.ExpiryInMinutes;

            _emailHelper = emailHelper;
            _cryptographyHelper = cryptographyHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
            _userRoleAssignmentService = userRoleAssignmentService;

            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
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

        public string GenerateJwtToken(int userId, string userEmail, string? userFirstName, string? userLastName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.GivenName, userFirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, userLastName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryInMinutes),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.WriteToken(token);

            return jwtToken;
        }

        public TokenValidationResult ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,

                ValidateAudience = true,
                ValidAudience = _audience,

                ValidateLifetime = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuerSigningKey = true
            };

            var validationResult = new TokenValidationResult();
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                validationResult.IsValid = true;
                validationResult.SecurityToken = validatedToken;
            }
            catch (SecurityTokenException ex)
            {
                validationResult.IsValid = false;
                validationResult.Exception = ex;
            }

            return validationResult;
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

        public async Task<bool> ValidateEmailAsync(string emailVerificationHash, string email)
        {
            if (!_emailHelper.VerifyEmail(email))
                return false;
            if (!Guid.TryParse(emailVerificationHash, out _))
                return false;

            return await _userRepository.ValidateEmail(emailVerificationHash, email);
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
            if (!(await ValidateEmailAsync(verificationHash, email)))
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
