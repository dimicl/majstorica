using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class UserSession
{
    private UserSession()
    {
        // potrebno za serializer / mapper / Mongo
    }

    public UserSession(
        Guid id,
        Guid userId,
        string token,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("Session id cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("User id cannot be empty.");

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Session token is required.");

        if (expiresAtUtc <= createdAtUtc)
            throw new DomainException("Session expiration must be after creation time.");

        Id = id;
        UserId = userId;
        Token = token.Trim();

        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;

        Status = SessionStatus.Active;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public SessionStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public bool IsActive() => Status == SessionStatus.Active;

    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;

    public bool IsUsable(DateTime nowUtc)
    {
        return Status == SessionStatus.Active && ExpiresAtUtc > nowUtc;
    }

    public void ReplaceToken(string token)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("Session token is required.");

        Token = token.Trim();
    }

    public void Extend(DateTime newExpirationUtc)
    {
        EnsureActive();

        if (newExpirationUtc <= ExpiresAtUtc)
            throw new DomainException("New expiration must be later than current expiration.");

        ExpiresAtUtc = newExpirationUtc;
    }

    public void Revoke()
    {
        if (Status == SessionStatus.Revoked)
            return;

        Status = SessionStatus.Revoked;
    }

    public void MarkExpired()
    {
        if (Status == SessionStatus.Expired)
            return;

        Status = SessionStatus.Expired;
    }

    private void EnsureActive()
    {
        if (Status != SessionStatus.Active)
            throw new DomainException("Only active session can be modified.");
    }
}