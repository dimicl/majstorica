using backend.Domain.Enums;
using backend.Domain.Events;
using backend.Domain.States;

namespace backend.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; }

    public Guid ClientId { get; private set; }
    public Guid? MasterId { get; private set; }

    public string Description { get; private set; }

    public JobStatus Status { get; private set; }

    private IJobState _state;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Job() { }

    public Job(Guid clientId, string description)
    {
        Id = Guid.NewGuid();
        ClientId = clientId;
        Description = description;
        Status = JobStatus.Created;

        _state = new CreatedState(this);
    }

    public static Job Rehydrate(Guid id, string description, string status)
    {
        var job = new Job();
        job.Id = id;
        job.Description = description;
        job.SetStateFromString(status);
        return job;
    }

    internal void SetStateFromString(string status)
    {
        Status = Enum.Parse<JobStatus>(status);

        _state = Status switch
        {
            JobStatus.Created => new CreatedState(this),
            JobStatus.InProgress => new InProgressState(this),
            JobStatus.Completed => new CompletedState(this),
            _ => throw new Exception("Nepoznat status posla")
        };
    } 

    // ------------------ DOMENSKE OPERACIJE ------------------

    public void AssignMaster(Guid masterId)
    {
        _state.AssignMaster(this, masterId);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void ChangeDescription(string description)
    {
        _state.ChangeDescription(this, description);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void Start()
    {
        _state.Start(this);
        AddEvent(new JobUpdatedEvent(Id));
    }

    public void Complete()
    {
        _state.Complete(this);
        AddEvent(new JobUpdatedEvent(Id));
    }


    // ------------------ INTERNAL HELPERS ------------------

    internal void SetMaster(Guid masterId)
    {
        MasterId = masterId;
    }

    internal void SetStatus(JobStatus status)
    {
        Status = status;
        _state = JobStateFactory.Create(status, this);
    }

    internal void ChangeDescriptionInternal(string description)
    {
        Description = description;
    }

    private void AddEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearEvents()
    {
        _domainEvents.Clear();
    }

}
