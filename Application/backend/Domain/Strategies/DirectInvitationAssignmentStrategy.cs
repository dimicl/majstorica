using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Exceptions;

namespace backend.Domain.Strategies;

public class DirectInvitationAssignmentStrategy : IAssignmentStrategy
{
    public void AssignToMaster(Job job, Guid masterId, DateTime assignedAtUtc)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        if (job.RequestType != JobRequestType.DirectInvitation)
            throw new DomainException("Direct invitation strategy can only assign direct invitation jobs.");

        job.AssignToMaster(masterId, assignedAtUtc);
    }

    public void AssignToCompany(Job job, Guid companyId, DateTime assignedAtUtc)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        if (job.RequestType != JobRequestType.DirectInvitation)
            throw new DomainException("Direct invitation strategy can only assign direct invitation jobs.");

        job.AssignToCompany(companyId, assignedAtUtc);
    }
}