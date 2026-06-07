using System.Diagnostics.CodeAnalysis;
using System.Text;
using Touir.ExpensesManager.Notifications.Infrastructure.Contracts;

namespace Touir.ExpensesManager.Notifications.Infrastructure
{
    [ExcludeFromCodeCoverage]
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

        public string GetEmailTemplate(string templateKey, Dictionary<string, string> parameters)
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "EmailTemplates", $"{templateKey}.html");
            var templateContent = new StringBuilder(File.ReadAllText(filePath));

            templateContent.Replace("@@YEAR@@", DateTime.UtcNow.Year.ToString());

            if (parameters != null)
            {
                foreach (KeyValuePair<string, string> param in parameters)
                    templateContent.Replace($"@@{param.Key}@@", param.Value);
            }

            return templateContent.ToString();
        }
    }
}
