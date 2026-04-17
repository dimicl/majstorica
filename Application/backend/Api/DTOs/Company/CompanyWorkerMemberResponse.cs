namespace backend.Api.DTOs.Company;

public class CompanyWorkerMemberResponse
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Headline { get; set; }
    public string? Description { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRateAmount { get; set; }
    public string HourlyRateCurrency { get; set; } = "RSD";
    public bool IsAvailable { get; set; }
    public List<string> ServiceCategories { get; set; } = new();
    public List<string> ServiceZones { get; set; } = new();
    public decimal? AverageRating { get; set; }
    public int TotalJobsCompleted { get; set; }
    public int TotalReviews { get; set; }
}
