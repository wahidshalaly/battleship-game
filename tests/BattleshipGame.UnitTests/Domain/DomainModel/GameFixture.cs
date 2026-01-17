using System;
using System.Linq;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Domain.DomainModel;

public class GameFixture
{
    public Game CreateGameInStateNew(PlayerId? playerId = null, int boardSize = DefaultBoardSize)
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var game = new Game(playerId, boardSize);
        return game;
    }

    public Game CreateGameInStateReady(PlayerId? playerId = null, int boardSize = DefaultBoardSize)
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var game = CreateGameInStateNew(playerId, boardSize);
        PlaceShipsOnBoard(game, BoardSide.Player);
        PlaceShipsOnBoard(game, BoardSide.Opponent);
        return game;
    }

    public Game CreateGameInStateStarted(
        PlayerId? playerId = null,
        int boardSize = DefaultBoardSize
    )
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var game = CreateGameInStateReady(playerId, boardSize);
        game.StartGameplay();
        return game;
    }

    public Game CreateGameInStateGameOver(PlayerId? playerId, BoardSide winnerSide)
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var game = CreateGameInStateStarted(playerId);
        var defeatedSide = winnerSide.OppositeSide();
        var rng = new Random();

        // Collect target cells on the defeated side - all are confirmed hits
        var confirmedTargets = game.GetShips(defeatedSide)
            .SelectMany(shipId => game.GetShipPosition(defeatedSide, shipId))
            .ToList();

        // Collect target cells on the winner side - random hits and misses
        var randomTargets = game.GetNextTargets(winnerSide)
            .OrderByDescending(c => c)
            .Take(confirmedTargets.Count)
            .ToList();

        // Interleave attacks until one side loses
        for (var i = 0; i < confirmedTargets.Count; i++)
        {
            var opponentTargetCode =
                (winnerSide == BoardSide.Player) ? confirmedTargets[i] : randomTargets[i];

            var playerTargetCode =
                (winnerSide == BoardSide.Opponent) ? confirmedTargets[i] : randomTargets[i];

            game.Attack(BoardSide.Opponent, opponentTargetCode);
            if (game.State == GameState.GameOver)
                break;

            game.Attack(BoardSide.Player, playerTargetCode);
        }

        return game;
    }

    public void AttackShip(Game game, BoardSide attackedSide, ShipId shipId)
    {
        // Collect target cells on the defeated side - all are confirmed hits
        var confirmedTargets = game.GetShipPosition(attackedSide, shipId).ToList();

        // Collect target cells on the winner side - random hits and misses
        var attackerSide = attackedSide.OppositeSide();
        var randomTargets = game.GetNextTargets(attackerSide)
            .OrderByDescending(c => c)
            .Take(confirmedTargets.Count)
            .ToList();

        for (var i = 0; i < confirmedTargets.Count; i++)
        {
            var opponentTargetCode =
                (attackerSide == BoardSide.Player) ? confirmedTargets[i] : randomTargets[i];

            var playerTargetCode =
                (attackerSide == BoardSide.Opponent) ? confirmedTargets[i] : randomTargets[i];

            game.Attack(BoardSide.Opponent, opponentTargetCode);
            game.Attack(BoardSide.Player, playerTargetCode);
        }
    }

    private void PlaceShipsOnBoard(Game game, BoardSide boardSide)
    {
        game.PlaceShip(boardSide, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.PlaceShip(boardSide, ShipKind.Cruiser, ShipOrientation.Vertical, "B1");
        game.PlaceShip(boardSide, ShipKind.Destroyer, ShipOrientation.Vertical, "C1");
        game.PlaceShip(boardSide, ShipKind.Submarine, ShipOrientation.Vertical, "D1");
        game.PlaceShip(boardSide, ShipKind.Carrier, ShipOrientation.Vertical, "E1");
    }
}
