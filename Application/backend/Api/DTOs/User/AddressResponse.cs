namespace backend.Api.DTOs.User;

public class AddressResponse
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

