using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private const string AuthTokenCookie = "auth_token";

        private readonly IAuthenticationService _authenticationService;
        private readonly IRoleService _roleService;
        private readonly IApplicationService _applicationService;
        private readonly JwtAuthOptions _jwtAuthOptions;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IRoleService roleService,
            IApplicationService applicationService,
            IOptions<JwtAuthOptions> jwtAuthOptions)
        {
            _authenticationService = authenticationService;
            _roleService = roleService;
            _applicationService = applicationService;
            _jwtAuthOptions = jwtAuthOptions.Value;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if(string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                var errors = await _authenticationService.RegisterNewUserAsync(request.FirstName, request.LastName, email, request.ApplicationCode);
                return Ok(new RegisterResponse
                {
                    Errors = errors,
                    HasError = errors != null && errors.Any()
                });
            }
            catch(Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
            
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            if(request == null)
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if (string.IsNullOrWhiteSpace(request.ApplicationCode) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized(new ErrorResponse{ Message = "MISSING_PARAMETERS" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                var user = await _authenticationService.AuthenticateAsync(email, request.Password);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_USERNAME_OR_PASSWORD" });

                var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id!.Value);
                if (roles == null || roles.Count() == 0)
                    return Unauthorized(new ErrorResponse { Message = "NO_ASSIGNED_ROLE" });

                var token = _authenticationService.GenerateJwtToken(user.Id!.Value, user.Email);

                Response.Cookies.Append(AuthTokenCookie, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtAuthOptions.ExpiryInMinutes)
                });

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
                    }),
                    Token = token
                });
            }
            catch(Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Verify email after registration
        /// </summary>
        /// <param name="emailVerificationHash">verification hash</param>
        /// <param name="email">email source</param>
        /// <param name="appCode">application code</param>
        /// <returns></returns>
        [Route("validate-email")]
        [HttpGet]
        public async Task<IActionResult> ValidateEmail(
            [FromQuery(Name ="h")] string emailVerificationHash, 
            [FromQuery(Name = "s")] string email, 
            [FromQuery(Name = "app_code")] string appCode)
        {
            if(string.IsNullOrWhiteSpace(emailVerificationHash) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(appCode))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            try
            {
                ApplicationEo app = await _applicationService.GetApplicationByCodeAsync(appCode);
                if (app == null)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });

                var emailLower = email.ToLowerInvariant();
                bool result = await _authenticationService.ValidateEmailAsync(emailVerificationHash, emailLower);
                if (!result)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });
                return Redirect($"{app.ResetPasswordUrlPath}?email={Uri.EscapeDataString(emailLower)}&h={emailVerificationHash}&mode=create");
            }
            catch(Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        [Route("change-password")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            // email is always mandatory for validation
            if (string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            // New password and confirm password are always needed
            if(string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if(!request.NewPassword.Equals(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "NOT_MATCHING_CONFIRM_PASSWORD" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                if(!(await _authenticationService.ChangePasswordAsync(email, request.OldPassword, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "SET_NEW_PASSWORD_FAILED" });
                    
                return Ok();
            }
            catch(Exception) 
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        [Route("request-password-reset")]
        [HttpPost]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });
            // email is always mandatory for validation
            if (string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });
            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _authenticationService.RequestPasswordResetAsync(email)))
                    return Unauthorized(new ErrorResponse { Message = "REQUEST_PASSWORD_RESET_FAILED" });
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        [Route("change-password-reset")]
        [HttpPost]
        public async Task<IActionResult> ChangePasswordReset(ChangePasswordResetRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            // email is always mandatory for validation
            if (string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if (string.IsNullOrWhiteSpace(request.VerificationHash))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            // New password and confirm password are always needed
            if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if (!request.NewPassword.Equals(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "NOT_MATCHING_CONFIRM_PASSWORD" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _authenticationService.ResetPasswordAsync(email, request.VerificationHash, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "RESET_PASSWORD_FAILED" });
                
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
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

                var validationResult = _authenticationService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_TOKEN" });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        [Route("session")]
        [HttpGet]
        public IActionResult Session()
        {
            try
            {
                if (!Request.Cookies.TryGetValue(AuthTokenCookie, out var token) || string.IsNullOrWhiteSpace(token))
                    return Unauthorized(new ErrorResponse { Message = "MISSING_TOKEN" });

                var validationResult = _authenticationService.ValidateToken(token);

                if (!validationResult.IsValid)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_TOKEN" });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthTokenCookie);
            return Ok();
        }
    }
}
