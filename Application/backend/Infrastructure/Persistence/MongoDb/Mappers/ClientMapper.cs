using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class ClientMapper
{
    public static ClientDocument ToDocument(Client client)
    {
        return new ClientDocument
        {
            Id = client.Id,
            UserId = client.UserId,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }

    public static Client ToDomain(ClientDocument doc)
    {
        return Client.Rehydrate(
            doc.Id,
            doc.UserId,
            doc.CreatedAt,
            doc.UpdatedAt);
    }
}
