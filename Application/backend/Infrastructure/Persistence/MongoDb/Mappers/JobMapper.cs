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
            Description = job.Description,
            Price = job.Price,
            IsEmergency = job.IsEmergency,
            Status = job.Status.ToString()
        };
    }

    public static Job ToDomain(JobDocument doc)
    {
        return Job.Rehydrate(
            doc.Id,
            doc.ClientId,
            doc.MasterId,
            doc.Description,
            doc.Price,
            doc.IsEmergency,
            doc.Status);
    }
}
