using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public AdminCategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<AdminCategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllWithArchivedAsync();
            return categories.Select(MapToDto);
        }

        public async Task<AdminCategoryDto> AddCategoryAsync(string name, string? description)
        {
            if (await _categoryRepository.ExistsWithNameAsync(name, parentCategoryId: null))
                throw new InvalidOperationException("CATEGORY_NAME_DUPLICATE");

            var category = new Category { Name = name, Description = description };
            var added = await _categoryRepository.AddAsync(category);
            return MapToDto(added);
        }

        public async Task<AdminCategoryDto> UpdateCategoryAsync(int id, string name, string? description)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (await _categoryRepository.ExistsWithNameAsync(name, parentCategoryId: null, excludeId: id))
                throw new InvalidOperationException("CATEGORY_NAME_DUPLICATE");

            category.Name = name;
            category.Description = description;
            await _categoryRepository.UpdateAsync(category);
            return MapToDto(category);
        }

        public async Task ArchiveCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            var hasActiveChildren = category.Children.Any(c => !c.IsDeleted);
            if (hasActiveChildren)
                throw new InvalidOperationException("CATEGORY_HAS_ACTIVE_SUBCATEGORIES");

            await _categoryRepository.ArchiveAsync(id);
        }

        public async Task UnarchiveCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (!category.IsDeleted)
                return;

            await _categoryRepository.UnarchiveAsync(id);
        }

        public async Task<AdminSubcategoryDto> AddSubcategoryAsync(int parentId, string name, string? description)
        {
            var parent = await _categoryRepository.GetByIdAsync(parentId)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (parent.IsDeleted)
                throw new InvalidOperationException("CATEGORY_ARCHIVED");

            if (parent.ParentCategoryId != null)
                throw new InvalidOperationException("CATEGORY_CANNOT_NEST_SUBCATEGORY");

            if (await _categoryRepository.ExistsWithNameAsync(name, parentCategoryId: parentId))
                throw new InvalidOperationException("CATEGORY_NAME_DUPLICATE");

            var subcategory = new Category { Name = name, Description = description, ParentCategoryId = parentId };
            var added = await _categoryRepository.AddAsync(subcategory);
            return MapToSubDto(added);
        }

        public async Task<AdminSubcategoryDto> UpdateSubcategoryAsync(int id, string name, string? description)
        {
            var subcategory = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (await _categoryRepository.ExistsWithNameAsync(name, parentCategoryId: subcategory.ParentCategoryId, excludeId: id))
                throw new InvalidOperationException("CATEGORY_NAME_DUPLICATE");

            subcategory.Name = name;
            subcategory.Description = description;
            await _categoryRepository.UpdateAsync(subcategory);
            return MapToSubDto(subcategory);
        }

        public async Task ArchiveSubcategoryAsync(int id)
        {
            var subcategory = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (subcategory.ParentCategoryId == null)
                throw new InvalidOperationException("CATEGORY_NOT_A_SUBCATEGORY");

            await _categoryRepository.ArchiveAsync(id);
        }

        public async Task UnarchiveSubcategoryAsync(int id)
        {
            var subcategory = await _categoryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("CATEGORY_NOT_FOUND");

            if (subcategory.ParentCategoryId == null)
                throw new InvalidOperationException("CATEGORY_NOT_A_SUBCATEGORY");

            if (!subcategory.IsDeleted)
                return;

            await _categoryRepository.UnarchiveAsync(id);
        }

        private static AdminCategoryDto MapToDto(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Icon = c.Icon,
            IsArchived = c.IsDeleted,
            Subcategories = c.Children.Select(MapToSubDto)
        };

        private static AdminSubcategoryDto MapToSubDto(Category s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Icon = s.Icon,
            IsArchived = s.IsDeleted
        };
    }
}
