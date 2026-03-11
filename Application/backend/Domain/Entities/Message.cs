using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class Message
{
    private Message()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Message(
        Guid id,
        Guid conversationId,
        Guid senderUserId,
        MessageType type,
        string content,
        DateTime sentAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("Message id cannot be empty.");

        if (conversationId == Guid.Empty)
            throw new DomainException("Conversation id cannot be empty.");

        if (senderUserId == Guid.Empty)
            throw new DomainException("Sender user id cannot be empty.");

        Id = id;
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Type = type;

        SetContent(content);

        SentAtUtc = sentAtUtc;
        UpdatedAtUtc = sentAtUtc;

        IsEdited = false;
        IsDeleted = false;
        EditedAtUtc = null;
        DeletedAtUtc = null;
    }

    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public Guid SenderUserId { get; private set; }

    public MessageType Type { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public bool IsEdited { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime SentAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? EditedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public void Edit(string content, DateTime editedAtUtc)
    {
        EnsureNotDeleted();

        if (Type == MessageType.System)
            throw new DomainException("System message cannot be edited.");

        SetContent(content);

        IsEdited = true;
        EditedAtUtc = editedAtUtc;

        Touch();
    }

    public void Delete(DateTime deletedAtUtc)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        Content = string.Empty;

        Touch();
    }

    private void SetContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Message content is required.");

        var value = content.Trim();

        if (value.Length > 4000)
            throw new DomainException("Message content cannot be longer than 4000 characters.");

        Content = value;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException("Deleted message cannot be modified.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}