using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class JobMapper
{
    public static JobDocument ToDocument(Job job)
    {
        return new JobDocument
        {
            Id = job.Id,
            ClientId = job.ClientId,
            MasterId = job.MasterId,
            Title = job.Title,
            Description = job.Description,
            ScheduledDate = job.ScheduledDate,
            Price = job.Price,
            IsEmergency = job.IsEmergency,
            Status = job.Status.ToString(),
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt
        };
    }

    public static Job ToDomain(JobDocument doc)
    {
        var createdAt = doc.CreatedAt ?? DateTime.UtcNow;
        var updatedAt = doc.UpdatedAt ?? DateTime.UtcNow;
        return Job.Rehydrate(
            doc.Id,
            doc.ClientId,
            doc.MasterId,
            doc.Title ?? string.Empty,
            doc.Description ?? string.Empty,
            doc.ScheduledDate,
            doc.Price,
            doc.IsEmergency,
            doc.Status,
            createdAt,
            updatedAt);
    }
}
