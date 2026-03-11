using backend.Domain.Enums;

namespace backend.Domain.States;

public interface IJobState
{
    JobStatus Status { get; }

    void CanPublish();
    void CanAssign();
    void CanStart();
    void CanComplete();
    void CanCancel();
    void CanExpire();
}