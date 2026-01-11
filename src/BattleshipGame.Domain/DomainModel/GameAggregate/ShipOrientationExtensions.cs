using BattleshipGame.Domain.Common;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

public static class ShipOrientationExtensions
{
    public static void Validate(this ShipOrientation orientation)
    {
        if (orientation == ShipOrientation.None)
        {
            throw new ArgumentException(ErrorMessages.InvalidShipOrientation);
        }
    }
}
