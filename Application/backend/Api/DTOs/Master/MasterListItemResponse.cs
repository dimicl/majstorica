namespace backend.Api.DTOs.Master;

public class MasterListItemResponse
{
    /// <summary>master | company</summary>
    public string Kind { get; init; } = "master";

    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string? Category { get; init; }
    public decimal? Rating { get; init; }

    /// <summary>Za filtriranje po kategoriji (majstor + firma).</summary>
    public List<string>? ServiceCategories { get; init; }

    /// <summary>Za kind=company.</summary>
    public string? CompanyName { get; init; }

    public string? Description { get; init; }
    public string? City { get; init; }
    public string? Email { get; init; }
}
