using com.touir.expenses.Users.Controllers.EO;
using com.touir.expenses.Users.Controllers.Requests;
using com.touir.expenses.Users.Controllers.Responses;
using com.touir.expenses.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace com.touir.expenses.Users.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IRoleService _roleService;

        public AuthenticationController(IAuthenticationService authenticationService, IRoleService roleService)
        {
            _authenticationService = authenticationService;
            _roleService = roleService;
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
                var errors = await _authenticationService.RegisterNewUserAsync(request.FirstName, request.LastName, request.Email);
                return Ok(new RegisterResponse
                {
                    Errors = errors,
                    HasError = errors != null && errors.Count() > 0
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
                var user = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized(new ErrorResponse { Message = "INVALID_USERNAME_OR_PASSWORD" });

                var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id);
                if (roles == null || roles.Count() == 0)
                    return Unauthorized(new ErrorResponse { Message = "NO_ASSIGNED_ROLE" });

                var token = _authenticationService.GenerateJwtToken(user.Id, user.Email);

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
        /// <param name="source">email source</param>
        /// <returns></returns>
        [Route("verify-email")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail([FromQuery(Name ="h")] string emailVerificationHash, [FromQuery(Name = "s")] string source)
        {
            if(string.IsNullOrWhiteSpace(emailVerificationHash) || string.IsNullOrWhiteSpace(source))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            try
            {
                bool result = await _authenticationService.VerifyEmailAsync(emailVerificationHash, source);
                if (!result)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });
                return Ok();
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

            // if hash not sent, it's a change password request so we need the old password to validate
            if (string.IsNullOrWhiteSpace(request.VerificationHash) && string.IsNullOrWhiteSpace(request.OldPassword))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            // New password and confirm password are always needed
            if(string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "MISSING_PARAMETERS" });

            if(!request.NewPassword.Equals(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "NOT_MATCHING_CONFIRM_PASSWORD" });

            try
            {
                // if hash is not set, it's a change password request
                if(!string.IsNullOrWhiteSpace(request.VerificationHash) && (await _authenticationService.ChangePasswordAsync(request.Email, request.OldPassword, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "SET_NEW_PASSWORD_FAILED" });
                // if hash is set, it's a reset password request
                if(string.IsNullOrWhiteSpace(request.VerificationHash) && (await _authenticationService.ResetPasswordAsync(request.Email, request.VerificationHash, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "RESET_PASSWORD_FAILED" });
                return Ok();
            }
            catch(Exception) 
            {
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }
    }
}
