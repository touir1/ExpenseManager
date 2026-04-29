using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Options;

namespace Touir.ExpensesManager.Users.Tests.Infrastructure
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
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body"
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenSmtpConnectionFails_WithSslDisabled()
        {
            var service = CreateService(enableSsl: false);

            var result = service.SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body"
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithRecipientCC()
        {
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "to@test.com",
                recipientCC: "cc@test.com",
                emailSubject: "Subject",
                emailBody: "Body"
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithRecipientBCC()
        {
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "to@test.com",
                recipientBCC: "bcc@test.com",
                emailSubject: "Subject",
                emailBody: "Body"
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithAttachments()
        {
            var service = CreateService();
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Test content");

            try
            {
                var result = service.SendEmail(
                    recipientTo: "to@test.com",
                    emailSubject: "Subject",
                    emailBody: "Body",
                    attachments: new List<string> { tempFile }
                );

                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithHTMLBody()
        {
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: "<html><body>Test</body></html>",
                isHTML: true
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithNullBody()
        {
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: null
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithMinimalParameters()
        {
            var service = CreateService();

            var result = service.SendEmail();

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithEmptyAttachmentsList()
        {
            var service = CreateService();

            var result = service.SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: "Body",
                attachments: new List<string>()
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithMultipleAttachments()
        {
            var service = CreateService();
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            File.WriteAllText(tempFile1, "Content 1");
            File.WriteAllText(tempFile2, "Content 2");

            try
            {
                var result = service.SendEmail(
                    recipientTo: "to@test.com",
                    emailSubject: "Subject",
                    emailBody: "Body",
                    attachments: new List<string> { tempFile1, tempFile2 }
                );

                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile1))
                    File.Delete(tempFile1);
                if (File.Exists(tempFile2))
                    File.Delete(tempFile2);
            }
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithAllParameters()
        {
            var service = CreateService();
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Attachment content");

            try
            {
                var result = service.SendEmail(
                    recipientTo: "to@test.com",
                    recipientCC: "cc@test.com",
                    recipientBCC: "bcc@test.com",
                    emailSubject: "Test Subject",
                    emailBody: "<html><body><h1>Hello</h1></body></html>",
                    isHTML: true,
                    attachments: new List<string> { tempFile }
                );

                Assert.False(result);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
