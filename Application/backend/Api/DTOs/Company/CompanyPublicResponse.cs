namespace backend.Api.DTOs.Company;

/// <summary>Javni prikaz firme za klijente (lista majstora / detalj).</summary>
public class CompanyPublicResponse
{
    public Guid Id { get; init; }

    /// <summary>Korisnik (vlasnik firme) za chat i zahtev za posao.</summary>
    public Guid OwnerUserId { get; init; }

    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? City { get; init; }
    public List<string> ServiceCategories { get; init; } = new();
}
