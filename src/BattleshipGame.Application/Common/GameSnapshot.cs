using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Common;

/// <summary>
/// Read-only projection of the game state for AI opponent decision-making.
/// </summary>
public sealed record GameSnapshot
{
    // TODO: Review the types of AvailableTargets, Hits, Misses.
    // Should they be IReadOnlyList<string>, HashSet<string>, List<string> or string[]?

    /// <summary>
    /// The size of the board (e.g., 10 for a 10×10 board).
    /// </summary>
    public required int BoardSize { get; init; }

    /// <summary>
    /// The current state of the game.
    /// </summary>
    public required GameState GameState { get; init; }

    /// <summary>
    /// The range of the board in cell codes (e.g., "A1-J10").
    /// </summary>
    public required string BoardDescription { get; init; }

    /// <summary>
    /// Cells that are valid targets for the next attack (not yet attacked).
    /// </summary>
    public required string[] AvailableTargets { get; init; }

    /// <summary>
    /// Cells that were attacked and hit a ship.
    /// </summary>
    public required string[] Hits { get; init; }

    /// <summary>
    /// Cells that were attacked but missed.
    /// </summary>
    public required string[] Misses { get; init; }
}
