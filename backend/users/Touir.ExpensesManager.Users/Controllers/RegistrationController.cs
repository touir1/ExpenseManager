using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest request)
        {
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
        [EnableRateLimiting("validate_email")]
        public async Task<IActionResult> ValidateEmail(
            [FromQuery(Name = "h")] string emailVerificationHash,
            [FromQuery(Name = "s")] string email,
            [FromQuery(Name = "app_code")] string appCode)
        {
            if (string.IsNullOrWhiteSpace(emailVerificationHash) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(appCode))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            try
            {
                ApplicationDto? app = await _applicationService.GetApplicationByCodeAsync(appCode);
                if (app == null)
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });

                var emailLower = email.ToLowerInvariant();
                bool result = await _registrationService.ValidateEmailAsync(emailVerificationHash, emailLower);
                if (!result)
                {
                    var errorPath = !string.IsNullOrWhiteSpace(app.VerifyEmailErrorUrlPath)
                        ? $"{app.UrlPath}{app.VerifyEmailErrorUrlPath}?email={Uri.EscapeDataString(emailLower)}&app_code={Uri.EscapeDataString(appCode)}"
                        : null;
                    if (errorPath != null)
                        return Redirect(errorPath);
                    return Unauthorized(new ErrorResponse { Message = "EMAIL_VERIFICATION_FAILED" });
                }
                return Redirect($"{app.ResetPasswordUrlPath}?email={Uri.EscapeDataString(emailLower)}&h={emailVerificationHash}&mode=create");
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("resend-verification")]
        [HttpPost]
        [EnableRateLimiting("resend_verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                await _registrationService.ResendVerificationEmailAsync(email, request.ApplicationCode);
                return Ok(new { Message = "a new verification link will be sent shortly" });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }
    }
}
