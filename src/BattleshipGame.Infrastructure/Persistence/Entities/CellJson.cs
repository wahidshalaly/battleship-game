namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class CellJson
{
    public string Code { get; set; } = string.Empty;
    public Guid? ShipId { get; set; }
    public int State { get; set; }
}
