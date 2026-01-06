using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services.Contracts;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        public ApplicationService(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }
        public async Task<Application> GetApplicationByCodeAsync(string applicationCode)
        {
            return await _applicationRepository.GetApplicationByCodeAsync(applicationCode);
        }
    }
}
