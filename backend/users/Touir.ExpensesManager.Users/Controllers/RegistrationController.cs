using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("auth")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private const string MissingParameters = "MISSING_PARAMETERS";
        private const string ServerError = "SERVER_ERROR";

        private readonly IRegistrationService _registrationService;
        private readonly IApplicationService _applicationService;

        public RegistrationController(
            IRegistrationService registrationService,
            IApplicationService applicationService)
        {
            _registrationService = registrationService;
            _applicationService = applicationService;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            try
            {
                var email = request.Email.ToLowerInvariant();
                var errors = await _registrationService.RegisterNewUserAsync(request.FirstName, request.LastName, email, request.ApplicationCode);
                return Ok(new RegisterResponse
                {
                    Errors = errors,
                    HasError = errors != null && errors.Any()
                });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        /// <summary>
        /// Verify email after registration
        /// </summary>
        /// <param name="emailVerificationHash">verification hash</param>
        /// <param name="email">email source</param>
        /// <param name="appCode">application code</param>
        [Route("validate-email")]
        [HttpGet]
        public async Task<IActionResult> ValidateEmail(
            [FromQuery(Name = "h")] string emailVerificationHash,
            [FromQuery(Name = "s")] string email,
            [FromQuery(Name = "app_code")] string appCode)
        {
            if (string.IsNullOrWhiteSpace(emailVerificationHash) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(appCode))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            try
            {
                ApplicationEo? app = await _applicationService.GetApplicationByCodeAsync(appCode);
                if (app == null)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });

                var emailLower = email.ToLowerInvariant();
                bool result = await _registrationService.ValidateEmailAsync(emailVerificationHash, emailLower);
                if (!result)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });
                return Redirect($"{app.ResetPasswordUrlPath}?email={Uri.EscapeDataString(emailLower)}&h={emailVerificationHash}&mode=create");
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }
    }
}
