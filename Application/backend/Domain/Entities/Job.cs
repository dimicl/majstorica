using backend.Domain.Enums;
using backend.Domain.Events;
using backend.Domain.Exceptions;
using backend.Domain.States;
using backend.Domain.Strategies;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class Job
{
    private readonly List<DomainEvent> _domainEvents = new();

    private Job()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Job(
        Guid id,
        Guid clientUserId,
        string title,
        string description,
        string serviceCategory,
        Address serviceAddress,
        JobRequestType requestType,
        ExecutorType executorType,
        DateTime createdAtUtc,
        bool isEmergency,
        DateTime? preferredDateUtc = null,
        string? preferredTimeNote = null,
        Money? budget = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Job id cannot be empty.");

        if (clientUserId == Guid.Empty)
            throw new DomainException("Client user id cannot be empty.");

        ValidateRequestConfiguration(requestType, executorType);

        Id = id;
        ClientUserId = clientUserId;

        SetTitle(title);
        SetDescription(description);
        SetServiceCategory(serviceCategory);
        SetServiceAddress(serviceAddress);
        SetPreferredDate(preferredDateUtc);
        SetPreferredTimeNote(preferredTimeNote);

        RequestType = requestType;
        ExecutorType = executorType;
        Budget = budget;

        Status = JobStatus.Draft;

        AssignedMasterId = null;
        AssignedCompanyId = null;
        FinalPrice = null;
        CancellationReason = null;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        IsEmergency = isEmergency;

        HasStoredServiceCategory = true;
    }

    /// <summary>Konstruktor za učitavanje iz persistence (Mongo).</summary>
    public Job(
        Guid id,
        Guid clientUserId,
        string title,
        string description,
        bool isEmergency,
        DateTime createdAtUtc,
        DateTime updatedAtUtc,
        DateTime? preferredDateUtc,
        Money? budget,
        string? serviceCategory,
        string status,
        Guid? assignedMasterId)
        : this(
            id,
            clientUserId,
            title,
            description,
            string.IsNullOrWhiteSpace(serviceCategory) ? "Ostalo" : serviceCategory,
            new Address("Nepoznata adresa", "Nepoznat grad"),
            JobRequestType.Marketplace,
            ExecutorType.Any,
            createdAtUtc,
            isEmergency,
            preferredDateUtc,
            null,
            budget)
    {
        UpdatedAtUtc = updatedAtUtc;
        AssignedMasterId = assignedMasterId == Guid.Empty ? null : assignedMasterId;
        if (Enum.TryParse<JobStatus>(status, out var parsed))
            Status = parsed;

        HasStoredServiceCategory = !string.IsNullOrWhiteSpace(serviceCategory);
    }

    public Guid Id { get; private set; }

    public Guid ClientUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string ServiceCategory { get; private set; } = string.Empty;

    /// <summary>True ako je kategorija stvarno upisana u skladište (Mongo). Starim zapisima bez polja odgovara false — u API-ju ne treba prikazivati lažno "Ostalo".</summary>
    public bool HasStoredServiceCategory { get; private set; }

    public bool IsEmergency { get; private set; }

    public Address ServiceAddress { get; private set; } = null!;

    public DateTime? PreferredDateUtc { get; private set; }

    public string? PreferredTimeNote { get; private set; }

    public Money? Budget { get; private set; }

    public Money? FinalPrice { get; private set; }

    public JobRequestType RequestType { get; private set; }

    public ExecutorType ExecutorType { get; private set; }

    public JobStatus Status { get; private set; }

    public Guid? AssignedMasterId { get; private set; }

    public Guid? AssignedCompanyId { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public DateTime? AssignedAtUtc { get; private set; }

    public DateTime? StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? CancelledAtUtc { get; private set; }

    public DateTime? ExpiredAtUtc { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public bool IsDraft() => Status == JobStatus.Draft;
    public bool IsPublished() => Status == JobStatus.Published;
    public bool IsAssigned() => Status == JobStatus.Assigned;
    public bool IsInProgress() => Status == JobStatus.InProgress;
    public bool IsCompleted() => Status == JobStatus.Completed;
    public bool IsCancelled() => Status == JobStatus.Cancelled;
    public bool IsExpired() => Status == JobStatus.Expired;

    public void UpdateBasicInfo(
        string title,
        string description,
        string serviceCategory,
        Address serviceAddress,
        Money? budget)
    {
        EnsureEditable();

        SetTitle(title);
        SetDescription(description);
        SetServiceCategory(serviceCategory);
        SetServiceAddress(serviceAddress);

        Budget = budget;

        Touch();
    }

    public void ApplyBookingStrategy(
        IBookingStrategy bookingStrategy,
        DateTime? preferredDateUtc,
        string? preferredTimeNote)
    {
        EnsureEditable();

        if (bookingStrategy is null)
            throw new DomainException("Booking strategy is required.");

        bookingStrategy.Apply(this, preferredDateUtc, preferredTimeNote);

        Touch();
    }

    public void Publish(DateTime publishedAtUtc)
    {
        GetState().CanPublish();

        Status = JobStatus.Published;
        PublishedAtUtc = publishedAtUtc;

        AddDomainEvent(new JobPublishedEvent(Id, ClientUserId, publishedAtUtc));

        Touch();
    }

    public void AssignToMasterWithStrategy(
        IAssignmentStrategy assignmentStrategy,
        Guid masterId,
        DateTime assignedAtUtc)
    {
        if (assignmentStrategy is null)
            throw new DomainException("Assignment strategy is required.");

        assignmentStrategy.AssignToMaster(this, masterId, assignedAtUtc);
    }

    public void AssignToCompanyWithStrategy(
        IAssignmentStrategy assignmentStrategy,
        Guid companyId,
        DateTime assignedAtUtc)
    {
        if (assignmentStrategy is null)
            throw new DomainException("Assignment strategy is required.");

        assignmentStrategy.AssignToCompany(this, companyId, assignedAtUtc);
    }

    public void AssignToMaster(Guid masterId, DateTime assignedAtUtc)
    {
        if (masterId == Guid.Empty)
            throw new DomainException("Assigned master id cannot be empty.");

        GetState().CanAssign();

        if (ExecutorType == ExecutorType.Company)
            throw new DomainException("This job is intended for company executor.");

        AssignedMasterId = masterId;
        AssignedCompanyId = null;

        Status = JobStatus.Assigned;
        AssignedAtUtc = assignedAtUtc;

        AddDomainEvent(new JobAssignedEvent(
            Id,
            ClientUserId,
            assignedMasterId: masterId,
            assignedCompanyId: null,
            assignedAtUtc));

        Touch();
    }

    public void AssignToCompany(Guid companyId, DateTime assignedAtUtc)
    {
        if (companyId == Guid.Empty)
            throw new DomainException("Assigned company id cannot be empty.");

        GetState().CanAssign();

        if (ExecutorType == ExecutorType.Master)
            throw new DomainException("This job is intended for master executor.");

        AssignedCompanyId = companyId;
        AssignedMasterId = null;

        Status = JobStatus.Assigned;
        AssignedAtUtc = assignedAtUtc;

        AddDomainEvent(new JobAssignedEvent(
            Id,
            ClientUserId,
            assignedMasterId: null,
            assignedCompanyId: companyId,
            assignedAtUtc));

        Touch();
    }

    public void Start(DateTime startedAtUtc)
    {
        GetState().CanStart();

        if (AssignedMasterId is null && AssignedCompanyId is null)
            throw new DomainException("Job must have assigned executor before start.");

        Status = JobStatus.InProgress;
        StartedAtUtc = startedAtUtc;

        Touch();
    }

    public void Complete(DateTime completedAtUtc, Money? finalPrice = null)
    {
        GetState().CanComplete();

        FinalPrice = finalPrice;
        Status = JobStatus.Completed;
        CompletedAtUtc = completedAtUtc;

        AddDomainEvent(new JobCompletedEvent(
            Id,
            ClientUserId,
            AssignedMasterId,
            AssignedCompanyId,
            completedAtUtc,
            finalPrice));

        Touch();
    }

    public void Cancel(DateTime cancelledAtUtc, string? cancellationReason = null)
    {
        GetState().CanCancel();

        Status = JobStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = NormalizeOptionalText(cancellationReason, 1000);

        Touch();
    }

    public void Expire(DateTime expiredAtUtc)
    {
        GetState().CanExpire();

        Status = JobStatus.Expired;
        ExpiredAtUtc = expiredAtUtc;

        Touch();
    }

    public void SetBudget(Money? budget)
    {
        EnsureEditable();

        Budget = budget;
        Touch();
    }

    public void SetPreferredDate(DateTime? preferredDateUtc)
    {
        PreferredDateUtc = preferredDateUtc;
    }

    public void SetPreferredTimeNote(string? preferredTimeNote)
    {
        PreferredTimeNote = NormalizeOptionalText(preferredTimeNote, 200);
    }

    public void SetServiceAddress(Address serviceAddress)
    {
        ServiceAddress = serviceAddress ?? throw new DomainException("Service address is required.");
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private IJobState GetState()
    {
        return JobStateFactory.Create(Status);
    }

    private void EnsureEditable()
    {
        if (Status != JobStatus.Draft)
            throw new DomainException("Only draft job can be edited.");
    }

    private void AddDomainEvent(DomainEvent domainEvent)
    {
        if (domainEvent is null)
            throw new DomainException("Domain event is required.");

        _domainEvents.Add(domainEvent);
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Job title is required.");

        var value = title.Trim();

        if (value.Length > 150)
            throw new DomainException("Job title cannot be longer than 150 characters.");

        Title = value;
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Job description is required.");

        var value = description.Trim();

        if (value.Length > 4000)
            throw new DomainException("Job description cannot be longer than 4000 characters.");

        Description = value;
    }

    private void SetServiceCategory(string serviceCategory)
    {
        if (string.IsNullOrWhiteSpace(serviceCategory))
            throw new DomainException("Service category is required.");

        var value = serviceCategory.Trim();

        if (value.Length > 100)
            throw new DomainException("Service category is too long.");

        ServiceCategory = value;
    }

    private static void ValidateRequestConfiguration(
        JobRequestType requestType,
        ExecutorType executorType)
    {
        if (requestType == JobRequestType.DirectInvitation && executorType == ExecutorType.Any)
            throw new DomainException("Direct invitation job cannot target Any executor type.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
            throw new DomainException($"Text cannot be longer than {maxLength} characters.");

        return normalized;
    }
}