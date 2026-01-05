namespace Touir.ExpensesManager.Users.Controllers.EO
{
    public class UserEo
    {
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? FamilyId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public UserEo? CreatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public UserEo? LastUpdatedBy { get; set; }
        public bool? IsDisabled { get; set; }
    }
}
