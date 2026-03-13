namespace backend.Api.DTOs.Master;

public class MastersGraphSearchQuery
{
    public string? CategoryNames { get; set; }

    public string? ZoneIds { get; set; }

    public decimal? MinRating { get; set; }

    public int Limit { get; set; } = 20;
}
