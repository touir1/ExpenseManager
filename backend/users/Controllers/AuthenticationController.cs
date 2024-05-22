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
                return BadRequest("Error while trying to register a new user. If the problem persist, please contact administrators");
            }
            
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            if(request.ApplicationCode == null || request.Email == null || request.Password == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var user = await _authenticationService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var roles = await _roleService.GetUserRolesByApplicationCodeAsync(request.ApplicationCode, user.Id);
            if(roles == null || roles.Count() == 0)
                return Unauthorized(new { message = "No assigned role" });

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
    }
}
