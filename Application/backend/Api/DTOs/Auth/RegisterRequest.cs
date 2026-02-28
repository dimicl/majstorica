using System.ComponentModel.DataAnnotations;
using backend.Domain.Enums;

namespace backend.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Ime je obavezno")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Ime mora biti između 2 i 20 karaktera")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Prezime je obavezno")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Prezime mora biti između 2 i 50 karaktera")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Email je obavezan")]
    [EmailAddress(ErrorMessage = "Email adresa nije validna")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Korisničko ime je obavezno")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Korisničko ime mora biti između 3 i 30 karaktera")]
    public string Username { get; set; } = default!;

    [Required(ErrorMessage = "Lozinka je obavezna")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 karaktera")]
    public string Password { get; set; } = default!;

    [Required(ErrorMessage = "Uloga je obavezna")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Uloga nije validna")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "Telefon je obavezan")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon mora imati tačno 10 cifara")]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? DeliveryAddress { get; set; }
}
