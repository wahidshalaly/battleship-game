using System;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Domain.DomainModel;

public class GameFixture
{
    public Game CreateNewGame(PlayerId? playerId = null, int? boardSize = null)
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var game = new Game(playerId, boardSize ?? DefaultBoardSize);
        return game;
    }

    public Game CreateReadyGame(PlayerId? playerId = null)
    {
        playerId ??= new PlayerId(Guid.NewGuid());

        var game = CreateNewGame(playerId);
        AddShipsOnBoard(game, BoardSide.Own);
        AddShipsOnBoard(game, BoardSide.Opp);
        return game;
    }

    public Game CreateCompletedGame(PlayerId? playerId, BoardSide winnerSide)
    {
        playerId ??= new PlayerId(Guid.NewGuid());
        var attackedSide = winnerSide.OppositeSide();
        var game = CreateReadyGame(playerId);

        var ships = game.GetShips(attackedSide);
        foreach (var shipId in ships)
        {
            AttackShip(game, attackedSide, shipId);
        }

        return game;
    }

    public void AttackShip(Game game, BoardSide attackedSide, ShipId shipId)
    {
        var position = game.GetShipPosition(attackedSide, shipId);
        foreach (var cellCode in position)
        {
            game.Attack(attackedSide, cellCode);
        }
    }

    private void AddShipsOnBoard(Game game, BoardSide boardSide)
    {
        game.AddShip(boardSide, ShipKind.Battleship, ShipOrientation.Vertical, "A1");
        game.AddShip(boardSide, ShipKind.Cruiser, ShipOrientation.Vertical, "B1");
        game.AddShip(boardSide, ShipKind.Destroyer, ShipOrientation.Vertical, "C1");
        game.AddShip(boardSide, ShipKind.Submarine, ShipOrientation.Vertical, "D1");
        game.AddShip(boardSide, ShipKind.Carrier, ShipOrientation.Vertical, "E1");
    }
}