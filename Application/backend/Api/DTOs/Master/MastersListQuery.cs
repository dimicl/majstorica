namespace backend.Api.DTOs.Master;

public class MastersListQuery
{
    public string? Search { get; set; }

    public string? Sort { get; set; }

    public string? Category { get; set; }

    public int? MinRating { get; set; }
}
