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

namespace com.touir.expenses.Users.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authenticationRepository;

        public AuthenticationService(
            IOptions<JwtAuthOptions> jwtAuthOptions, 
            IUserRepository userRepository, 
            IAuthenticationRepository authenticationRepository) 
        {
            _secretKey = jwtAuthOptions.Value.SecretKey;
            _issuer = jwtAuthOptions.Value.Issuer;
            _audience = jwtAuthOptions.Value.Audience;
            _expiryInMinutes = jwtAuthOptions.Value.ExpiryInMinutes;

            _userRepository = userRepository;
            _authenticationRepository = authenticationRepository;
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
            throw new NotImplementedException();
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
