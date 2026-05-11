using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("auth")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordManagementService _passwordManagementService;

        public PasswordController(IPasswordManagementService passwordManagementService)
        {
            _passwordManagementService = passwordManagementService;
        }

        /// <summary>
        /// Change the authenticated user's password by providing the current password.
        /// </summary>
        /// <param name="request">Email, old password, and new password.</param>
        [Route("change-password")]
        [HttpPost]
        [EnableRateLimiting("change_password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.ChangePasswordAsync(email, request.OldPassword, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.SetNewPasswordFailed });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Send a password reset email to the given address.
        /// Always returns 200 to avoid user enumeration.
        /// </summary>
        /// <param name="request">Email and application code.</param>
        [Route("request-password-reset")]
        [HttpPost]
        [EnableRateLimiting("request_password_reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetRequest request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.RequestPasswordResetAsync(email, request.AppCode)))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.RequestPasswordResetFailed });
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Set a new password using the verification hash from the registration email.
        /// </summary>
        /// <param name="request">Email, verification hash, and new password.</param>
        [Route("create-password")]
        [HttpPost]
        [EnableRateLimiting("create_password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePassword(CreatePasswordRequest request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.CreatePasswordAsync(email, request.VerificationHash, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.CreatePasswordFailed });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Set a new password using the verification hash from a password reset email.
        /// </summary>
        /// <param name="request">Email, verification hash, and new password.</param>
        [Route("change-password-reset")]
        [HttpPost]
        [EnableRateLimiting("change_password_reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePasswordReset(ChangePasswordResetRequest request)
        {
            try
            {
                var email = request.Email.ToLowerInvariant();
                if (!(await _passwordManagementService.ResetPasswordAsync(email, request.VerificationHash, request.NewPassword)))
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.ResetPasswordFailed });

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
