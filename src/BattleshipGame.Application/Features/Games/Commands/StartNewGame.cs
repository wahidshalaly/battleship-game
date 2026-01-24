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
        var player =
            await playerRepository.GetByIdAsync(request.PlayerId, ct)
            ?? throw new PlayerNotFoundException(request.PlayerId);

        var game = new Game(request.PlayerId, request.BoardSize, request.OpponentStrategy);
        player.JoinGame(game.Id);

        await gameRepository.SaveAsync(game, ct);
        await playerRepository.SaveAsync(player, ct);

        logger.LogInformation(
            "Player joined new game. {@Payload}",
            new { PlayerId = request.PlayerId.Value, GameId = game.Id.Value }
        );

        // TODO: This is not fail-safe. Should Consider transaction or outbox pattern and Unit of Work.
        // This an issue for later, not part of the current experiment.
        // Also, I need to think if dispatch should follow the Save or precede it.
        await eventDispatcher.DispatchEventsAsync(player, ct);
        await eventDispatcher.DispatchEventsAsync(game, ct);

        return game.Id;
    }
}
