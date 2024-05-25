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
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR_REGISTER_USER" });
            }
            
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            if(request.ApplicationCode == null || request.Email == null || request.Password == null)
                return Unauthorized(new ErrorResponse{ Message = "INVALID_USERNAME_OR_PASSWORD" });

            var user = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new ErrorResponse { Message = "INVALID_USERNAME_OR_PASSWORD" });

            var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id);
            if(roles == null || roles.Count() == 0)
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

        /// <summary>
        /// Verify email after registration
        /// </summary>
        /// <param name="h">verification hash</param>
        /// <param name="s">email source</param>
        /// <returns></returns>
        [Route("verifyEmail")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail([FromQuery] string h, [FromQuery] string s)
        {
            bool result = await _authenticationService.VerifyEmail(h, s);
            if (!result)
                return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED"});
            return Ok();
        }
    }
}
