using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IFamilyRepository
    {
        Task<bool> ExistsWithNameForUserAsync(string name, int userId, int? excludeId = null);
        Task<Family> CreateAsync(Family family, FamilyMembership headMembership);
        Task<Family?> GetByIdAsync(int id);
        Task<(Family? Family, IEnumerable<FamilyMembership> Members)> GetByIdWithMembersAsync(int id);
        Task<IEnumerable<(Family Family, FamilyMembership Membership)>> GetFamiliesByUserAsync(int userId);
        Task<Family?> GetDefaultFamilyForUserAsync(int userId);
        Task<bool> HasDefaultFamilyAsync(int userId);
        Task<FamilyMembership?> GetMembershipAsync(int familyId, int userId);
        Task<bool> IsMemberAsync(int familyId, int userId);
        Task<int> CountHeadsAsync(int familyId, int headRoleId);
        Task AddMemberAsync(FamilyMembership membership);
        Task UpdateMemberAsync(FamilyMembership membership);
        Task RemoveMemberAsync(FamilyMembership membership);
        Task UpdateFamilyAsync(Family family);
        Task AddInvitationAsync(FamilyInvitation invitation);
        Task<FamilyInvitation?> GetInvitationByTokenAsync(string token);
        Task UpdateInvitationAsync(FamilyInvitation invitation);
        Task<IEnumerable<FamilyInvitation>> GetPendingInvitationsByFamilyAsync(int familyId);
        Task DeleteInvitationAsync(FamilyInvitation invitation);
        Task AddAttributionsAsync(IEnumerable<ExpenseFamilyAttribution> attributions);
        Task ClearAttributionsAsync(long expenseId);
        Task<int> CountMemberAttributionsAsync(int familyId, int userId);
        Task RemoveMemberAttributionsAsync(int familyId, int ownerId);
    }
}
