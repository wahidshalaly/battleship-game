namespace BattleshipGame.Application.Interfaces.Broadcasting;

/// <summary>
/// Repository contract for broadcasting announcements.
/// </summary>
public interface IBroadcastor
{
    /// <summary>
    /// Broadcasts an announcement asynchronously.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <param name="ct">The cancellation token.</param>
    Task BroadcastAsync(string announcement, CancellationToken ct);
}
