using com.touir.expenses.Users.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace com.touir.expenses.Users.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginRequest model)
        {
            var user = await _authenticationService.AuthenticateAsync(model.Email, model.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var token = _authenticationService.GenerateJwtToken(user.Id, user.Email);

            return Ok(new
            {
                Id = user.Id,
                FirstName = user.FirstName, 
                LastName = user.LastName,
                Email = user.Email,
                Token = token
            });
        }
    }
}
