using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Korisničko ime ili email je obavezan")]
    public string UsernameOrEmail { get; set; } = default!;

    [Required(ErrorMessage = "Lozinka je obavezna")]
    public string Password { get; set; } = default!;
}
