using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Domain.Specifications;

public class ActiveJobSpecification : ISpecification<Job>
{
    public bool IsSatisfiedBy(Job job)
    {
        if (job is null)
            return false;

        return job.Status == JobStatus.Published
               || job.Status == JobStatus.Assigned
               || job.Status == JobStatus.InProgress;
    }
}