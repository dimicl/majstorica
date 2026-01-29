namespace backend.Api.DTOs.Jobs;

public class SendRequestsRequest
{
    public List<Guid> MasterIds { get; set; } = new();
}
