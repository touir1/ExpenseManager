namespace Touir.ExpensesManager.Users.Infrastructure.Contracts
{
    public interface IEmailService
    {
        bool SendEmail(
            string? recipientTo = null,
            string? recipientCC = null,
            string? recipientBCC = null,
            string? emailSubject = null,
            string? emailBody = null,
            bool isHTML = false,
            ICollection<string>? attachments = null);
    }
}
