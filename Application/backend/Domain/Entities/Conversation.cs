using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Entities;

public class Conversation
{
    private Conversation()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Conversation(
        Guid id,
        Guid clientUserId,
        ConversationType type,
        DateTime createdAtUtc,
        Guid? masterUserId = null,
        Guid? companyId = null,
        Guid? jobId = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Conversation id cannot be empty.");

        if (clientUserId == Guid.Empty)
            throw new DomainException("Client user id cannot be empty.");

        ValidateParticipant(masterUserId, companyId);

        Id = id;
        ClientUserId = clientUserId;
        MasterUserId = masterUserId;
        CompanyId = companyId;
        JobId = jobId;
        Type = type;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        LastMessageAtUtc = null;
        IsClosed = false;
    }

    public Guid Id { get; private set; }

    public Guid ClientUserId { get; private set; }

    public Guid? MasterUserId { get; private set; }

    public Guid? CompanyId { get; private set; }

    public Guid? JobId { get; private set; }

    public ConversationType Type { get; private set; }

    public bool IsClosed { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? LastMessageAtUtc { get; private set; }

    public bool IsWithMaster() => MasterUserId.HasValue;

    public bool IsWithCompany() => CompanyId.HasValue;

    public bool IsJobRelated() => Type == ConversationType.JobRelated;

    public void AttachToJob(Guid jobId)
    {
        if (jobId == Guid.Empty)
            throw new DomainException("Job id cannot be empty.");

        JobId = jobId;
        Touch();
    }

    public void MarkMessageSent(DateTime sentAtUtc)
    {
        EnsureOpen();

        LastMessageAtUtc = sentAtUtc;
        Touch();
    }

    public void Close()
    {
        if (IsClosed)
            return;

        IsClosed = true;
        Touch();
    }

    public void Reopen()
    {
        if (!IsClosed)
            return;

        IsClosed = false;
        Touch();
    }

    private void EnsureOpen()
    {
        if (IsClosed)
            throw new DomainException("Conversation is closed.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateParticipant(Guid? masterUserId, Guid? companyId)
    {
        var hasMaster = masterUserId.HasValue && masterUserId.Value != Guid.Empty;
        var hasCompany = companyId.HasValue && companyId.Value != Guid.Empty;

        if (!hasMaster && !hasCompany)
            throw new DomainException("Conversation must have either master user id or company id.");

        if (hasMaster && hasCompany)
            throw new DomainException("Conversation cannot be linked to both master and company.");
    }
}