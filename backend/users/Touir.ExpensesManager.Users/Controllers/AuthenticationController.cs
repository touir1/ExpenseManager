using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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

        /// <summary>
        /// Authenticate user and issue access + refresh token cookies.
        /// </summary>
        /// <param name="request">Login credentials and application code.</param>
        [Route("login")]
        [HttpPost]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            try
            {
                var email = request.Email!.ToLowerInvariant();
                var user = await _authenticationService.AuthenticateAsync(email, request.Password);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.InvalidUsernameOrPassword });

                var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id!.Value);
                if (!roles.Any())
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.NoAssignedRole });

                var isAdmin = await _roleService.IsAdminAsync(user.Id!.Value);
                var accessToken = _jwtTokenService.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName, isAdmin);
                var rememberMe = request.RememberMe ?? false;
                var refreshTokenValue = await _refreshTokenService.GenerateAsync(user.Id!.Value, rememberMe);

                var cookieOptions = BuildCookieOptions(rememberMe, _jwtAuthOptions.ExpiryInMinutes);
                var refreshCookieOptions = BuildCookieOptions(rememberMe, _jwtAuthOptions.RefreshExpiryInDays * 24 * 60);

                Response.Cookies.Append(AuthTokenCookie, accessToken, cookieOptions);
                Response.Cookies.Append(RefreshTokenCookie, refreshTokenValue, refreshCookieOptions);

                return Ok(new LoginResponse
                {
                    User = new UserDto
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    },
                    Roles = roles.Select(s => new RoleDto
                    {
                        Code = s.Code,
                        Description = s.Description,
                        Name = s.Name
                    })
                });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Validate a JWT token from the Authorization header or auth_token cookie.
        /// Used by nginx as an auth subrequest before proxying requests.
        /// </summary>
        [Route("check")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

                var validationResult = _jwtTokenService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.InvalidToken });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Return the authenticated user's profile from the auth_token cookie.
        /// </summary>
        [Route("session")]
        [HttpGet]
        [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult Session()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(AuthTokenCookie, out var token) || string.IsNullOrWhiteSpace(token))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

                var validationResult = _jwtTokenService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.InvalidToken });

                var jwtToken = validationResult.SecurityToken as JwtSecurityToken;
                var email = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
                var firstName = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                var lastName = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
                var isAdmin = jwtToken?.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value == "true";

                return Ok(new SessionResponse { Email = email, FirstName = firstName, LastName = lastName, IsAdmin = isAdmin });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Rotate the refresh token and issue a new access token cookie.
        /// Reads the refresh_token cookie; rotates it on success.
        /// </summary>
        [Route("refresh")]
        [HttpPost]
        [EnableRateLimiting("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshAsync()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

                var (isValid, userId) = await _refreshTokenService.ValidateAsync(refreshToken);
                if (!isValid)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.InvalidToken });

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.UserNotFound });

                var isAdminOnRefresh = await _roleService.IsAdminAsync(userId);

                // Rotate refresh token
                await _refreshTokenService.RevokeAsync(refreshToken);
                var newRefreshToken = await _refreshTokenService.GenerateAsync(userId, rememberMe: true);
                var newAccessToken = _jwtTokenService.GenerateJwtToken(user.Id, user.Email, user.FirstName, user.LastName, isAdminOnRefresh);

                var cookieOptions = BuildCookieOptions(rememberMe: true, _jwtAuthOptions.ExpiryInMinutes);
                var refreshCookieOptions = BuildCookieOptions(rememberMe: true, _jwtAuthOptions.RefreshExpiryInDays * 24 * 60);

                Response.Cookies.Append(AuthTokenCookie, newAccessToken, cookieOptions);
                Response.Cookies.Append(RefreshTokenCookie, newRefreshToken, refreshCookieOptions);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Revoke the refresh token and clear auth cookies.
        /// </summary>
        [Route("logout")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
