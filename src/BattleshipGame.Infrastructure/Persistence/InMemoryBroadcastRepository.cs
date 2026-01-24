using System.Collections.Concurrent;
using BattleshipGame.Application.Interfaces.Persistence;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the Player repository.
/// </summary>
public class InMemoryBroadcastRepository(ILogger<InMemoryBroadcastRepository> logger)
    : IBroadcastRepository
{
    private readonly ConcurrentDictionary<Guid, string> _announcements = new();

    /// <inheritdoc />
    public Task AnnounceAsync(string announcement, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Announcing: {Announcement}", announcement);
            _announcements.TryAdd(Guid.NewGuid(), announcement);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to announce: {Announcement}", announcement);
        }
        return Task.CompletedTask;
    }
}
