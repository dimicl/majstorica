using backend.Api.DTOs.Master;

namespace backend.Application.Interfaces;

public interface IMastersListCache
{
    Task<List<MasterListItemResponse>?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(string key, List<MasterListItemResponse> list, TimeSpan ttl, CancellationToken cancellationToken = default);
}
