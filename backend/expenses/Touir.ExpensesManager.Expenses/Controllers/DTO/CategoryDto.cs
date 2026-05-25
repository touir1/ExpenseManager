namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public IEnumerable<SubcategoryDto> Subcategories { get; set; } = [];
    }
}
