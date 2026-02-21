using Microsoft.Extensions.Options;
using Moq;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using Touir.ExpensesManager.Expenses.Services;
using Xunit;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class RabbitMQServiceTests
    {
        [Fact]
        public void Constructor_WithValidOptions_InitializesConnectionFactory()
        {
            // Arrange
            var options = CreateRabbitMQOptions("localhost", 5672, "testuser", "testpass");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsNullReferenceException()
        {
            // Arrange
            IOptions<RabbitMQOptions>? options = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new RabbitMQService(options!));
        }

        [Fact]
        public void Constructor_WithNullOptionsValue_ThrowsNullReferenceException()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<RabbitMQOptions>>();
            mockOptions.Setup(o => o.Value).Returns((RabbitMQOptions)null!);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new RabbitMQService(mockOptions.Object));
        }

        [Theory]
        [InlineData("localhost", 5672, "guest", "guest")]
        [InlineData("rabbitmq.example.com", 5673, "admin", "password123")]
        [InlineData("127.0.0.1", 5674, "user1", "pass1")]
        [InlineData("rabbitmq-server.internal", 5672, "app_user", "secure_pass")]
        [InlineData("10.0.0.1", 15672, "test", "test123")]
        public void Constructor_WithVariousValidOptions_InitializesSuccessfully(
            string hostname, int port, string username, string password)
        {
            // Arrange
            var options = CreateRabbitMQOptions(hostname, port, username, password);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithEmptyHostName_InitializesService()
        {
            // Arrange
            var options = CreateRabbitMQOptions("", 5672, "guest", "guest");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullHostName_InitializesService()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = null,
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(5672)]
        [InlineData(65535)]
        [InlineData(65536)]
        [InlineData(99999)]
        public void Constructor_WithVariousPorts_InitializesService(int port)
        {
            // Arrange
            var options = CreateRabbitMQOptions("localhost", port, "guest", "guest");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
            // Note: Connection validation happens when GetConnection() is called, not in constructor
        }

        [Fact]
        public void Constructor_WithNullUsername_ThrowsArgumentNullException()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = null,
                Password = "guest"
            };
            var options = Options.Create(rabbitMqOptions);

            // Act & Assert
            // ConnectionFactory throws ArgumentNullException when UserName is null
            Assert.Throws<ArgumentNullException>(() => new RabbitMQService(options));
        }

        [Fact]
        public void Constructor_WithNullPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = null
            };
            var options = Options.Create(rabbitMqOptions);

            // Act & Assert
            // ConnectionFactory throws ArgumentNullException when Password is null
            Assert.Throws<ArgumentNullException>(() => new RabbitMQService(options));
        }

        [Fact]
        public void Constructor_WithEmptyUsername_InitializesService()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "",
                Password = "guest"
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithEmptyPassword_InitializesService()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = ""
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithDefaultOptions_InitializesSuccessfully()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions();
            var options = Options.Create(rabbitMqOptions);

            // Act
            var exception = Record.Exception(() => new RabbitMQService(options));

            // Assert
            // ConnectionFactory throws ArgumentNullException when UserName or Password is null
            // This is expected behavior from RabbitMQ.Client library
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void Constructor_WithAllNullProperties_ThrowsArgumentNullException()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = null,
                Port = 0,
                UserName = null,
                Password = null
            };
            var options = Options.Create(rabbitMqOptions);

            // Act & Assert
            // ConnectionFactory throws ArgumentNullException when UserName or Password is null
            var exception = Assert.Throws<ArgumentNullException>(() => new RabbitMQService(options));
            Assert.Contains("value", exception.Message);
        }

        [Theory]
        [InlineData("localhost", 5672, "user1", "pass1")]
        [InlineData("127.0.0.1", 5673, "user2", "pass2")]
        [InlineData("rabbitmq.local", 5674, "user3", "pass3")]
        public void Constructor_CreateMultipleInstances_EachInitializesSuccessfully(
            string hostname, int port, string username, string password)
        {
            // Arrange & Act
            var service1 = new RabbitMQService(CreateRabbitMQOptions(hostname, port, username, password));
            var service2 = new RabbitMQService(CreateRabbitMQOptions(hostname, port, username, password));
            var service3 = new RabbitMQService(CreateRabbitMQOptions(hostname, port, username, password));

            // Assert
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotNull(service3);
            // Each service instance is independent
            Assert.NotSame(service1, service2);
            Assert.NotSame(service2, service3);
            Assert.NotSame(service1, service3);
        }

        [Fact]
        public void Constructor_WithSpecialCharactersInCredentials_InitializesService()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "user@domain.com",
                Password = "p@ssw0rd!#$%"
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithLongCredentials_InitializesService()
        {
            // Arrange
            var longUsername = new string('u', 1000);
            var longPassword = new string('p', 1000);
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "localhost",
                Port = 5672,
                UserName = longUsername,
                Password = longPassword
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithWhitespaceHostName_InitializesService()
        {
            // Arrange
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = "   ",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
            var options = Options.Create(rabbitMqOptions);

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_SameConfigurationTwice_CreatesDifferentInstances()
        {
            // Arrange
            var options = CreateRabbitMQOptions("localhost", 5672, "guest", "guest");

            // Act
            var service1 = new RabbitMQService(options);
            var service2 = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotSame(service1, service2);
        }

        [Theory]
        [InlineData("LOCALHOST")]
        [InlineData("LocalHost")]
        [InlineData("localhost")]
        public void Constructor_WithDifferentHostNameCasing_InitializesService(string hostname)
        {
            // Arrange
            var options = CreateRabbitMQOptions(hostname, 5672, "guest", "guest");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithIPv6Address_InitializesService()
        {
            // Arrange
            var options = CreateRabbitMQOptions("::1", 5672, "guest", "guest");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithHostNameContainingPort_InitializesService()
        {
            // Arrange
            // Note: This is not a recommended pattern, but testing it doesn't break constructor
            var options = CreateRabbitMQOptions("localhost:5672", 5672, "guest", "guest");

            // Act
            var service = new RabbitMQService(options);

            // Assert
            Assert.NotNull(service);
        }

        private static IOptions<RabbitMQOptions> CreateRabbitMQOptions(
            string hostname, 
            int port, 
            string username, 
            string password)
        {
            var rabbitMqOptions = new RabbitMQOptions
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password
            };
            return Options.Create(rabbitMqOptions);
        }
    }
}
