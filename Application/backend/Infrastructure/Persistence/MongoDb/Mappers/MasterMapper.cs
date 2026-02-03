using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;
public static class MasterMapper
{
    public static MasterDocument ToEntity(Master master)
    {
        return new MasterDocument
        {
            Id = master.UserId,
            UserId = master.UserId,
            Bio = master.Bio,
            Categories = master.Categories.ToList(),
            Rating = master.Rating,
            YearsExperience = master.YearsExperience,
            CreatedAt = master.CreatedAt,
            UpdatedAt = master.UpdatedAt
        };
    }

    public static Master ToDomain(MasterDocument doc)
    {
        return Master.Rehydrate(
            doc.UserId,
            doc.Bio,
            doc.Categories,
            doc.Rating,
            doc.YearsExperience,
            doc.CreatedAt,
            doc.UpdatedAt);
    }
}
