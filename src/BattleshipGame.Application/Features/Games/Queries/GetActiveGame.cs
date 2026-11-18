using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries;

/// <summary>
/// Query to get a player's active game: the one that is not over, if any.
/// </summary>
/// <param name="PlayerId">The player identifier.</param>
public record GetActiveGameQuery(PlayerId PlayerId) : IRequest<GetGameQueryResult?>;

/// <summary>
/// Handler for getting a player's active game.
/// </summary>
/// <param name="gameRepository">The game repository.</param>
internal class GetActiveGameHandler(IGameRepository gameRepository)
    : IRequestHandler<GetActiveGameQuery, GetGameQueryResult?>
{
    /// <inheritdoc />
    public async Task<GetGameQueryResult?> Handle(GetActiveGameQuery request, CancellationToken ct)
    {
        var game = await gameRepository.GetActiveGameByPlayerIdAsync(request.PlayerId, ct);
        return game is null ? null : GetGameQueryResult.From(game);
    }
}
