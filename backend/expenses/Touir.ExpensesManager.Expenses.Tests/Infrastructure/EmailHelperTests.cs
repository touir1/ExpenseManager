using Moq;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Infrastructure
{
    public class EmailHelperTests
    {
        private static (EmailHelper helper, Mock<IEmailService> emailServiceMock) CreateHelper()
        {
            var emailServiceMock = new Mock<IEmailService>();
            return (new EmailHelper(emailServiceMock.Object), emailServiceMock);
        }

        private static string CreateTempTemplate(string key, string content)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{key}.html");
            File.WriteAllText(path, content);
            return path;
        }

        // ── GetEmailTemplate ──────────────────────────────────────────────────

        [Fact]
        public void GetEmailTemplate_ReplacesParameters_WhenProvided()
        {
            var (helper, _) = CreateHelper();
            var path = CreateTempTemplate("TestTpl", "Hello @@Name@@! Code: @@Code@@.");
            try
            {
                var result = helper.GetEmailTemplate("TestTpl", new Dictionary<string, string>
                {
                    { "Name", "World" },
                    { "Code", "12345" }
                });

                Assert.Equal("Hello World! Code: 12345.", result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReturnsTemplateAsIs_WhenNoParameters()
        {
            var (helper, _) = CreateHelper();
            const string content = "Hello @@Name@@!";
            var path = CreateTempTemplate("NoParamsTpl", content);
            try
            {
                var result = helper.GetEmailTemplate("NoParamsTpl", null!);

                Assert.Equal(content, result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReturnsTemplateAsIs_WhenEmptyParameters()
        {
            var (helper, _) = CreateHelper();
            const string content = "Hello @@Name@@!";
            var path = CreateTempTemplate("EmptyParamsTpl", content);
            try
            {
                var result = helper.GetEmailTemplate("EmptyParamsTpl", new Dictionary<string, string>());

                Assert.Equal(content, result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReplacesMultipleOccurrences_OfSameParameter()
        {
            var (helper, _) = CreateHelper();
            var path = CreateTempTemplate("MultiTpl", "@@Name@@, hello @@Name@@! Welcome @@Name@@.");
            try
            {
                var result = helper.GetEmailTemplate("MultiTpl", new Dictionary<string, string> { { "Name", "John" } });

                Assert.Equal("John, hello John! Welcome John.", result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReplacesInviteLinkAndFamilyName_ForFamilyInvitationTemplate()
        {
            var (helper, _) = CreateHelper();
            var inviteLink = "https://localhost/families/accept-invite?token=abc-123";
            var familyName = "Smith Family";
            var path = CreateTempTemplate(
                "FamilyInvTpl",
                "Join @@FAMILY_NAME@@ at @@INVITE_LINK@@");
            try
            {
                var result = helper.GetEmailTemplate("FamilyInvTpl", new Dictionary<string, string>
                {
                    { "INVITE_LINK", inviteLink },
                    { "FAMILY_NAME", familyName }
                });

                Assert.Equal($"Join {familyName} at {inviteLink}", result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReplacesYear_Automatically()
        {
            var (helper, _) = CreateHelper();
            var path = CreateTempTemplate("YearTpl", "Copyright @@YEAR@@ Corp.");
            try
            {
                var result = helper.GetEmailTemplate("YearTpl", new Dictionary<string, string>());

                Assert.Equal($"Copyright {DateTime.UtcNow.Year} Corp.", result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        [Fact]
        public void GetEmailTemplate_ReplacesYear_EvenWithNullParameters()
        {
            var (helper, _) = CreateHelper();
            var path = CreateTempTemplate("YearNullTpl", "Year: @@YEAR@@");
            try
            {
                var result = helper.GetEmailTemplate("YearNullTpl", null!);

                Assert.Equal($"Year: {DateTime.UtcNow.Year}", result);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }

        // ── SendEmail ─────────────────────────────────────────────────────────

        [Fact]
        public void SendEmail_DelegatesToEmailService_AndReturnsTrue()
        {
            var (helper, mock) = CreateHelper();
            mock.Setup(s => s.SendEmail("to@test.com", null, null, "Subject", "Body", false, null))
                .Returns(true);

            var result = helper.SendEmail(recipientTo: "to@test.com", emailSubject: "Subject", emailBody: "Body");

            Assert.True(result);
            mock.Verify(s => s.SendEmail("to@test.com", null, null, "Subject", "Body", false, null), Times.Once);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenEmailServiceReturnsFalse()
        {
            var (helper, mock) = CreateHelper();
            mock.Setup(s => s.SendEmail(
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                    It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<ICollection<string>?>()))
                .Returns(false);

            var result = helper.SendEmail(recipientTo: "to@test.com", emailSubject: "Subject", emailBody: "Body");

            Assert.False(result);
        }
    }
}
