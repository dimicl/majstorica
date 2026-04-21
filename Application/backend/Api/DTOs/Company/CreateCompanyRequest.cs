using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs.Company;

public class CreateCompanyRequest
{
    [Required(ErrorMessage = "Naziv firme je obavezan")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Naziv mora biti između 2 i 150 karaktera")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "Telefon firme je obavezan")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon mora imati tačno 10 cifara")]
    public string PhoneNumber { get; set; } = default!;

    [Required(ErrorMessage = "Email firme je obavezan")]
    [EmailAddress(ErrorMessage = "Email firme nije validan")]
    public string Email { get; set; } = default!;

    [StringLength(200)]
    public string? Street { get; set; }

    [StringLength(100)]
    public string? City { get; set; }
}
