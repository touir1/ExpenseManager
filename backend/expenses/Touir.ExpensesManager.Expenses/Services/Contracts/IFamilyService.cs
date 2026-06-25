using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IFamilyService
    {
        Task CreateDefaultAsync(int userId);
        Task<FamilyDto> CreateAsync(string name, int userId);
        Task<IEnumerable<FamilyDto>> GetByUserAsync(int userId);
        Task<FamilyDetailDto> GetByIdAsync(int id, int userId);
        Task<FamilyDto> RenameAsync(int id, string name, int userId);
        Task<string> InviteAsync(int familyId, string email, int invitedById);
        Task AcceptInviteAsync(string token, int userId);
        Task RemoveMemberAsync(int familyId, int targetUserId, int removedById);
        Task ChangeRoleAsync(int familyId, int targetUserId, string roleName, int changedById);
        Task ArchiveAsync(int familyId, int userId);
        Task UnarchiveAsync(int familyId, int userId);
        Task LeaveAsync(int familyId, int userId);
        Task<IEnumerable<FamilyPendingInvitationDto>> GetPendingInvitationsAsync(int familyId, int userId);
        Task RevokeInvitationAsync(int familyId, string token, int userId);
    }
}
