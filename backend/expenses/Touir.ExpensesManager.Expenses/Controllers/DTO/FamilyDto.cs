namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class FamilyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsArchived { get; set; }
        public string UserRole { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class FamilyDetailDto : FamilyDto
    {
        public IEnumerable<FamilyMemberDto> Members { get; set; } = [];
    }

    public class FamilyMemberDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
    }
}
