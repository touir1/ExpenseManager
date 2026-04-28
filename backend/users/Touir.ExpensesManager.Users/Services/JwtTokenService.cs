using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Touir.ExpensesManager.Users.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryInMinutes;

        public JwtTokenService(IOptions<JwtAuthOptions> jwtAuthOptions)
        {
            _secretKey = jwtAuthOptions.Value.SecretKey;
            _issuer = jwtAuthOptions.Value.Issuer;
            _audience = jwtAuthOptions.Value.Audience;
            _expiryInMinutes = jwtAuthOptions.Value.ExpiryInMinutes;
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

            return new JwtSecurityTokenHandler().WriteToken(token);
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
    }
}
