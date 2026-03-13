using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class ClientMapper
{
    public static ClientDocument ToDocument(Guid userId, ClientProfile client)
    {
        return new ClientDocument
        {
            Id = userId,
            PreferredContactPhone = client.PreferredContactPhone,
            Notes = client.Notes,
            TotalJobsPosted = client.TotalJobsPosted,
            CompletedJobs = client.CompletedJobs
        };
    }

    public static ClientProfile ToDomain(ClientDocument doc)
    {
        return new ClientProfile(
            doc.PreferredContactPhone,
            doc.Notes,
            doc.TotalJobsPosted,
            doc.CompletedJobs);
    }
}
