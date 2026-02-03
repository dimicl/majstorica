using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class ClientMapper
{
    public static ClientDocument ToDocument(Client client)
    {
        return new ClientDocument
        {
            UserId = client.UserId,
            Phone = client.Phone,
            DeliveryAddress = client.DeliveryAddress,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt
        };
    }

    public static Client ToDomain(ClientDocument doc)
    {
        return Client.Rehydrate(
            doc.UserId,
            doc.Phone,
            doc.DeliveryAddress,
            doc.CreatedAt,
            doc.UpdatedAt);
    }
}
