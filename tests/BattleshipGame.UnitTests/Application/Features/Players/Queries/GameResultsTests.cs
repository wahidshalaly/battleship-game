using BattleshipGame.Application.Features.Players.Queries;

namespace BattleshipGame.UnitTests.Application.Features.Players.Queries;

public class GameResultsTests
{
    [Fact]
    public void GamesFinished_ShouldBeWinsPlusLosses()
    {
        // Arrange
        var results = new GameResults(GamesWon: 3, GamesLost: 2);

        // Act
        var gamesFinished = results.GamesFinished;

        // Assert
        gamesFinished.Should().Be(5);
    }

    [Theory]
    [InlineData(1, 0, 1.0)]
    [InlineData(0, 1, 0.0)]
    [InlineData(3, 1, 0.75)]
    [InlineData(1, 2, 1.0 / 3)]
    public void WinRate_ShouldBeTheShareOfFinishedGamesWon(int won, int lost, double expected)
    {
        // Arrange
        var results = new GameResults(won, lost);

        // Act
        var winRate = results.WinRate;

        // Assert
        winRate.Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void WinRate_WhenNoGamesFinished_ShouldBeZero()
    {
        // Arrange
        var results = GameResults.None;

        // Act
        var winRate = results.WinRate;

        // Assert
        winRate.Should().Be(0);
    }
}
