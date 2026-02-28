namespace backend.Api.DTOs.Master;

public class MasterListItemResponse
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Category { get; init; }
    public decimal? Rating { get; init; }
}
