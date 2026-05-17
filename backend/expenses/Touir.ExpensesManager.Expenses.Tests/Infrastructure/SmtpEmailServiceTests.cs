using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;

namespace Touir.ExpensesManager.Expenses.Tests.Infrastructure
{
    public class SmtpEmailServiceTests
    {
        private static SmtpEmailService CreateService(bool enableSsl = true)
        {
            var options = Options.Create(new EmailOptions
            {
                Email = "sender@test.com",
                Password = "pass",
                Host = "smtp.test.com",
                Port = 587,
                EnableSsl = enableSsl
            });
            return new SmtpEmailService(options);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenSmtpConnectionFails()
        {
            var result = CreateService().SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body");

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenSslDisabled()
        {
            var result = CreateService(enableSsl: false).SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body");

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithRecipientCC()
        {
            var result = CreateService().SendEmail(
                recipientTo: "to@test.com",
                recipientCC: "cc@test.com",
                emailSubject: "Subject",
                emailBody: "Body");

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithRecipientBCC()
        {
            var result = CreateService().SendEmail(
                recipientTo: "to@test.com",
                recipientBCC: "bcc@test.com",
                emailSubject: "Subject",
                emailBody: "Body");

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithHTMLBody()
        {
            var result = CreateService().SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: "<html><body>Test</body></html>",
                isHTML: true);

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithNullBody()
        {
            var result = CreateService().SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: null);

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithMinimalParameters()
        {
            Assert.False(CreateService().SendEmail());
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithEmptyAttachmentsList()
        {
            var result = CreateService().SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: "Body",
                attachments: new List<string>());

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithSingleAttachment()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Test content");
            try
            {
                var result = CreateService().SendEmail(
                    recipientTo: "to@test.com",
                    emailSubject: "Subject",
                    emailBody: "Body",
                    attachments: new List<string> { tempFile });

                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WithAllParameters()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Attachment content");
            try
            {
                var result = CreateService().SendEmail(
                    recipientTo: "to@test.com",
                    recipientCC: "cc@test.com",
                    recipientBCC: "bcc@test.com",
                    emailSubject: "Test Subject",
                    emailBody: "<html><body><h1>Hello</h1></body></html>",
                    isHTML: true,
                    attachments: new List<string> { tempFile });

                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
