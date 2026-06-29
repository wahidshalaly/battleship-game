using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.IntegrationTests.Infrastructure.Persistence.Repositories;

public class GameRepositoryTests(PostgresFixture postgres)
    : BaseRepositoryTests(postgres),
        IClassFixture<PostgresFixture>
{
    private readonly GameFixture _fixture = new();
    private IGameRepository _subject = null!;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        _subject = new GameRepository(_dbcontext);
        return Task.CompletedTask;
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenGameExists_ShouldReturnGame()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetByIdAsync(game.Id, _cancellationToken);

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
        var result = await _subject.GetByIdAsync(nonExistentGameId, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.GetByIdAsync(game.Id, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WhenNewGame_ShouldSaveAndReturnGameId()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();

        // Act
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Assert
        var savedGame = await _subject.GetByIdAsync(game.Id, _cancellationToken);
        savedGame.Should().NotBeNull();
        savedGame.Id.Should().Be(game.Id);
    }

    [Fact]
    public async Task SaveAsync_WhenUpdatingExistingGame_ShouldUpdateAndReturnGameId()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Modify the game
        game.Attack(BoardSide.Opponent, "A1");

        // Act
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Assert
        var updatedGame = await _subject.GetByIdAsync(game.Id, _cancellationToken);
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
        var otherPlayerGame = _fixture.CreateGameInStateReady(); // Different player

        await _subject.SaveAsync(game1, _cancellationToken);
        await _subject.SaveAsync(game2, _cancellationToken);
        await _subject.SaveAsync(otherPlayerGame, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetByPlayerIdAsync(playerId, _cancellationToken);

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
        var result = await _subject.GetByPlayerIdAsync(playerId, _cancellationToken);

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

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.GetByPlayerIdAsync(playerId, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByPlayerIdAsync_ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange - commit games before concurrent reads so each new context can find them
        var playerId = new PlayerId(Guid.NewGuid());
        var games = new List<Game>();

        for (int i = 0; i < 5; i++)
        {
            var game = new Game(playerId);
            games.Add(game);
            await _subject.SaveAsync(game, _cancellationToken);
        }
        await CommitAsync();

        // Act - each concurrent read uses its own DbContext (DbContext is not thread-safe)
        var queryTasks = Enumerable
            .Range(0, 10)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                return await new GameRepository(ctx).GetByPlayerIdAsync(
                    playerId,
                    _cancellationToken
                );
            });
        var results = await Task.WhenAll(queryTasks);

        // Assert
        foreach (var result in results)
        {
            result.Should().HaveCount(5);
            foreach (var game in games)
                result.Should().Contain(g => g.Id == game.Id);
        }
    }

    #endregion

    #region GetActiveGameByPlayerIdAsync Tests

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WhenPlayerHasActiveGame_ShouldReturnActiveGame()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetActiveGameByPlayerIdAsync(game.PlayerId, _cancellationToken);

        // Assert
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
        var result = await _subject.GetActiveGameByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WithMultipleActiveGames_ShouldReturnLastCreatedActiveGame()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game1 = new Game(playerId);
        var game2 = new Game(playerId);

        await _subject.SaveAsync(game1, _cancellationToken);
        await _subject.SaveAsync(game2, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetActiveGameByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().Be(playerId);
        result.Id.Should().Be(game2.Id); // Last created active game
        result.State.Should().Be(GameState.New);
    }

    [Fact]
    public async Task GetActiveGameByPlayerIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.GetActiveGameByPlayerIdAsync(playerId, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Data Integrity and State Consistency Tests

    [Fact]
    public async Task Repository_WhenSavingAndRetrieving_ShouldMaintainGameState()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        var originalState = game.State;
        var originalBoardSize = game.BoardSize;

        // Act
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();
        var retrievedGame = await _subject.GetByIdAsync(game.Id, _cancellationToken);

        // Assert
        retrievedGame.Should().NotBeNull();
        retrievedGame.State.Should().Be(originalState);
        retrievedGame.BoardSize.Should().Be(originalBoardSize);
        retrievedGame.PlayerId.Should().Be(game.PlayerId);
    }

    [Fact]
    public async Task Repository_ConcurrentSaveAndRead_ShouldMaintainConsistency()
    {
        // Arrange - persist game first so concurrent saves are UPDATEs, not INSERTs
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Act - concurrent reads and updates, each with their own DbContext
        var readTasks = Enumerable
            .Range(0, 20)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                return await new GameRepository(ctx).GetByIdAsync(game.Id, _cancellationToken);
            });

        var saveTasks = Enumerable
            .Range(0, 10)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                var repo = new GameRepository(ctx);
                await repo.SaveAsync(game, _cancellationToken);
                try
                {
                    await ctx.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Expected under optimistic concurrency (xmin): a losing writer
                    // sees the row already updated. Consistency is still maintained.
                }
            });

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
        var game = _fixture.CreateGameInStateStarted();
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        // Act - attack a cell to change game state
        game.Attack(BoardSide.Opponent, "A1");
        await _subject.SaveAsync(game, _cancellationToken);
        await CommitAsync();

        var updatedGame = await _subject.GetByIdAsync(game.Id, _cancellationToken);

        // Assert
        updatedGame.Should().NotBeNull();
        updatedGame.Id.Should().Be(game.Id);
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
            games.Add(new Game(playerId));

        // Act
        foreach (var g in games)
            await _subject.SaveAsync(g, _cancellationToken);
        await CommitAsync();

        var retrievedGames = await _subject.GetByPlayerIdAsync(playerId, _cancellationToken);

        // Assert
        retrievedGames.Should().HaveCount(gameCount);
        foreach (var game in games)
            retrievedGames.Should().Contain(g => g.Id == game.Id);
    }

    #endregion
}
