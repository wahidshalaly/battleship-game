namespace BattleshipGame.Domain.DomainModel.GameAggregate.Services;

public interface IShipPositionCalculator
{
    /// <summary>
    /// Calculates the position of a ship based on its kind, orientation, and bow code.
    /// </summary>
    /// <param name="boardSize">The board size.</param>
    /// <param name="kind">The type of ship.</param>
    /// <param name="orientation">The orientation of the ship (horizontal or vertical).</param>
    /// <param name="bowCode">The starting position of the ship on the board.</param>
    /// <returns>A list of cell codes representing the position of the ship.</returns>
    List<string> CalculatePosition(int boardSize, ShipKind kind, ShipOrientation orientation, string bowCode);

    /// <summary>
    /// Validates the position of a ship based on its kind, orientation, and bow code.
    /// </summary>
    /// <param name="boardSize">The board size.</param>
    /// <param name="kind">The type of ship.</param>
    /// <param name="orientation">The orientation of the ship (horizontal or vertical).</param>
    /// <param name="bowCode">The starting position of the ship on the board.</param>
    void ValidatePosition(int boardSize, ShipKind kind, ShipOrientation orientation, string bowCode);

    /// <summary>
    /// Validates the orientation of a ship.
    /// </summary>
    /// <param name="orientation"> The ship orientation to be validated.</param>
    void ValidateOrientation(ShipOrientation orientation);

    /// <summary>
    /// Validates the kind of ship.
    /// </summary>
    void ValidateShipKind(ShipKind kind);
}
