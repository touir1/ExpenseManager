using Moq;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Users.Tests.Infrastructure
{
    public class EmailHelperTests
    {
        private static (EmailHelper helper, Mock<IEmailService> emailServiceMock) CreateEmailHelper()
        {
            var emailServiceMock = new Mock<IEmailService>();
            return (new EmailHelper(emailServiceMock.Object), emailServiceMock);
        }

        #region VerifyEmail Tests

        [Fact]
        public void VerifyEmail_ReturnsTrue_WhenEmailIsValid()
        {
            var (helper, _) = CreateEmailHelper();

            Assert.True(helper.VerifyEmail("valid@email.com"));
            Assert.True(helper.VerifyEmail("user.name@example.com"));
            Assert.True(helper.VerifyEmail("test+tag@domain.co.uk"));
        }

        [Fact]
        public void VerifyEmail_ReturnsFalse_WhenEmailIsInvalid()
        {
            var (helper, _) = CreateEmailHelper();

            Assert.False(helper.VerifyEmail("invalid-email"));
            Assert.False(helper.VerifyEmail("@example.com"));
            Assert.False(helper.VerifyEmail("user@"));
            Assert.False(helper.VerifyEmail("no-domain"));
        }

        #endregion

        #region GetEmailTemplate Tests

        [Fact]
        public void GetEmailTemplate_ReplacesParameters_WhenParametersProvided()
        {
            var (helper, _) = CreateEmailHelper();
            var templateKey = "TestTemplate";
            var templateDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, templateKey + ".html");

            try
            {
                File.WriteAllText(templatePath, "Hello @@Name@@! Your code is @@Code@@.");
                var parameters = new Dictionary<string, string>
                {
                    { "Name", "World" },
                    { "Code", "12345" }
                };

                var result = helper.GetEmailTemplate(templateKey, parameters);

                Assert.Equal("Hello World! Your code is 12345.", result);
            }
            finally
            {
                if (File.Exists(templatePath))
                    File.Delete(templatePath);
            }
        }

        [Fact]
        public void GetEmailTemplate_ReturnsTemplateAsIs_WhenNoParametersProvided()
        {
            var (helper, _) = CreateEmailHelper();
            var templateKey = "NoParamsTemplate";
            var templateDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, templateKey + ".html");

            try
            {
                var templateContent = "Hello @@Name@@!";
                File.WriteAllText(templatePath, templateContent);

                var result = helper.GetEmailTemplate(templateKey, null!);

                Assert.Equal(templateContent, result);
            }
            finally
            {
                if (File.Exists(templatePath))
                    File.Delete(templatePath);
            }
        }

        [Fact]
        public void GetEmailTemplate_ReturnsTemplateAsIs_WhenEmptyParametersProvided()
        {
            var (helper, _) = CreateEmailHelper();
            var templateKey = "EmptyParamsTemplate";
            var templateDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, templateKey + ".html");

            try
            {
                var templateContent = "Hello @@Name@@!";
                File.WriteAllText(templatePath, templateContent);
                var parameters = new Dictionary<string, string>();

                var result = helper.GetEmailTemplate(templateKey, parameters);

                Assert.Equal(templateContent, result);
            }
            finally
            {
                if (File.Exists(templatePath))
                    File.Delete(templatePath);
            }
        }

        [Fact]
        public void GetEmailTemplate_ReplacesMultipleOccurrences_WhenParameterAppearsMultipleTimes()
        {
            var (helper, _) = CreateEmailHelper();
            var templateKey = "MultipleOccurrences";
            var templateDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, templateKey + ".html");

            try
            {
                File.WriteAllText(templatePath, "@@Name@@, hello @@Name@@! Welcome @@Name@@.");
                var parameters = new Dictionary<string, string> { { "Name", "John" } };

                var result = helper.GetEmailTemplate(templateKey, parameters);

                Assert.Equal("John, hello John! Welcome John.", result);
            }
            finally
            {
                if (File.Exists(templatePath))
                    File.Delete(templatePath);
            }
        }

        #endregion

        #region SendEmail Tests

        [Fact]
        public void SendEmail_DelegatesToEmailService()
        {
            var (helper, emailServiceMock) = CreateEmailHelper();
            emailServiceMock
                .Setup(s => s.SendEmail("to@test.com", null, null, "Subject", "Body", false, null))
                .Returns(true);

            var result = helper.SendEmail(recipientTo: "to@test.com", emailSubject: "Subject", emailBody: "Body");

            Assert.True(result);
            emailServiceMock.Verify(
                s => s.SendEmail("to@test.com", null, null, "Subject", "Body", false, null),
                Times.Once);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenEmailServiceReturnsFalse()
        {
            var (helper, emailServiceMock) = CreateEmailHelper();
            emailServiceMock
                .Setup(s => s.SendEmail(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<ICollection<string>?>()))
                .Returns(false);

            var result = helper.SendEmail(recipientTo: "to@test.com", emailSubject: "Subject", emailBody: "Body");

            Assert.False(result);
        }

        #endregion
    }
}
