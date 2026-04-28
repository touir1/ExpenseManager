using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("auth")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private const string MissingParameters = "MISSING_PARAMETERS";
        private const string ServerError = "SERVER_ERROR";

        private readonly IPasswordManagementService _passwordManagementService;

        public PasswordController(IPasswordManagementService passwordManagementService)
        {
            _passwordManagementService = passwordManagementService;
        }

        [Route("change-password")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (!request.NewPassword.Equals(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "NOT_MATCHING_CONFIRM_PASSWORD" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.ChangePasswordAsync(email, request.OldPassword, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "SET_NEW_PASSWORD_FAILED" });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("request-password-reset")]
        [HttpPost]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.AppCode))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.RequestPasswordResetAsync(email, request.AppCode)))
                    return Unauthorized(new ErrorResponse { Message = "REQUEST_PASSWORD_RESET_FAILED" });
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [Route("change-password-reset")]
        [HttpPost]
        public async Task<IActionResult> ChangePasswordReset(ChangePasswordResetRequest request)
        {
            if (request == null)
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.Email))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.VerificationHash))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = MissingParameters });

            if (!request.NewPassword.Equals(request.ConfirmPassword))
                return Unauthorized(new ErrorResponse { Message = "NOT_MATCHING_CONFIRM_PASSWORD" });

            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.ResetPasswordAsync(email, request.VerificationHash, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = "RESET_PASSWORD_FAILED" });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }
    }
}
