using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Strategies;

public class MarketplaceAssignmentStrategy : IAssignmentStrategy
{
    public void AssignToMaster(Job job, Guid masterId, DateTime assignedAtUtc)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        if (job.RequestType != JobRequestType.Marketplace)
            throw new DomainException("Marketplace strategy can only assign marketplace jobs.");

        job.AssignToMaster(masterId, assignedAtUtc);
    }

    public void AssignToCompany(Job job, Guid companyId, DateTime assignedAtUtc)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        if (job.RequestType != JobRequestType.Marketplace)
            throw new DomainException("Marketplace strategy can only assign marketplace jobs.");

        job.AssignToCompany(companyId, assignedAtUtc);
    }
}