using Touir.ExpensesManager.Expenses.Messaging.Messages;

namespace Touir.ExpensesManager.Expenses.Messaging.Publishers
{
    public interface IFamilyEventPublisher
    {
        void Publish(FamilyEventMessage message);
        void PublishRaw(string eventType, string jsonPayload, string messageId);
    }
}
