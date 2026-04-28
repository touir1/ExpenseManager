using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class UserRoleAssignmentService : IUserRoleAssignmentService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IRoleRepository _roleRepository;

        public UserRoleAssignmentService(IApplicationRepository applicationRepository, IRoleRepository roleRepository)
        {
            _applicationRepository = applicationRepository;
            _roleRepository = roleRepository;
        }

        public async Task TryAssignDefaultRoleAsync(string? applicationCode, User? user)
        {
            if (applicationCode == null || user == null)
                return;
            Application? app = await _applicationRepository.GetApplicationByCodeAsync(applicationCode);
            if (app == null)
                return;
            var role = await _roleRepository.GetDefaultRoleByApplicationIdAsync(app.Id);
            if (role != null)
                await _roleRepository.AssignRoleToUserAsync(role.Id, user.Id);
        }
    }
}
