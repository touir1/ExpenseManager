namespace com.touir.expenses.Expenses.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Category? ParentCategory { get; set; }
    }
}
