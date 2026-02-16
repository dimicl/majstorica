namespace backend.Domain.Entities;

public class ChatConversation
{
    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid MasterId { get; private set; }

    public bool IsActive { get; private set; }

    protected ChatConversation() { }

    public ChatConversation(Guid jobId, Guid clientId, Guid masterId)
    {
        Id = Guid.NewGuid();
        JobId = jobId;
        ClientId = clientId;
        MasterId = masterId;
        IsActive = true;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------

    public void Close()
    {
        IsActive = false;
    }

    public static ChatConversation Rehydrate(
        Guid id,
        Guid jobId,
        Guid clientId,
        Guid masterId,
        bool isActive)
    {
        return new ChatConversation
        {
            Id = id,
            JobId = jobId,
            ClientId = clientId,
            MasterId = masterId,
            IsActive = isActive
        };
    }
}
