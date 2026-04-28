using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IUserRoleAssignmentService
    {
        Task TryAssignDefaultRoleAsync(string? applicationCode, User? user);
    }
}
