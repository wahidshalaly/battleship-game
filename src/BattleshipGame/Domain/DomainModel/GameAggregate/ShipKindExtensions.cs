using BattleshipGame.Domain.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

public static class ShipKindExtensions
{
    private static readonly Dictionary<ShipKind, int> _sizes = new()
    {
        { ShipKind.Destroyer, 2 },
        { ShipKind.Cruiser, 3 },
        { ShipKind.Submarine, 3 },
        { ShipKind.Battleship, 4 },
        { ShipKind.Carrier, 5 },
    };

    public static int ToSize(this ShipKind kind)
    {
        return _sizes[kind];
    }
}
