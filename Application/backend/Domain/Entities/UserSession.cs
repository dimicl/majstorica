using backend.Domain.Enums;

namespace backend.Domain.Entities;

public class UserSession
{
    public string Id { get; internal set; } = default!;
    public Guid UserId { get; internal set; }
    public UserRole Role { get; internal set; }
    public Guid? CurrentJobId { get; internal set; }
    public Guid? CurrentConversationId { get; internal set; }
    public string ConnectionId { get; internal set; } = default!;
    public DateTime LastSeen { get; internal set; } = DateTime.UtcNow;

    protected UserSession() { }

    public UserSession(string id, Guid userId, UserRole role, string connectionId)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        UserId = userId;
        Role = role;
        ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        LastSeen = DateTime.UtcNow;
    }

    public static UserSession FromPersistence(
        string id,
        Guid userId,
        UserRole role,
        Guid? currentJobId,
        Guid? currentConversationId,
        string connectionId,
        DateTime lastSeen)
    {
        return new UserSession
        {
            Id = id,
            UserId = userId,
            Role = role,
            CurrentJobId = currentJobId,
            CurrentConversationId = currentConversationId,
            ConnectionId = connectionId,
            LastSeen = lastSeen
        };
    }

    public void SetCurrentJob(Guid? jobId)
    {
        CurrentJobId = jobId;
        LastSeen = DateTime.UtcNow;
    }

    public void SetCurrentConversation(Guid? conversationId)
    {
        CurrentConversationId = conversationId;
        LastSeen = DateTime.UtcNow;
    }

    public void Touch()
    {
        LastSeen = DateTime.UtcNow;
    }
}
