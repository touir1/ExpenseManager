namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class AdminCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsArchived { get; set; }
        public IEnumerable<AdminSubcategoryDto> Subcategories { get; set; } = [];
    }

    public class AdminSubcategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsArchived { get; set; }
    }
}
