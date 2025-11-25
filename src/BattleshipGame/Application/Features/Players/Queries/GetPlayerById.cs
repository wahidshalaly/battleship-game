using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Query to get a player by ID.
/// </summary>
/// <param name="PlayerId">The player identifier.</param>
public record GetPlayerByIdQuery(PlayerId PlayerId) : IRequest<GetPlayerQueryResult>;

/// <summary>
/// Handler for getting a player by ID.
/// </summary>
/// <remarks>
/// Initializes a new instance of the GetPlayerQueryHandler class.
/// </remarks>
/// <param name="playerRepository">The player repository.</param>
public class GetPlayerByIdHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByIdQuery, GetPlayerQueryResult?>
{
    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> Handle(
        GetPlayerByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var player = await playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);

        if (player is null)
            return null;

        return new GetPlayerQueryResult(
            player.Id,
            player.Username,
            player.ActiveGameId?.Value,
            player.TotalGamesPlayed
        );
    }
}
