using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries;

/// <summary>
/// Query to get a game by ID.
/// </summary>
/// <param name="GameId">The game identifier.</param>
public record GetGameQuery(GameId GameId) : IRequest<GameModel>;

/// <summary>
/// Result of getting a game.
/// </summary>
/// <param name="GameId">The game's identifier.</param>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="BoardSize">The board size.</param>
/// <param name="State">The game state.</param>
public record GameModel(Guid GameId, Guid PlayerId, int BoardSize, GameState State);

/// <summary>
/// Handler for getting a game by ID.
/// </summary>
/// <param name="gameRepository"> The game repository to access game data.</param>
internal class GetGameQueryHandler(IGameRepository gameRepository) : IRequestHandler<GetGameQuery, GameModel?>
{
    /// <inheritdoc />
    public async Task<GameModel?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken);

        return game is null ? null : new GameModel(game.Id, game.PlayerId.Value, game.BoardSize, game.State);
    }
}
