using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;

namespace Touir.ExpensesManager.Users.Infrastructure
{
    public class SmtpEmailService : IEmailService
    {
        private readonly string? _sender;
        private readonly string? _senderPassword;
        private readonly string? _host;
        private readonly int? _port;
        private readonly bool _enableSsl;

        public SmtpEmailService(IOptions<EmailOptions> emailOptions)
        {
            _sender = emailOptions.Value.Email;
            _senderPassword = emailOptions.Value.Password;
            _host = emailOptions.Value.Host;
            _port = emailOptions.Value.Port;
            _enableSsl = emailOptions.Value.EnableSsl;
        }

        public bool SendEmail(
            string? recipientTo = null,
            string? recipientCC = null,
            string? recipientBCC = null,
            string? emailSubject = null,
            string? emailBody = null,
            bool isHTML = false,
            ICollection<string>? attachments = null)
        {
            try
            {
                using var client = new SmtpClient();
                client.Host = _host!;
                client.Port = _port!.Value;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = _enableSsl; // NOSONAR - intentionally configurable: false only for local dev (Mailpit), true in all other environments
                client.Credentials = new NetworkCredential(_sender, _senderPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(_sender!);
                if (recipientTo != null)
                    message.To.Add(recipientTo);
                if (recipientCC != null)
                    message.CC.Add(recipientCC);
                if (recipientBCC != null)
                    message.Bcc.Add(recipientBCC);
                if (attachments != null && attachments.Count > 0)
                {
                    foreach (string filePath in attachments)
                        message.Attachments.Add(new Attachment(filePath));
                }
                message.Subject = emailSubject;
                if (emailBody != null)
                {
                    message.Body = emailBody;
                    message.IsBodyHtml = isHTML;
                }

                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return false;
            }
        }
    }
}
