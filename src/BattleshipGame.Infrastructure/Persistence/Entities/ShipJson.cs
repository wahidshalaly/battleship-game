namespace BattleshipGame.Infrastructure.Persistence.Entities;

internal class ShipJson
{
    public Guid Id { get; set; }
    public int Kind { get; set; }
    public List<string> Codes { get; set; } = [];
    public List<string> Hits { get; set; } = [];
}
