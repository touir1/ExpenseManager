using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Touir.ExpensesManager.Expenses.Jobs;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Jobs
{
    public class RecurringExpenseGenerationJobTests
    {
        private static RecurringExpenseGenerationJob CreateJob(
            IRecurringExpenseService? service = null,
            ILogger<RecurringExpenseGenerationJob>? logger = null)
        {
            return new RecurringExpenseGenerationJob(
                service ?? Mock.Of<IRecurringExpenseService>(),
                logger ?? Mock.Of<ILogger<RecurringExpenseGenerationJob>>());
        }

        private static IJobExecutionContext MakeContext()
        {
            var ctx = new Mock<IJobExecutionContext>();
            ctx.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
            return ctx.Object;
        }

        [Fact]
        public async Task Execute_CallsGenerateDueAsync()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GenerateDueAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateJob(service.Object).Execute(MakeContext());

            service.Verify(s => s.GenerateDueAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ServiceThrows_DoesNotPropagate()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GenerateDueAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("db down"));

            var ex = await Record.ExceptionAsync(() => CreateJob(service.Object).Execute(MakeContext()));
            Assert.Null(ex);
        }

        [Fact]
        public async Task Execute_ServiceThrows_LogsError()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GenerateDueAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("db down"));
            var logger = new Mock<ILogger<RecurringExpenseGenerationJob>>();

            await CreateJob(service.Object, logger.Object).Execute(MakeContext());

            logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => true),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
