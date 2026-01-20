using backend.Domain.Entities;

namespace backend.Domain.States;

public interface IJobState
{
    void AssignMaster(Job job, Guid masterId);
    void ChangeDescription(Job job, string description);
    void Start(Job job);
    void Complete(Job job);
}
