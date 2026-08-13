using Quartz;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Jobs
{
    [DisallowConcurrentExecution]
    public class RecurringExpenseGenerationJob(IRecurringExpenseService recurringExpenseService, ILogger<RecurringExpenseGenerationJob> logger) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                await recurringExpenseService.GenerateDueAsync(today, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Recurring expense auto-generation failed.");
            }
        }
    }
}
