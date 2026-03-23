using Microsoft.Extensions.Options;
using Moq;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using System.Net.Mail;

namespace Touir.ExpensesManager.Users.Tests.Infrastructure
{
    public class EmailHelperTests
    {
        private static EmailHelper CreateEmailHelper(bool enableSsl = true)
        {
            var options = Options.Create(new EmailOptions
            {
                Email = "sender@test.com",
                Password = "pass",
                Host = "smtp.test.com",
                Port = 587,
                EnableSsl = enableSsl
            });
            return new EmailHelper(options);
        }

        #region VerifyEmail Tests

        [Fact]
        public void VerifyEmail_ReturnsTrue_WhenEmailIsValid()
        {
            var helper = CreateEmailHelper();
            
            Assert.True(helper.VerifyEmail("valid@email.com"));
            Assert.True(helper.VerifyEmail("user.name@example.com"));
            Assert.True(helper.VerifyEmail("test+tag@domain.co.uk"));
        }

        [Fact]
        public void VerifyEmail_ReturnsFalse_WhenEmailIsInvalid()
        {
            var helper = CreateEmailHelper();
            
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
            var helper = CreateEmailHelper();
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
            var helper = CreateEmailHelper();
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
            var helper = CreateEmailHelper();
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
            var helper = CreateEmailHelper();
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
        public void SendEmail_ReturnsFalse_WhenSmtpConnectionFails()
        {
            // This will fail because we're using invalid SMTP settings
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body"
            );
            
            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithRecipientTo()
        {
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
                recipientTo: "recipient@test.com",
                emailSubject: "Subject",
                emailBody: "Body",
                isHTML: false
            );
            
            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithRecipientCC()
        {
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
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
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
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
            var helper = CreateEmailHelper();
            
            // Create a temporary file for attachment
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Test content");
            
            try
            {
                var result = helper.SendEmail(
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
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: "<html><body>Test</body></html>",
                isHTML: true
            );
            
            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithAllParameters()
        {
            var helper = CreateEmailHelper();
            
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "Attachment content");
            
            try
            {
                var result = helper.SendEmail(
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

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithNullBody()
        {
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
                recipientTo: "to@test.com",
                emailSubject: "Subject",
                emailBody: null
            );
            
            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithMinimalParameters()
        {
            var helper = CreateEmailHelper();
            
            // Call with all defaults (all nulls)
            var result = helper.SendEmail();
            
            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenSmtpConnectionFails_WithSslDisabled()
        {
            // Simulates Mailpit / dev SMTP with EnableSsl=false
            var helper = CreateEmailHelper(enableSsl: false);

            var result = helper.SendEmail(
                recipientTo: "test@example.com",
                emailSubject: "Test Subject",
                emailBody: "Test Body"
            );

            Assert.False(result);
        }

        [Fact]
        public void SendEmail_ReturnsFalse_WhenExceptionOccurs_WithEmptyAttachmentsList()
        {
            var helper = CreateEmailHelper();
            
            var result = helper.SendEmail(
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
            var helper = CreateEmailHelper();
            
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            File.WriteAllText(tempFile1, "Content 1");
            File.WriteAllText(tempFile2, "Content 2");
            
            try
            {
                var result = helper.SendEmail(
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

        #endregion
    }
}
