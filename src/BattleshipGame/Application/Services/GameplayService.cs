using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public sealed class GameplayService(IMediator mediator, ILogger<GameplayService> logger)
    : IGameplayService
{
    public async Task<GameId> StartNewGameAsync(
        PlayerId playerId,
        int boardSize,
        CancellationToken cancellationToken
    )
    {
        var guid = await mediator.Send(
            new Features.Games.Commands.StartNewGameCommand(playerId, boardSize),
            cancellationToken
        );
        return new GameId(guid);
    }

    public async Task<ShipId> PlaceShipAsync(
        GameId gameId,
        BoardSide side,
        ShipKind kind,
        ShipOrientation orientation,
        string bowCode,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Placing ship {Kind} {Orientation} at {Bow} on {Side} for game {GameId}",
            kind,
            orientation,
            bowCode,
            side,
            gameId
        );
        var guid = await mediator.Send(
            new Features.Games.Commands.AddShipCommand(gameId, side, kind, orientation, bowCode),
            cancellationToken
        );
        logger.LogInformation(
            "Ship {ShipId} placed on {Side} for game {GameId}",
            guid,
            side,
            gameId
        );
        return new ShipId(guid);
    }

    public async Task StartGameplayAsync(GameId gameId, CancellationToken cancellationToken)
    {
        await mediator.Send(
            new Features.Games.Commands.StartGameplayCommand(gameId),
            cancellationToken
        );
    }

    public async Task<AttackResult> AttackAsync(
        GameId gameId,
        BoardSide targetSide,
        string cellCode,
        CancellationToken cancellationToken
    )
    {
        var attacker = targetSide.OppositeSide();
        logger.LogInformation(
            "{Attacker} attacking {Cell} in game {GameId}",
            attacker,
            cellCode,
            gameId
        );
        var cellState = await mediator.Send(
            new Features.Games.Commands.AttackCellCommand(gameId, targetSide, cellCode),
            cancellationToken
        );
        // Build preliminary result; a follow-up opponent move may alter state but we re-query if needed via status check
        var gameResult = await mediator.Send(
            new Features.Games.Commands.CheckGameStatusCommand(gameId),
            cancellationToken
        );
        var attackResult = new AttackResult(
            cellCode,
            cellState,
            targetSide,
            gameResult?.IsGameOver ?? false,
            gameResult?.Winner ?? BoardSide.None
        );
        if (
            attacker == BoardSide.Player
            && targetSide == BoardSide.Opponent
            && (gameResult is null || !gameResult.IsGameOver)
        )
        {
            logger.LogInformation("Opponent counter-attack initiating for game {GameId}", gameId);
            var opponent = await mediator.Send(
                new Features.Games.Commands.OpponentAttackCommand(gameId),
                cancellationToken
            );
            logger.LogInformation(
                "Opponent attacked {Cell} => {State} (gameOver={Over})",
                opponent.CellCode,
                opponent.CellState,
                opponent.IsGameOver
            );
        }
        return attackResult;
    }

    public async Task<AttackResult> OpponentAttackAsync(
        GameId gameId,
        CancellationToken cancellationToken
    )
    {
        var attackResult = await mediator.Send(
            new Features.Games.Commands.OpponentAttackCommand(gameId),
            cancellationToken
        );
        return new AttackResult(
            attackResult.CellCode,
            attackResult.CellState,
            BoardSide.Opponent,
            attackResult.IsGameOver,
            attackResult.Winner
        );
    }

    public async Task<GameResult?> CheckGameStatusAsync(
        GameId gameId,
        CancellationToken cancellationToken
    )
    {
        var gameResult = await mediator.Send(
            new Features.Games.Commands.CheckGameStatusCommand(gameId),
            cancellationToken
        );
        return gameResult;
    }

    public async Task EndGameAsync(GameId gameId, CancellationToken cancellationToken)
    {
        await mediator.Send(new Features.Games.Commands.EndGameCommand(gameId), cancellationToken);
    }
}

