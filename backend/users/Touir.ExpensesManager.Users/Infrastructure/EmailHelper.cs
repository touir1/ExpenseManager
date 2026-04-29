using System.Net.Mail;
using System.Text;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Users.Infrastructure
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IEmailService _emailService;

        public EmailHelper(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public bool SendEmail(
            string? recipientTo = null,
            string? recipientCC = null,
            string? recipientBCC = null,
            string? emailSubject = null,
            string? emailBody = null,
            bool isHTML = false,
            ICollection<string>? attachments = null)
            => _emailService.SendEmail(recipientTo, recipientCC, recipientBCC, emailSubject, emailBody, isHTML, attachments);

        public bool VerifyEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string GetEmailTemplate(string templateKey, Dictionary<string, string> parameters)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates", $"{templateKey}.html");
            StringBuilder templateContent = new StringBuilder(File.ReadAllText(filePath));

            if(parameters != null) { 
                foreach(KeyValuePair<string, string> param in  parameters)
                {
                    templateContent.Replace($"@@{param.Key}@@", param.Value);
                }
            }

            return templateContent.ToString();
        }
    }
}
