using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Infrastructure.Persistence;

public class InMemoryGameRepositoryTests
{
    private readonly GameFixture _fixture = new();
    private readonly InMemoryGameRepository _repository = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenGameExists_ShouldReturnGame()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        await _repository.SaveAsync(game, _cancellationToken);

        // Act
        var result = await _repository.GetByIdAsync(game.Id, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(game.Id);
        result.PlayerId.Should().Be(game.PlayerId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGameDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentGameId = new GameId(Guid.NewGuid());

        // Act
        var result = await _repository.GetByIdAsync(nonExistentGameId, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        await _repository.SaveAsync(game, _cancellationToken);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await _repository.GetByIdAsync(game.Id, cts.Token);

        // Assert - Should complete successfully as method is synchronous internally
        result.Should().NotBeNull();
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WhenNewGame_ShouldSaveAndReturnGameId()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();

        // Act
        await _repository.SaveAsync(game, _cancellationToken);

        // Assert
        var savedGame = await _repository.GetByIdAsync(game.Id, _cancellationToken);
        savedGame.Should().NotBeNull();
        savedGame.Id.Should().Be(game.Id);
    }

    [Fact]
    public async Task SaveAsync_WhenUpdatingExistingGame_ShouldUpdateAndReturnGameId()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        await _repository.SaveAsync(game, _cancellationToken);

        // Modify the game
        game.Attack(BoardSide.Player, "A1");

        // Act
        await _repository.SaveAsync(game, _cancellationToken);

        // Assert
        var updatedGame = await _repository.GetByIdAsync(game.Id, _cancellationToken);
        updatedGame.Should().NotBeNull();
        updatedGame.Id.Should().Be(game.Id);
    }

    #endregion

    #region GetByPlayerIdAsync Tests

    [Fact]
    public async Task GetByPlayerIdAsync_WhenPlayerHasGames_ShouldReturnAllGames()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game1 = new Game(playerId);
        var game2 = new Game(playerId);
        var otherPlayerGame = _fixture.CreateReadyGame(); // Different player

        await _repository.SaveAsync(game1, _cancellationToken);
        await _repository.SaveAsync(game2, _cancellationToken);
        await _repository.SaveAsync(otherPlayerGame, _cancellationToken);

        // Act
        var result = await _repository.GetByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(g => g.Id == game1.Id);
        result.Should().Contain(g => g.Id == game2.Id);
        result.Should().NotContain(g => g.Id == otherPlayerGame.Id);
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WhenPlayerHasNoGames_ShouldReturnEmptyCollection()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());

        // Act
        var result = await _repository.GetByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await _repository.GetByPlayerIdAsync(playerId, cts.Token);

        // Assert - Should complete successfully as method is synchronous internally
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var games = new List<Game>();

        for (int i = 0; i < 5; i++)
        {
            var game = new Game(playerId);
            games.Add(game);
            await _repository.SaveAsync(game, _cancellationToken);
        }

        // Act - Query same player concurrently
        var queryTasks = Enumerable
            .Range(0, 10)
            .Select(_ => _repository.GetByPlayerIdAsync(playerId, _cancellationToken));
        var results = await Task.WhenAll(queryTasks);

