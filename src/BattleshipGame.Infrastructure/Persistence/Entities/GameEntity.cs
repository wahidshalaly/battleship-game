namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class GameEntity
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public int BoardSize { get; set; }
    public int OpponentStrategy { get; set; }
    public int State { get; set; }
    public int TargetSide { get; set; }
    public int WinnerSide { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public BoardJson OwnBoard { get; set; } = null!;
    public BoardJson OppBoard { get; set; } = null!;
}
