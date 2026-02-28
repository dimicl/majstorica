using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class MasterMapper
{
    public static MasterDocument ToEntity(Master master)
    {
        return new MasterDocument
        {
            Id = master.Id,
            UserId = master.UserId,
            Bio = master.Bio,
            Category = master.Category is null ? null : (int)master.Category.Value,
            Rating = master.Rating,
            YearsExperience = master.YearsExperience,
            CreatedAt = master.CreatedAt,
            UpdatedAt = master.UpdatedAt
        };
    }

    public static Master ToDomain(MasterDocument doc)
    {
        var category = MasterCategoryDisplay.FromValue(doc.Category);

        return Master.Rehydrate(
            doc.Id,
            doc.UserId,
            doc.Bio,
            category,
            doc.Rating,
            doc.YearsExperience,
            doc.CreatedAt,
            doc.UpdatedAt);
    }
}
