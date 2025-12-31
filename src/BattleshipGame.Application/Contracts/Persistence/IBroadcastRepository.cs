namespace BattleshipGame.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for broadcasting announcements.
/// </summary>
public interface IBroadcastRepository
{
    /// <summary>
    /// Announces a message asynchronously.
    /// </summary>
    /// <param name="announcement">The announcement message.</param>
    /// <param name="ct">The cancellation token.</param>
    Task AnnounceAsync(string announcement, CancellationToken ct);
}
