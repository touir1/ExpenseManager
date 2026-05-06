using Touir.ExpensesManager.Users.Messaging.Messages;

namespace Touir.ExpensesManager.Users.Messaging.Publishers
{
    public interface IUserEventPublisher
    {
        void Publish(UserEventMessage message);
        void PublishRaw(string eventType, string jsonPayload, string messageId);
    }
}
