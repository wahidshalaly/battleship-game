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
        OpponentStrategy opponentStrategy,
        CancellationToken ct
    )
    {
        var guid = await mediator.Send(
            new StartNewGameCommand(playerId, boardSize, opponentStrategy),
            ct
        );
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

    public async Task<LastRoundResult> PlayerAttackThenCounterAttackAsync(
        GameId gameId,
        string cellCode,
        CancellationToken ct
    )
    {
        // Execute player's attack
        var playerAttack = await mediator.Send(new PlayerAttackCommand(gameId, cellCode), ct);

        // Check if game ended after player's attack
        if (playerAttack.GameState == GameState.GameOver)
        {
            return new LastRoundResult(
                GameId: gameId,
                PlayerTargetCell: playerAttack.TargetCell,
                PlayerAttackResult: playerAttack.CellState,
                PlayerSunkShip: playerAttack.SunkShip,
                OpponentTargetCell: null,
                OpponentAttackResult: null,
                OpponentSunkShip: null,
                GameState: playerAttack.GameState,
                WinnerSide: playerAttack.WinnerSide
            );
        }

        // Execute opponent's counter-attack
        var opponentAttack = await mediator.Send(new OpponentAttackCommand(gameId), ct);

        // Return complete round result
        return new LastRoundResult(
            GameId: gameId,
            PlayerTargetCell: playerAttack.TargetCell,
            PlayerAttackResult: playerAttack.CellState,
            PlayerSunkShip: playerAttack.SunkShip,
            OpponentTargetCell: opponentAttack.TargetCell,
            OpponentAttackResult: opponentAttack.CellState,
            OpponentSunkShip: opponentAttack.SunkShip,
            GameState: opponentAttack.GameState,
            WinnerSide: opponentAttack.WinnerSide
        );
    }

    public Task EndGameAsync(GameId gameId, CancellationToken ct)
    {
        return mediator.Send(new EndGameCommand(gameId), ct);
    }
}
