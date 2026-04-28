using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private const string AuthTokenCookie = "auth_token";
        private const string RefreshTokenCookie = "refresh_token";
        private const string MissingParameters = "MISSING_PARAMETERS";
        private const string ServerError = "SERVER_ERROR";

        private readonly IAuthenticationService _authenticationService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleService _roleService;
        private readonly JwtAuthOptions _jwtAuthOptions;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IUserRepository userRepository,
            IRoleService roleService,
            IOptions<JwtAuthOptions> jwtAuthOptions)
        {
            _authenticationService = authenticationService;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _userRepository = userRepository;
            _roleService = roleService;
            _jwtAuthOptions = jwtAuthOptions.Value;
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.ApplicationCode) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            try
            {
                var email = request.Email.ToLowerInvariant();
                var user = await _authenticationService.AuthenticateAsync(email, request.Password);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_USERNAME_OR_PASSWORD" });

                var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id!.Value);
                if (roles == null || !roles.Any())
                    return Unauthorized(new ErrorResponse { Message = "NO_ASSIGNED_ROLE" });

                var accessToken = _jwtTokenService.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName);
                var rememberMe = request.RememberMe ?? false;
                var refreshTokenValue = await _refreshTokenService.GenerateAsync(user.Id!.Value, rememberMe);

                var cookieOptions = BuildCookieOptions(rememberMe, _jwtAuthOptions.ExpiryInMinutes);
                var refreshCookieOptions = BuildCookieOptions(rememberMe, _jwtAuthOptions.RefreshExpiryInDays * 24 * 60);

                Response.Cookies.Append(AuthTokenCookie, accessToken, cookieOptions);
                Response.Cookies.Append(RefreshTokenCookie, refreshTokenValue, refreshCookieOptions);

                return Ok(new LoginResponse
                {
                    User = new UserEo
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    },
                    Roles = roles.Select(s => new RoleEo
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name
                    })
                });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("check")]
        [HttpGet]
        public IActionResult Check()
        {
            try
            {
                string? token = null;

                var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
                    token = authorizationHeader.Substring("Bearer ".Length).Trim();
                else
                    Request.Cookies.TryGetValue(AuthTokenCookie, out token);

                if (string.IsNullOrWhiteSpace(token))
                    return Unauthorized(new ErrorResponse { Message = "MISSING_TOKEN" });

                var validationResult = _jwtTokenService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_TOKEN" });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("session")]
        [HttpGet]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        public IActionResult Session()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(AuthTokenCookie, out var token) || string.IsNullOrWhiteSpace(token))
                    return Unauthorized(new ErrorResponse { Message = "MISSING_TOKEN" });

                var validationResult = _jwtTokenService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_TOKEN" });

                var jwtToken = validationResult.SecurityToken as JwtSecurityToken;
                var email = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
                var firstName = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                var lastName = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

                return Ok(new SessionResponse { Email = email, FirstName = firstName, LastName = lastName });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshAsync()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                    return Unauthorized(new ErrorResponse { Message = "MISSING_TOKEN" });

                var (isValid, userId) = await _refreshTokenService.ValidateAsync(refreshToken);
                if (!isValid)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_TOKEN" });

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = "USER_NOT_FOUND" });

                // Rotate refresh token
                await _refreshTokenService.RevokeAsync(refreshToken);
                var newRefreshToken = await _refreshTokenService.GenerateAsync(userId, rememberMe: true);
                var newAccessToken = _jwtTokenService.GenerateJwtToken(user.Id, user.Email, user.FirstName, user.LastName);

                var cookieOptions = BuildCookieOptions(rememberMe: true, _jwtAuthOptions.ExpiryInMinutes);
                var refreshCookieOptions = BuildCookieOptions(rememberMe: true, _jwtAuthOptions.RefreshExpiryInDays * 24 * 60);

                Response.Cookies.Append(AuthTokenCookie, newAccessToken, cookieOptions);
                Response.Cookies.Append(RefreshTokenCookie, newRefreshToken, refreshCookieOptions);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
                await _refreshTokenService.RevokeAsync(refreshToken);

            Response.Cookies.Delete(AuthTokenCookie);
            Response.Cookies.Delete(RefreshTokenCookie);
            return Ok();
        }

        private static CookieOptions BuildCookieOptions(bool rememberMe, int expiryInMinutes)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            };
            if (rememberMe)
                options.Expires = DateTimeOffset.UtcNow.AddMinutes(expiryInMinutes);
            return options;
        }
    }
}
