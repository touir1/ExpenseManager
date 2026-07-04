using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("{id:long}/receipt")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class ExpenseReceiptController : ControllerBase
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };

        private readonly IReceiptService _receiptService;

        public ExpenseReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        /// <summary>
        /// Upload (or replace) the receipt image for an expense owned by the authenticated user.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="file">Receipt image (jpeg/png/webp, max 5MB).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPost]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadAsync(long id, IFormFile? file, CancellationToken cancellationToken)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var fileError = ValidateUploadedFile(file);
            if (fileError is not null)
                return BadRequest(new ErrorResponse { Message = fileError });

            try
            {
                using var ms = new MemoryStream();
                await file!.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;

                var extension = Path.GetExtension(file.FileName);
                var dto = await _receiptService.UploadAsync(id, userId.Value, ms, file.ContentType, extension, cancellationToken);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Stream the receipt image back, either for inline viewing or as an attachment download.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="download">When true, sets Content-Disposition to attachment.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAsync(long id, [FromQuery] bool download, CancellationToken cancellationToken)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                var result = await _receiptService.GetAsync(id, userId.Value, cancellationToken);
                if (!result.ExpenseFound)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

                if (!result.HasReceipt)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ReceiptNotFound });

                if (download)
                {
                    var extension = GetExtensionFromContentType(result.ContentType!);
                    var fileName = $"receipt-{id}{extension}";
                    return File(result.Stream!, result.ContentType!, fileName);
                }

                return File(result.Stream!, result.ContentType!);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Remove the receipt image (if any) attached to an expense owned by the authenticated user.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                var result = await _receiptService.DeleteAsync(id, userId.Value, cancellationToken);
                if (!result.ExpenseFound)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

                if (!result.HadReceipt)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ReceiptNotFound });

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        private static string? ValidateUploadedFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return ControllerErrors.ReceiptNoFile;

            if (file.Length > MaxFileSizeBytes)
                return ControllerErrors.ReceiptFileTooLarge;

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                return ControllerErrors.ReceiptInvalidFileType;

            if (!AllowedContentTypes.Contains(file.ContentType))
                return ControllerErrors.ReceiptInvalidFileType;

            return null;
        }

        private static string GetExtensionFromContentType(string contentType) => contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg",
        };
    }
}
