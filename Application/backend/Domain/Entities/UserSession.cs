using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class UserSession
{
    private UserSession()
    {
        Id = string.Empty;
    }

    public UserSession(
        string id,
        Guid userId,
        UserRole role,
        string connectionId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Session id cannot be empty.");
        if (userId == Guid.Empty)
            throw new DomainException("User id cannot be empty.");
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new DomainException("Connection id is required.");

        Id = id;
        UserId = userId;
        Role = role;
        ConnectionId = connectionId.Trim();
        LastSeen = DateTime.UtcNow;
    }

    /// <summary>Konstruktor za učitavanje iz persistence (Redis).</summary>
    public UserSession(
        string id,
        Guid userId,
        UserRole role,
        Guid? currentJobId,
        Guid? currentConversationId,
        string connectionId,
        DateTime lastSeen)
        : this(id, userId, role, connectionId)
    {
        CurrentJobId = currentJobId;
        CurrentConversationId = currentConversationId;
        LastSeen = lastSeen;
    }

    public string Id { get; private set; }

    public Guid UserId { get; private set; }

    public UserRole Role { get; private set; }

    public Guid? CurrentJobId { get; set; }

    public Guid? CurrentConversationId { get; set; }

    public string ConnectionId { get; set; } = string.Empty;

    public DateTime LastSeen { get; set; }

    public void Touch()
    {
        LastSeen = DateTime.UtcNow;
    }
}