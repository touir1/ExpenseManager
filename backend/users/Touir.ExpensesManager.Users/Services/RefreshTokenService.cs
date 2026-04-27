using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Touir.ExpensesManager.Users.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repository;
        private readonly int _refreshExpiryInDays;

        public RefreshTokenService(IRefreshTokenRepository repository, IOptions<JwtAuthOptions> jwtAuthOptions)
        {
            _repository = repository;
            _refreshExpiryInDays = jwtAuthOptions.Value.RefreshExpiryInDays;
        }

        public async Task<string> GenerateAsync(int userId, bool rememberMe)
        {
            var tokenValue = Guid.NewGuid().ToString("N");
            var expiresAt = rememberMe
                ? DateTime.UtcNow.AddDays(_refreshExpiryInDays)
                : DateTime.UtcNow.AddDays(1);

            var refreshToken = new RefreshToken
            {
                Token = tokenValue,
                UserId = userId,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
            };

            await _repository.AddAsync(refreshToken);
            return tokenValue;
        }

        public async Task<(bool isValid, int userId)> ValidateAsync(string token)
        {
            var refreshToken = await _repository.GetActiveByTokenAsync(token);
            if (refreshToken == null || !refreshToken.IsActive)
                return (false, 0);
            return (true, refreshToken.UserId);
        }

        public async Task RevokeAsync(string token)
        {
            var refreshToken = await _repository.GetActiveByTokenAsync(token);
            if (refreshToken != null)
                await _repository.RevokeAsync(refreshToken);
        }

        public Task RevokeAllForUserAsync(int userId)
            => _repository.RevokeAllByUserIdAsync(userId);
    }
}
