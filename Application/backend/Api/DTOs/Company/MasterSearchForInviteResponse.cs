namespace backend.Api.DTOs.Company;

public class MasterSearchForInviteResponse
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Headline { get; set; }
}
