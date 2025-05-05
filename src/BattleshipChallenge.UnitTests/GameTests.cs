using Xunit;

namespace BattleshipChallenge.UnitTests;

public class GameTests
{
    [Fact]
    public void Attack_WhenNotOccupied_ReturnFalse()
    {
        var game = new Game();
        game.CreateBoard(Player.One);
        var result = game.Attack(Player.One, "C5");

        Assert.False(result);
    }

    [Fact]
    public void Attack_WhenOccupied_ReturnTrue()
    {
        var game = new Game();
        game.CreateBoard(Player.One);
        game.AddShip(Player.One, "C3", "C5");
        var result = game.Attack(Player.One, "C4");

        Assert.True(result);
    }

    [Fact]
    public void PlayerHasLost_WhenShipsSunk_ReturnTrue()
    {
        var game = new Game();
        game.CreateBoard(Player.One);
        game.AddShip(Player.One, "C3", "C5");
        _ = game.Attack(Player.One, "C3");
        _ = game.Attack(Player.One, "C4");
        _ = game.Attack(Player.One, "C5");

        var result = game.PlayerHasLost(Player.One);

        Assert.True(result);
    }
}