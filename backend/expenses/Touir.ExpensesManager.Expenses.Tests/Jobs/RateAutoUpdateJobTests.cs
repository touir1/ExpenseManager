using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Touir.ExpensesManager.Expenses.Jobs;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Jobs
{
    public class RateAutoUpdateJobTests
    {
        private static RateAutoUpdateJob CreateJob(ICurrencyRateService? service = null, ILogger<RateAutoUpdateJob>? logger = null)
        {
            return new RateAutoUpdateJob(
                service ?? Mock.Of<ICurrencyRateService>(),
                logger ?? Mock.Of<ILogger<RateAutoUpdateJob>>());
        }

        private static IJobExecutionContext MakeContext()
        {
            var ctx = new Mock<IJobExecutionContext>();
            ctx.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
            return ctx.Object;
        }

        [Fact]
        public async Task Execute_CallsRunDailyUpdateAsync()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RunDailyUpdateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateJob(service.Object).Execute(MakeContext());

            service.Verify(s => s.RunDailyUpdateAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ServiceThrows_DoesNotPropagate()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RunDailyUpdateAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("provider down"));

            var ex = await Record.ExceptionAsync(() => CreateJob(service.Object).Execute(MakeContext()));
            Assert.Null(ex);
        }

        [Fact]
        public async Task Execute_ServiceThrows_LogsError()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RunDailyUpdateAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("provider down"));
            var logger = new Mock<ILogger<RateAutoUpdateJob>>();

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
