using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// Generates random but valid ship placements for testing purposes.
/// Ships are placed without overlapping and within the board boundaries.
/// </summary>
/// <param name="boardSize">The size of the game board (e.g., 10 for a 10x10 grid).</param>
/// <param name="seed">Optional seed for reproducible random placements. Useful for debugging test failures.</param>
public sealed class RandomShipPlacementGenerator(int boardSize, int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : new Random();
    private readonly HashSet<string> _occupiedCells = [];

    /// <summary>
    /// Generates random valid placements for all standard ship types.
    /// </summary>
    /// <returns>Array of ship definitions with bow code, kind, orientation, and position.</returns>
    public (
        string BowCode,
        ShipKind Kind,
        ShipOrientation Orientation,
        string[] Position
    )[] GeneratePlacements()
    {
        _occupiedCells.Clear();

        // Place ships in order from largest to smallest for better placement success
        var shipKinds = new[]
        {
            ShipKind.Carrier,
            ShipKind.Battleship,
            ShipKind.Cruiser,
            ShipKind.Submarine,
            ShipKind.Destroyer,
        };

        var placements =
            new List<(
                string BowCode,
                ShipKind Kind,
                ShipOrientation Orientation,
                string[] Position
            )>();

        foreach (var kind in shipKinds)
        {
            var placement = GeneratePlacementForShip(kind);
            placements.Add(placement);
        }

        return [.. placements];
    }

    private (
        string BowCode,
        ShipKind Kind,
        ShipOrientation Orientation,
        string[] Position
    ) GeneratePlacementForShip(ShipKind kind)
    {
        var shipSize = kind.ToSize();
        const int maxAttempts = 100;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var orientation =
                _random.Next(2) == 0 ? ShipOrientation.Horizontal : ShipOrientation.Vertical;
            var (bowCode, positions) = TryGeneratePosition(shipSize, orientation);

            if (positions.Length > 0 && !positions.Any(_occupiedCells.Contains))
            {
                foreach (var pos in positions)
                {
                    _occupiedCells.Add(pos);
                }

                return (bowCode, kind, orientation, positions);
            }
        }

        throw new InvalidOperationException(
            $"Failed to place ship {kind} after {maxAttempts} attempts."
        );
    }

    private (string BowCode, string[] Positions) TryGeneratePosition(
        int shipSize,
        ShipOrientation orientation
    )
    {
        // Calculate valid ranges based on orientation
        int maxColumn,
            maxRow;

        if (orientation == ShipOrientation.Horizontal)
        {
            maxColumn = boardSize - shipSize + 1;
            maxRow = boardSize;
        }
        else
        {
            maxColumn = boardSize;
            maxRow = boardSize - shipSize + 1;
        }

        if (maxColumn <= 0 || maxRow <= 0)
        {
            return (string.Empty, []);
        }

        var column = _random.Next(maxColumn);
        var row = _random.Next(maxRow) + 1; // Rows are 1-indexed

        var bowCode = $"{(char)('A' + column)}{row}";
        var positions = CalculatePositions(column, row, shipSize, orientation);

        return (bowCode, positions);
    }

    private static string[] CalculatePositions(
        int startColumn,
        int startRow,
        int shipSize,
        ShipOrientation orientation
    )
    {
        var positions = new string[shipSize];

        for (var i = 0; i < shipSize; i++)
        {
            if (orientation == ShipOrientation.Horizontal)
            {
                positions[i] = $"{(char)('A' + startColumn + i)}{startRow}";
            }
            else
            {
                positions[i] = $"{(char)('A' + startColumn)}{startRow + i}";
            }
        }

        return positions;
    }
}
