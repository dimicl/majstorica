namespace backend.Api.DTOs.Jobs;

public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = default!;
    public DateTime? ScheduledDate { get; set; }
    public decimal? Price { get; set; }
    public bool IsEmergency { get; set; }    
    public string? ServiceCategory { get; set; }
}
