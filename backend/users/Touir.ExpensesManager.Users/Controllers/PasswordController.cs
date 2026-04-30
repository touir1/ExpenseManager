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
