using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class JobMapper
{
    public static JobDocument ToDocument(Job job)
    {
        return new JobDocument
        {
            Id = job.Id,
            ClientId = job.ClientUserId,
            AssignedMasterId = job.AssignedMasterId ?? Guid.Empty,
            Title = job.Title,
            Description = job.Description,
            PreferredDateUtc = job.PreferredDateUtc,
            Budget = job.Budget,
            IsEmergency = job.IsEmergency,
            Status = job.Status.ToString(),
            CreatedAt = job.CreatedAtUtc,
            UpdatedAt = job.UpdatedAtUtc
        };
    }

    public static Job ToDomain(JobDocument doc)
    {
        var createdAt = doc.CreatedAt ?? DateTime.UtcNow;
        var updatedAt = doc.UpdatedAt ?? createdAt;
        var assignedMaster = doc.AssignedMasterId == Guid.Empty
            ? (Guid?)null
            : doc.AssignedMasterId;

        return new Job(
            doc.Id,
            doc.ClientId,
            doc.Title ?? string.Empty,
            doc.Description ?? string.Empty,
            doc.IsEmergency,
            createdAt,
            updatedAt,
            doc.PreferredDateUtc,
            doc.Budget,
            doc.Status,
            assignedMaster);
    }
}
