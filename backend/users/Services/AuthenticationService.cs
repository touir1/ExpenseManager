using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Infrastructure.Contracts;
using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using com.touir.expenses.Users.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace com.touir.expenses.Users.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        private readonly IEmailHelper _emailHelper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;

        private readonly string _verifyEmailUrl;

        public AuthenticationService(
            IOptions<JwtAuthOptions> jwtAuthOptions,
            IOptions<AuthenticationServiceOptions> authServiceOptions,
            IEmailHelper emailHelper,
            IUserRepository userRepository, 
            IAuthenticationRepository authenticationRepository) 
        {
            _secretKey = jwtAuthOptions.Value.SecretKey;
            _issuer = jwtAuthOptions.Value.Issuer;
            _audience = jwtAuthOptions.Value.Audience;
            _expiryInMinutes = jwtAuthOptions.Value.ExpiryInMinutes;

            _emailHelper = emailHelper;
            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;

            _verifyEmailUrl = authServiceOptions.Value.VerifyEmailBaseUrl;
        }
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                return null;
            var authentication = await _authenticationRepository.GetAuthenticationByIdAsync(user.Id);
            if (authentication == null || 
                !VerifyPasswordHash(password, authentication.HashPasswordBytes, authentication.HashSaltBytes))
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

        public async Task<IEnumerable<string>> RegisterNewUserAsync(string firstname, string lastname, string email)
        {
            IList<string> errors = new List<string>();
            
            if(firstname == null || String.Empty.Equals(firstname.Trim()))
                errors.Add("firstname is empty");
            if(lastname == null || String.Empty.Equals(lastname.Trim()))
                errors.Add("lastname is empty");
            
            if (email == null || String.Empty.Equals(email.Trim()))
                errors.Add("email is empty");
            if(email != null && !_emailHelper.ValidateEmail(email))
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
                IList<string> existingEmailValidationHashes = await _userRepository.GetUsedEmailValidationHashesAsync();
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

                try
                {
                    string verificationLink = $"{_verifyEmailUrl.TrimEnd('/')}?h={HttpUtility.UrlEncode(emailValidationHash)}&s={HttpUtility.UrlEncode(email)}";
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

        public async Task<bool> VerifyEmail(string emailVerificationHash, string source)
        {
            if(!_emailHelper.ValidateEmail(source))
                return false;
            if(!Guid.TryParse(emailVerificationHash, out _)) 
                return false;
            
            return await _userRepository.VerifyEmail(emailVerificationHash, source);
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                    return false;
            }
            return true;
        }
    }
}
