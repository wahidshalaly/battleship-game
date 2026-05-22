namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class BoardJson
{
    public Guid Id { get; set; }
    public List<ShipJson> Ships { get; set; } = [];
    public List<CellJson> Cells { get; set; } = [];
}
