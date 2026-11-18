using System.ComponentModel.DataAnnotations;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Configuration options for the leaderboard ranking.
/// </summary>
public sealed class LeaderboardOptions
{
    public const string SectionName = "Leaderboard";

    /// <summary>
    /// Number of finished games a player needs before they are ranked by win rate. Players with
    /// fewer are provisional and listed after the ranked ones, so a single lucky win doesn't top
    /// the board.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MinimumFinishedGamesToRank { get; set; } = 5;
}
