using backend.Domain.Enums;

namespace backend.Domain.Events;

public class MessageSentEvent : DomainEvent
{
    public MessageSentEvent(
        Guid messageId,
        Guid conversationId,
        Guid senderUserId,
        MessageType messageType,
        DateTime sentAtUtc)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message id cannot be empty.", nameof(messageId));

        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation id cannot be empty.", nameof(conversationId));

        if (senderUserId == Guid.Empty)
            throw new ArgumentException("Sender user id cannot be empty.", nameof(senderUserId));

        MessageId = messageId;
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        MessageType = messageType;
        SentAtUtc = sentAtUtc;
    }

    public Guid MessageId { get; }

    public Guid ConversationId { get; }

    public Guid SenderUserId { get; }

    public MessageType MessageType { get; }

    public DateTime SentAtUtc { get; }
}