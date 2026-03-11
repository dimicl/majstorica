using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Domain.Specifications;

public class CompanyEligibleForJobSpecification
{
    public bool IsSatisfiedBy(Company company, Job job)
    {
        if (company is null || job is null)
            return false;

        var executorMatches =
            job.ExecutorType == ExecutorType.Company ||
            job.ExecutorType == ExecutorType.Any;

        var categoryMatches = company.ServiceCategories.Any(x =>
            x.Equals(job.ServiceCategory, StringComparison.OrdinalIgnoreCase));

        return company.IsActive
               && executorMatches
               && categoryMatches;
    }
}