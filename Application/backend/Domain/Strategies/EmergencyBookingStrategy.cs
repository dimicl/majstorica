using backend.Domain.Entities;
using backend.Domain.Exceptions;

namespace backend.Domain.Strategies;

public class EmergencyBookingStrategy : IBookingStrategy
{
    public void Apply(Job job, DateTime? preferredDateUtc, string? preferredTimeNote)
    {
        if (job is null)
            throw new DomainException("Job is required.");

        if (!preferredDateUtc.HasValue)
            throw new DomainException("Emergency booking must have preferred date.");

        var normalizedNote = string.IsNullOrWhiteSpace(preferredTimeNote)
            ? "Emergency request"
            : preferredTimeNote.Trim();

        job.SetPreferredDate(preferredDateUtc);
        job.SetPreferredTimeNote(normalizedNote);
    }
}