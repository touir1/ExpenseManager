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
        private readonly IRegistrationService _registrationService;
        private readonly IApplicationService _applicationService;

        public RegistrationController(
            IRegistrationService registrationService,
            IApplicationService applicationService)
        {
            _registrationService = registrationService;
            _applicationService = applicationService;
        }

        /// <summary>
        /// Register a new user account. Sends a verification email on success.
        /// Re-registering an unverified email silently resends the verification link.
        /// </summary>
        /// <param name="request">First name, last name, email, and application code.</param>
        [Route("register")]
        [HttpPost]
        [EnableRateLimiting("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Verify email after registration. Redirects to the app's success or error URL on completion.
        /// Hash expires 24 hours after issue; expired links return 401.
        /// </summary>
        /// <param name="emailVerificationHash">Verification hash from the email link (query param "h").</param>
        /// <param name="email">User email address (query param "s").</param>
        /// <param name="appCode">Application code identifying the redirect targets (query param "app_code").</param>
        [Route("validate-email")]
        [HttpGet]
        [EnableRateLimiting("validate_email")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateEmail(
            [FromQuery(Name = "h")] string emailVerificationHash,
            [FromQuery(Name = "s")] string email,
            [FromQuery(Name = "app_code")] string appCode)
        {
            if (string.IsNullOrWhiteSpace(emailVerificationHash) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(appCode))
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingParameters });

            try
            {
                ApplicationDto? app = await _applicationService.GetApplicationByCodeAsync(appCode);
                if (app == null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.EmailVerificationFailed });

                var emailLower = email.ToLowerInvariant();
                bool result = await _registrationService.ValidateEmailAsync(emailVerificationHash, emailLower);
                if (!result)
                {
                    var errorPath = !string.IsNullOrWhiteSpace(app.VerifyEmailErrorUrlPath)
                        ? $"{app.UrlPath}{app.VerifyEmailErrorUrlPath}?email={Uri.EscapeDataString(emailLower)}&app_code={Uri.EscapeDataString(appCode)}"
                        : null;
                    if (errorPath != null)
                        return Redirect(errorPath);
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.EmailVerificationFailed });
                }
                return Redirect($"{app.ResetPasswordUrlPath}?email={Uri.EscapeDataString(emailLower)}&h={emailVerificationHash}&mode=create");
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Resend the email verification link. Always returns 200 regardless of account existence.
        /// Resets the 24-hour expiry window; any previously issued link becomes invalid.
        /// </summary>
        /// <param name="request">Email and application code.</param>
        [Route("resend-verification")]
        [HttpPost]
        [EnableRateLimiting("resend_verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
