using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs.Company;

public class InviteMasterRequest
{
    [Required]
    public Guid MasterUserId { get; set; }
}
