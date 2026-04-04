using backend.Domain.Entities;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class MasterMapper
{
    public static MasterProfileDocument ToDocument(MasterProfile profile)
    {
        return new MasterProfileDocument
        {
            Headline = profile.Headline,
            Description = profile.Description,
            YearsOfExperience = profile.YearsOfExperience,
            HourlyRateAmount = profile.HourlyRate.Amount,
            HourlyRateCurrency = profile.HourlyRate.Currency,
            IsAvailable = profile.IsAvailable,
            AverageRatingValue = profile.AverageRating?.Value,
            TotalJobsCompleted = profile.TotalJobsCompleted,
            TotalReviews = profile.TotalReviews,
            ServiceCategories = profile.ServiceCategories.ToList(),
            ServiceZones = profile.ServiceZones.ToList()
        };
    }

    public static MasterProfile ToDomain(MasterProfileDocument doc)
    {
        // Prazne liste iz Mongo-a ne smeju u SetServiceCategories/SetServiceZones (bacaju "mora bar jedna").
        var categories = doc.ServiceCategories is { Count: > 0 } ? doc.ServiceCategories : null;
        var zones = doc.ServiceZones is { Count: > 0 } ? doc.ServiceZones : null;

        return new MasterProfile(
            doc.Headline,
            doc.Description,
            doc.YearsOfExperience,
            doc.HourlyRateAmount,
            doc.HourlyRateCurrency ?? "RSD",
            doc.IsAvailable,
            doc.AverageRatingValue,
            doc.TotalJobsCompleted,
            doc.TotalReviews,
            categories,
            zones);
    }
}
