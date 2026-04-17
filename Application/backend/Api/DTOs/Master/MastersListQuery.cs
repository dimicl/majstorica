namespace backend.Api.DTOs.Master;

public class MastersListQuery
{
    public string? Search { get; set; }

    public string? Sort { get; set; }

    public string? Category { get; set; }

    public int? MinRating { get; set; }

    /// <summary>all | masters | companies</summary>
    public string? EntityType { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}
