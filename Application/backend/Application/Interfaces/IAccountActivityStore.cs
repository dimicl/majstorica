namespace backend.Application.Interfaces;

/// <summary>
/// Kratak zapis aktivnosti naloga u Redisu (lista poslednjih događaja po korisniku).
/// </summary>
public interface IAccountActivityStore
{
    Task RecordAsync(
        Guid userId,
        string eventType,
        string? detail = null,
        CancellationToken cancellationToken = default);
}
