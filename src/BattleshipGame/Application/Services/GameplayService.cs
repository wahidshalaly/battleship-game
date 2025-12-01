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
        logger.LogInformation(
            "Starting new game for player {PlayerId} size {BoardSize}",
            playerId,
            boardSize
        );
        var guid = await mediator.Send(
            new Features.Games.Commands.StartNewGameCommand(playerId, boardSize),
            cancellationToken
        );
        logger.LogInformation("Game {GameId} created for player {PlayerId}", guid, playerId);
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
        var shipGuid = await mediator.Send(
            new Features.Games.Commands.AddShipCommand(gameId, side, kind, orientation, bowCode),
            cancellationToken
        );
        logger.LogInformation(
            "Ship {ShipId} placed on {Side} for game {GameId}",
            shipGuid,
            side,
            gameId
        );
        return new ShipId(shipGuid);
    }

    public async Task StartGameplayAsync(GameId gameId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting gameplay for game {GameId}", gameId);
        await mediator.Send(
            new Features.Games.Commands.StartGameplayCommand(gameId),
            cancellationToken
        );
        logger.LogInformation("Gameplay confirmed for game {GameId}", gameId);
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
        var status = await mediator.Send(
            new Features.Games.Commands.CheckGameStatusCommand(gameId),
            cancellationToken
        );
        var result = new AttackResult(
            cellCode,
            cellState,
            targetSide,
            status?.IsGameOver ?? false,
            status?.Winner ?? BoardSide.None
        );
        if (
            attacker == BoardSide.Player
            && targetSide == BoardSide.Opponent
            && (status is null || !status.IsGameOver)
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
        return result;
    }

    public async Task<AttackResult> OpponentAttackAsync(
        GameId gameId,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Opponent attack for game {GameId}", gameId);
        var attackDto = await mediator.Send(
            new Features.Games.Commands.OpponentAttackCommand(gameId),
            cancellationToken
        );
        return new AttackResult(
            attackDto.CellCode,
            attackDto.CellState,
            BoardSide.Opponent,
            attackDto.IsGameOver,
            attackDto.Winner
        );
    }

    public async Task<GameResult?> CheckGameStatusAsync(
        GameId gameId,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Checking status for game {GameId}", gameId);
        var status = await mediator.Send(
            new Features.Games.Commands.CheckGameStatusCommand(gameId),
            cancellationToken
        );
        return status;
    }

    public async Task EndGameAsync(GameId gameId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ending game {GameId}", gameId);
        await mediator.Send(new Features.Games.Commands.EndGameCommand(gameId), cancellationToken);
        logger.LogInformation("Game {GameId} ended.", gameId);
    }
}

// DTO records moved to dedicated files for clarity.
