using Touir.ExpensesManager.Users.Controllers.EO;
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
        public async Task<ApplicationEo> GetApplicationByCodeAsync(string applicationCode)
        {
            var app = await _applicationRepository.GetApplicationByCodeAsync(applicationCode);
            if (app == null) return null;
            return new ApplicationEo
            {
                Id = app.Id,
                Code = app.Code,
                Name = app.Name,
                Description = app.Description,
                UrlPath = app.UrlPath
            };
        }
    }
}
