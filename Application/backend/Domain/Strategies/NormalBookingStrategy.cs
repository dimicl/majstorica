using backend.Domain.Entities;
using backend.Domain.Exceptions;

namespace backend.Domain.Strategies;

public class NormalBookingStrategy : IBookingStrategy
{
    public void Apply(Job job, DateTime? preferredDateUtc, string? preferredTimeNote)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        job.SetPreferredDate(preferredDateUtc);
        job.SetPreferredTimeNote(preferredTimeNote);
    }
}