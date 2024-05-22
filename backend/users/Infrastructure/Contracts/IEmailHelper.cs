namespace com.touir.expenses.Users.Infrastructure.Contracts
{
    public interface IEmailHelper
    {
        bool SendEmail(
            string? recipientTo = null,
            string? recipientCC = null,
            string? recipientBCC = null,
            string emailSubject = null,
            string? emailBody = null,
            bool isHTML = false,
            ICollection<string>? attachments = null);
        bool ValidateEmail(string email);

        string GetEmailTemplate(string templateKey, Dictionary<string, string> parameters);
    }
}
