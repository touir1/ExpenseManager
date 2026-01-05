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
        [Fact]
        public void VerifyEmail_ValidAndInvalid()
        {
            var options = Options.Create(new EmailOptions { Email = "sender@test.com", Password = "pass", Host = "smtp.test.com", Port = 587 });
            
            var helper = new EmailHelper(options);
            
            Assert.True(helper.VerifyEmail("valid@email.com"));
            Assert.False(helper.VerifyEmail("invalid-email"));
        }

        [Fact]
        public void GetEmailTemplate_ReplacesParameters()
        {
            var options = Options.Create(new EmailOptions { Email = "sender@test.com", Password = "pass", Host = "smtp.test.com", Port = 587 });
            var helper = new EmailHelper(options);
            // Create a temporary template file
            var templateKey = "TestTemplate";
            var templateDir = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            var templatePath = Path.Combine(templateDir, templateKey + ".html");
            File.WriteAllText(templatePath, "Hello @@Name@@!");
            var parameters = new Dictionary<string, string> { { "Name", "World" } };
            
            var result = helper.GetEmailTemplate(templateKey, parameters);
            
            Assert.Equal("Hello World!", result);
            File.Delete(templatePath);
        }
    }
}
