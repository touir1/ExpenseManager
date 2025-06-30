﻿using com.touir.expenses.Users.Infrastructure.Contracts;
using com.touir.expenses.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace com.touir.expenses.Users.Infrastructure
{
    public class EmailHelper : IEmailHelper
    {
        private readonly string? sender;
        private readonly string? senderPassword;
        private readonly string? host;
        private readonly int? port;
        public EmailHelper(IOptions<EmailOptions> emailOptions) 
        {
            this.sender = emailOptions.Value.Email;
            this.senderPassword = emailOptions.Value.Password;
            this.host = emailOptions.Value.Host;
            this.port = emailOptions.Value.Port;
        }

        public bool SendEmail(
            string? recipientTo = null, 
            string? recipientCC = null, 
            string? recipientBCC = null, 
            string emailSubject = null, 
            string? emailBody = null, 
            bool isHTML = false,
            ICollection<string>? attachments = null) 
        {
            try { 
                using (var client = new SmtpClient()) 
                {
                    client.Host = host;
                    client.Port = port.Value;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(sender, senderPassword);

                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(sender);
                        if(recipientTo != null)
                            message.To.Add(recipientTo);
                        if(recipientCC != null)
                            message.CC.Add(recipientCC);
                        if(recipientBCC != null)
                            message.Bcc.Add(recipientBCC);
                        if(attachments != null && attachments.Count > 0)
                        {
                            foreach(string filePath in attachments)
                            {
                                message.Attachments.Add(new Attachment(filePath));
                            }
                        }
                        message.Subject = emailSubject;
                        if(emailBody != null)
                        {
                            message.Body = emailBody;
                            message.IsBodyHtml = isHTML;
                        }

                        client.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
            return false;
        }

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
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $"Assets\\EmailTemplates\\{templateKey}.html");
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
