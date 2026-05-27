using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Filters;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("admin/categories")]
    [ApiController]
    [AppAdmin]
    [EnableRateLimiting("expenses_global")]
    public class AdminCategoryController : ControllerBase
    {
        private readonly IAdminCategoryService _adminCategoryService;

        public AdminCategoryController(IAdminCategoryService adminCategoryService)
        {
            _adminCategoryService = adminCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var categories = await _adminCategoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCategoryAsync(AdminCategoryRequest request)
        {
            try
            {
                var category = await _adminCategoryService.AddCategoryAsync(request.Name, request.Description);
                return CreatedAtRoute("GetAdminCategories", null, category);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoryAsync(int id, AdminCategoryRequest request)
        {
            try
            {
                var category = await _adminCategoryService.UpdateCategoryAsync(id, request.Name, request.Description);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("{id:int}/archive")]
        public async Task<IActionResult> ArchiveCategoryAsync(int id)
        {
            try
            {
                await _adminCategoryService.ArchiveCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("{id:int}/unarchive")]
        public async Task<IActionResult> UnarchiveCategoryAsync(int id)
        {
            try
            {
                await _adminCategoryService.UnarchiveCategoryAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("{id:int}/subcategories")]
        public async Task<IActionResult> AddSubcategoryAsync(int id, AdminCategoryRequest request)
        {
            try
            {
                var sub = await _adminCategoryService.AddSubcategoryAsync(id, request.Name, request.Description);
                return CreatedAtRoute("GetAdminCategories", null, sub);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPut("{id:int}/subcategories/{subId:int}")]
        public async Task<IActionResult> UpdateSubcategoryAsync(int id, int subId, AdminCategoryRequest request)
        {
            try
            {
                var sub = await _adminCategoryService.UpdateSubcategoryAsync(subId, request.Name, request.Description);
                return Ok(sub);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("{id:int}/subcategories/{subId:int}/archive")]
        public async Task<IActionResult> ArchiveSubcategoryAsync(int id, int subId)
        {
            try
            {
                await _adminCategoryService.ArchiveSubcategoryAsync(subId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("{id:int}/subcategories/{subId:int}/unarchive", Name = "GetAdminCategories")]
        public async Task<IActionResult> UnarchiveSubcategoryAsync(int id, int subId)
        {
            try
            {
                await _adminCategoryService.UnarchiveSubcategoryAsync(subId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
