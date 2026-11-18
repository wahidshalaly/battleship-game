using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// This command starts a new battleship game.
/// </summary>
/// <param name="PlayerId">The player creating the game.</param>
/// <param name="BoardSize">The size of the game board (optional, defaults to 10).</param>
/// <param name="OpponentStrategy">Computer opponent strategy (optional, defaults to Random).</param>
public record StartNewGameCommand(
    PlayerId PlayerId,
    int BoardSize = DefaultBoardSize,
    OpponentStrategy OpponentStrategy = OpponentStrategy.Random
) : IRequest<Guid>;

internal class StartNewGameHandler(
    ILogger<StartNewGameHandler> logger,
    IGameRepository gameRepository,
    IPlayerRepository playerRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<StartNewGameCommand, Guid>
{
    public async Task<Guid> Handle(StartNewGameCommand request, CancellationToken ct)
    {
        _ =
            await playerRepository.GetByIdAsync(request.PlayerId, ct)
            ?? throw new PlayerNotFoundException(request.PlayerId);

        if (
            await gameRepository.GetActiveGameByPlayerIdAsync(request.PlayerId, ct) is
            { } activeGame
        )
            throw new PlayerHasActiveGameException(request.PlayerId, activeGame.Id);

        var game = new Game(request.PlayerId, request.BoardSize, request.OpponentStrategy);
        await gameRepository.SaveAsync(game, ct);

        logger.LogInformation(
            "Player started a new game. {@Payload}",
            new { PlayerId = request.PlayerId.Value, GameId = game.Id.Value }
        );

        await eventDispatcher.DispatchEventsAsync(game, ct);

        return game.Id;
    }
}
