namespace backend.Api.DTOs.Jobs;

public class CreateJobRequest
{
    public string Description { get; set; } = default!;
    public decimal? Price { get; set; }
    public bool IsEmergency { get; set; }
}
