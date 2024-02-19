namespace Expenses.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsHidden { get; set; }
        public User User { get; set; }
        public Category Category { get; set; }
        public Currency Currency { get; set; }
        public double Amount { get; set; }
    }
}
