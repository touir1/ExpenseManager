using Quartz;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Jobs
{
    [DisallowConcurrentExecution]
    public class RateAutoUpdateJob(ICurrencyRateService currencyRateService, ILogger<RateAutoUpdateJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await currencyRateService.RunDailyUpdateAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Currency rate auto-update failed.");
            }
        }
    }
}
