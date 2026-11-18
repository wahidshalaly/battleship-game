namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// The outcomes of a player's finished games. Unfinished games count as neither won nor lost.
/// </summary>
/// <param name="GamesWon">The number of finished games the player won.</param>
/// <param name="GamesLost">The number of finished games the player lost.</param>
public record GameResults(int GamesWon, int GamesLost)
{
    /// <summary>
    /// The results of a player who has not finished any game.
    /// </summary>
    public static GameResults None { get; } = new(0, 0);

    /// <summary>
    /// Gets the number of finished games.
    /// </summary>
    public int GamesFinished => GamesWon + GamesLost;

    /// <summary>
    /// Gets the share of finished games won, from 0 to 1; 0 when no game is finished.
    /// </summary>
    public double WinRate => GamesFinished == 0 ? 0 : (double)GamesWon / GamesFinished;
}
