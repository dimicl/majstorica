namespace backend.Application.Interfaces;

/// <summary>
/// Serverski zapis autentikacije u Redisu (TTL), uz JWT — za buduću invalidaciju ili monitoring.
/// </summary>
public interface IAuthServerSessionStore
{
    Task TouchServerSessionAsync(Guid userId, CancellationToken cancellationToken = default);
}
