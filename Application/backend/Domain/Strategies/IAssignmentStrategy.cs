using backend.Domain.Entities;

namespace backend.Domain.Strategies;

public interface IAssignmentStrategy
{
    void AssignToMaster(Job job, Guid masterId, DateTime assignedAtUtc);

    void AssignToCompany(Job job, Guid companyId, DateTime assignedAtUtc);
}