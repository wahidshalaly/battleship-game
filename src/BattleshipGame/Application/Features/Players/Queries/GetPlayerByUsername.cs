using BattleshipGame.Application.Contracts.Persistence;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Query to get a player by username.
/// </summary>
/// <param name="Username">The player's username.</param>
public record GetPlayerByUsernameQuery(string Username) : IRequest<GetPlayerQueryResult>;

/// <summary>
/// Handler for getting a player by username.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GetPlayerByUsernameQueryHandler class.
/// </remarks>
/// <param name="playerRepository">The player repository.</param>
public class GetPlayerByUsernameHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByUsernameQuery, GetPlayerQueryResult?>
{
    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> Handle(
        GetPlayerByUsernameQuery request,
        CancellationToken ct
    )
    {
        var player = await playerRepository.GetByUsernameAsync(request.Username, ct);

        return player is null
            ? null
            : new GetPlayerQueryResult(
                player.Id,
                player.Username,
                player.ActiveGameId?.Value,
                player.TotalGamesPlayed
            );
    }
}
