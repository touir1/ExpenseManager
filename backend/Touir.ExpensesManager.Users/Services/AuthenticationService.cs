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
using System.Security.Cryptography;
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
        private readonly IApplicationRepository _applicationRepository;
        private readonly IRoleRepository _roleRepository;

        private readonly string _verifyEmailUrl;

        public AuthenticationService(
            IOptions<JwtAuthOptions> jwtAuthOptions,
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IEmailHelper emailHelper,
            ICryptographyHelper cryptographyHelper,
            IUserRepository userRepository,
            IAuthenticationRepository authenticationRepository,
            IApplicationRepository applicationRepository,
            IRoleRepository roleRepository) 
        {
            _secretKey = jwtAuthOptions.Value.SecretKey;
            _issuer = jwtAuthOptions.Value.Issuer;
            _audience = jwtAuthOptions.Value.Audience;
            _expiryInMinutes = jwtAuthOptions.Value.ExpiryInMinutes;

            _emailHelper = emailHelper;
            _cryptographyHelper = cryptographyHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
            _applicationRepository = applicationRepository;
            _roleRepository = roleRepository;

            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
        }
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return null;
            var authentication = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if (authentication == null || 
                !_cryptographyHelper.VerifyPasswordHash(password, authentication.HashPasswordBytes, authentication.HashSaltBytes))
                return null;

            return user;
        }

        public string GenerateJwtToken(int userId, string userEmail)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Email, userEmail),
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
                // Validate the token
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Token validation succeeded
                validationResult.IsValid = true;
                validationResult.SecurityToken = validatedToken;
            }
            catch (SecurityTokenException ex)
            {
                // Token validation failed
                validationResult.IsValid = false;
                validationResult.Exception = ex;
            }

            return validationResult;
        }

        public async Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email, string? applicationCode)
        {
            IList<string> errors = new List<string>();
            
            if(email != null && !_emailHelper.VerifyEmail(email))
                errors.Add("email format is invalid");

            if (errors.Count() > 0)
                return errors;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if(user != null)
            {
                if (user.IsEmailValidated)
                    errors.Add("email is already used");
                else
                    errors.Add("there's already a registration process ongoing, please check you mailbox to validate the email");
            }
            else
            {
                string emailValidationHash;
                ISet<string> existingEmailValidationHashes = new HashSet<string>(await _userRepository.GetUsedEmailValidationHashesAsync());
                do
                {
                    emailValidationHash = Guid.NewGuid().ToString();
                }
                while (existingEmailValidationHashes.Contains(emailValidationHash));

                user = await _userRepository.CreateUserAsync(new User
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

                if(applicationCode != null)
                {
                    Application app = await _applicationRepository.GetApplicationByCodeAsync(applicationCode);
                    if(app != null)
                    {
                        var role = await _roleRepository.GetDefaultRoleByApplicationIdAsync(app.Id);
                        if(role != null)
                        {
                            await _roleRepository.AssignRoleToUserAsync(role.Id, user.Id);
                        }
                    }
                }

                try
                {
                    string verificationLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(emailValidationHash)}&s={HttpUtility.UrlEncode(email)}&app_code={HttpUtility.UrlEncode(applicationCode)}";
                    Console.WriteLine($"user: {user.FirstName} {user.LastName}, verifLink: {verificationLink}");
                    string emailVerificationHtml = _emailHelper.GetEmailTemplate(EmailHTMLTemplate.EmailVerification.Key, new Dictionary<string, string> {
                        { EmailHTMLTemplate.EmailVerification.Variables.VerificationLink, verificationLink },
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
            
            return errors;
        }

        public async Task<bool> ValidateEmailAsync(string emailVerificationHash, string email)
        {
            if(!_emailHelper.VerifyEmail(email))
                return false;
            if(!Guid.TryParse(emailVerificationHash, out _)) 
                return false;
            
            return await _userRepository.ValidateEmail(emailVerificationHash, email);
        }

        

        public async Task<bool> ChangePasswordAsync(string email, string oldPassword, string newPassword)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;
            
            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);
            if(auth == null) 
                return false;

            // verify old password
            if (!_cryptographyHelper.VerifyPasswordHash(oldPassword, auth.HashPasswordBytes, auth.HashSaltBytes))
                return false;

            // update password
            auth.HashSaltBytes = _cryptographyHelper.GenerateRandomSalt();
            auth.HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, auth.HashSaltBytes);
            return await _authenticationRepository.UpdateAuthenticationAsync(auth);
        }

        public async Task<bool> ResetPasswordAsync(string email, string verificationHash, string newPassword)
        {
            // verify hash before changing password
            if (!(await ValidateEmailAsync(verificationHash, email)))
                return false;

            User? user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return false;

            Authentication? auth = await _authenticationRepository.GetAuthenticationByUserIdAsync(user.Id);

            // salt for password hash
            byte[] salt = _cryptographyHelper.GenerateRandomSalt();

            // new user so we need to create the authentication
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

            // if auth exists just change the password and reset temporary password flag
            auth.IsTemporaryPassword = false;
            auth.HashSaltBytes = salt;
            auth.HashPasswordBytes = _cryptographyHelper.GeneratePasswordHash(newPassword, salt);

            return await _authenticationRepository.UpdateAuthenticationAsync(auth, resetHash: true);
        }

        public async Task<bool> RequestPasswordResetAsync(string email)
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
                string resetLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(resetHash)}&s={HttpUtility.UrlEncode(email)}";
                //string emailResetHtml = _emailHelper.GetEmailTemplate(EmailHTMLTemplate.PasswordReset.Key, new Dictionary<string, string> {
                //    { EmailHTMLTemplate.PasswordReset.Variables.ResetLink, resetLink },
                //});
                //_emailHelper.SendEmail(recipientTo: email, emailSubject: "[Expenses Manager] Password Reset", isHTML: true, emailBody: emailResetHtml);
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
