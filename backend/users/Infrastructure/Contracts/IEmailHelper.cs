namespace com.touir.expenses.Users.Infrastructure.Contracts
{
    public interface IEmailHelper
    {
        bool SendEmail(
            string? recipientTo,
            string? recipientCC,
            string? recipientBCC,
            string emailSubject,
            string? emailBody,
            bool isHTML = false,
            ICollection<string>? attachments = null);
    }
}
