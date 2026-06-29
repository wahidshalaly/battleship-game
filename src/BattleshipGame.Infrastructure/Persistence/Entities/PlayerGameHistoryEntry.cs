namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class PlayerGameHistoryEntry
{
    public Guid PlayerId { get; set; }
    public Guid GameId { get; set; }

    public PlayerEntity Player { get; set; } = null!;
}
