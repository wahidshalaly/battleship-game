using BattleshipGame.Application.Interfaces.Broadcasting;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Infrastructure.Broadcasting;

/// <summary>
/// A placeholder for something else interesting to come later.
/// </summary>
public class Broadcaster(ILogger<Broadcaster> logger) : IBroadcastor
{
    /// <inheritdoc />
    public Task BroadcastAsync(string announcement, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Announcement: {Announcement}", announcement);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to announce: {Announcement}", announcement);
        }
        return Task.CompletedTask;
    }
}
