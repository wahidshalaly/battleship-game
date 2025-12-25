using BattleshipGame.Application.Features.Games.Commands;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public sealed class GameplayService(IMediator mediator) : IGameplayService
{
    public async Task<GameId> StartNewGameAsync(
        PlayerId playerId,
        int boardSize,
        CancellationToken ct
    )
    {
        var guid = await mediator.Send(new StartNewGameCommand(playerId, boardSize), ct);
        return new GameId(guid);
    }

    public async Task<ShipId> PlaceShipAsync(
        GameId gameId,
        BoardSide side,
        ShipKind kind,
        ShipOrientation orientation,
        string bowCode,
        CancellationToken ct
    )
    {
        var guid = await mediator.Send(
            new PlaceShipCommand(gameId, side, kind, orientation, bowCode),
            ct
        );

        return new ShipId(guid);
    }

    public Task StartGameplayAsync(GameId gameId, CancellationToken ct) =>
        mediator.Send(new StartGameplayCommand(gameId), ct);

    public async Task<AttackResult> AttackAsync(
        GameId gameId,
        BoardSide targetSide,
        string cellCode,
        CancellationToken ct
    )
    {
        var attacker = targetSide.OppositeSide();
        var cellState = await mediator.Send(new AttackCommand(gameId, targetSide, cellCode), ct);
        // Build preliminary result; a follow-up opponent move may alter state but we re-query if needed via status check
        var gameResult = await mediator.Send(new CheckGameStatusCommand(gameId), ct);
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
            var opponent = await mediator.Send(new OpponentAttackCommand(gameId), ct);
        }
        return attackResult;
    }

    public Task<GameResult?> CheckGameStatusAsync(GameId gameId, CancellationToken ct)
    {
        return mediator.Send(new CheckGameStatusCommand(gameId), ct);
    }

    public Task EndGameAsync(GameId gameId, CancellationToken ct)
    {
        return mediator.Send(new EndGameCommand(gameId), ct);
    }
}
