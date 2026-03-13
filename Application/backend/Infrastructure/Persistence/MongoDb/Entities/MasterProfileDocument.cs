namespace backend.Infrastructure.Persistence.MongoDb.Entities;

public class MasterProfileDocument
{
    public string Headline { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRateAmount { get; set; }
    public string HourlyRateCurrency { get; set; } = "RSD";
    public bool IsAvailable { get; set; }
    public decimal? AverageRatingValue { get; set; }
    public int TotalJobsCompleted { get; set; }
    public int TotalReviews { get; set; }
    public List<string> ServiceCategories { get; set; } = new();
    public List<string> ServiceZones { get; set; } = new();
}
