namespace backend.Api.DTOs.Company;

public class CompanyInvitationPendingResponse
{
    public Guid InvitationId { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
