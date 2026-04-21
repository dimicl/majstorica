using backend.Api.DTOs.User;

namespace backend.Api.DTOs.Master;

/// <summary>Profil majstora (trenutni korisnik = majstor): user + kategorija i ocena.</summary>
public class MasterProfileResponse
{
    public UserRequest User { get; init; } = default!;
    /// <summary>Prikazno ime kategorije (npr. "Električar") ili null ako nije izabrana.</summary>
    public string? Category { get; init; }
    public decimal? Rating { get; init; }
    /// <summary>Za ulogu CompanyWorker — firma u kojoj radi.</summary>
    public Guid? EmployerCompanyId { get; init; }
    public string? EmployerCompanyName { get; init; }
    public int YearsOfExperience { get; init; }
    public decimal HourlyRateAmount { get; init; }
    public string HourlyRateCurrency { get; init; } = "RSD";
    public int TotalReviews { get; init; }
}