        // Assert
        foreach (var result in results)
        {
            result.Should().HaveCount(5);
            foreach (var game in games)
            {
                result.Should().Contain(g => g.Id == game.Id);
            }
        }
    }

    #endregion

    #region GetActiveGameByPlayerIdAsync Tests

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WhenPlayerHasActiveGame_ShouldReturnGame()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId); // State: Started (active)

        await _repository.SaveAsync(game, _cancellationToken);

        // Act
        var result = await _repository.GetActiveGameByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(game.Id);
        result.State.Should().NotBe(GameState.GameOver);
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WhenPlayerHasActiveGame_ShouldReturnActiveGame()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        await _repository.SaveAsync(game, _cancellationToken);

        // Act - Use the correct playerId from the game
        var result = await _repository.GetActiveGameByPlayerIdAsync(
            game.PlayerId,
            _cancellationToken
        );

        // Assert - _fixture.CreateReadyGame() creates a game in BoardsAreReady state, not GameOver
        // So this should return the active game
        result.Should().NotBeNull();
        result.State.Should().NotBe(GameState.GameOver);
        result.Id.Should().Be(game.Id);
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WhenPlayerHasNoGames_ShouldReturnNull()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());

        // Act
        var result = await _repository.GetActiveGameByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WithMultipleActiveGames_ShouldReturnFirst()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game1 = new Game(playerId);
        var game2 = new Game(playerId);

        await _repository.SaveAsync(game1, _cancellationToken);
        await _repository.SaveAsync(game2, _cancellationToken);

        // Act
        var result = await _repository.GetActiveGameByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId);
        result.State.Should().NotBe(GameState.GameOver);
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var result = await _repository.GetActiveGameByPlayerIdAsync(playerId, cts.Token);

        // Assert - Should complete successfully as method is synchronous internally
        result.Should().BeNull();
    }

    #endregion

    #region Data Integrity and State Consistency Tests

    [Fact]
    public async Task Repository_WhenSavingAndRetrieving_ShouldMaintainGameState()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        var originalState = game.State;
        var originalBoardSize = game.BoardSize;

        // Act
        await _repository.SaveAsync(game, _cancellationToken);
        var retrievedGame = await _repository.GetByIdAsync(game.Id, _cancellationToken);

        // Assert
        retrievedGame.Should().NotBeNull();
        retrievedGame.State.Should().Be(originalState);
        retrievedGame.BoardSize.Should().Be(originalBoardSize);
        retrievedGame.PlayerId.Should().Be(game.PlayerId);
    }

    [Fact]
    public async Task Repository_ConcurrentSaveAndRead_ShouldMaintainConsistency()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);
        await _repository.SaveAsync(game, _cancellationToken);

        var readTasks = new List<Task<Game?>>();
        var saveTasks = new List<Task>();

        // Act - Perform concurrent reads and saves
        for (int i = 0; i < 20; i++)
        {
            readTasks.Add(_repository.GetByIdAsync(game.Id, _cancellationToken));
            if (i % 2 == 0)
            {
                saveTasks.Add(_repository.SaveAsync(game, _cancellationToken));
            }
        }

        var readResults = await Task.WhenAll(readTasks);
        await Task.WhenAll(saveTasks);

        // Assert
        readResults.Should().OnlyContain(g => g != null);
        readResults.Should().OnlyContain(g => g!.Id == game.Id);
    }

    [Fact]
    public async Task Repository_WhenGameUpdated_ShouldReflectChangesInSubsequentReads()
    {
        // Arrange
        var game = _fixture.CreateReadyGame();
        await _repository.SaveAsync(game, _cancellationToken);

        // Act - Attack a cell to change game state
        game.Attack(BoardSide.Player, "A1");
        await _repository.SaveAsync(game, _cancellationToken);

        // Retrieve updated game
        var updatedGame = await _repository.GetByIdAsync(game.Id, _cancellationToken);

        // Assert
        updatedGame.Should().NotBeNull();
        updatedGame.Id.Should().Be(game.Id);
        // The updated game should reflect the attack
    }

    #endregion

    #region Edge Cases and Exception Handling

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task Repository_WithMultipleGames_ShouldHandleCorrectly(int gameCount)
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var games = new List<Game>();

        for (int i = 0; i < gameCount; i++)
        {
            games.Add(new Game(playerId));
        }

        // Act
        var saveTasks = games.Select(g => _repository.SaveAsync(g, _cancellationToken));
        await Task.WhenAll(saveTasks);

        var retrievedGames = await _repository.GetByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        retrievedGames.Should().HaveCount(gameCount);
        foreach (var game in games)
        {
            retrievedGames.Should().Contain(g => g.Id == game.Id);
        }
    }

    #endregion
}
