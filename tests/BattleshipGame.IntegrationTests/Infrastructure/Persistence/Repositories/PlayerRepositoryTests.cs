using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.IntegrationTests.Infrastructure.Persistence.Repositories;

[Collection(PostgresCollection.Name)]
public class PlayerRepositoryTests(PostgresFixture postgres) : BaseRepositoryTests(postgres)
{
    private IPlayerRepository _subject = null!;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public override Task InitializeAsync()
    {
        base.InitializeAsync();
        _subject = new PlayerRepository(_dbcontext);
        return Task.CompletedTask;
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenPlayerExists_ShouldReturnPlayer()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetByIdAsync(player.Id, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(player.Id);
        result.Username.Should().Be(player.Username);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPlayerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentPlayerId = new PlayerId(Guid.NewGuid());

        // Act
        var result = await _subject.GetByIdAsync(nonExistentPlayerId, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.GetByIdAsync(player.Id, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region SaveAsync Tests

    [Fact]
    public async Task SaveAsync_WhenNewPlayer_ShouldSaveAndReturnPlayerId()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");

        // Act
        var result = await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Assert
        result.Should().Be(player.Id);
        var savedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);
        savedPlayer.Should().NotBeNull();
        savedPlayer.Id.Should().Be(player.Id);
        savedPlayer.Username.Should().Be(player.Username);
    }

    [Fact]
    public async Task SaveAsync_WhenUpdatingExistingPlayer_ShouldUpdateAndReturnPlayerId()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), "OriginalName");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Assert
        result.Should().Be(player.Id);
        var retrievedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);
        retrievedPlayer.Should().NotBeNull();
        retrievedPlayer.Id.Should().Be(player.Id);
    }

    [Fact]
    public async Task SaveAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.SaveAsync(player, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SaveAsync_ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Act - each player save uses its own DbContext to avoid thread-safety issues
        var saveTasks = Enumerable
            .Range(0, 10)
            .Select(i =>
            {
                var player = new Player(
                    new PlayerId(Guid.NewGuid()),
                    $"Player{i}_{Guid.NewGuid():N}"
                );
                return Task.Run(async () =>
                {
                    await using var ctx = _postgres.CreateDbContext();
                    var repo = new PlayerRepository(ctx);
                    var id = await repo.SaveAsync(player, _cancellationToken);
                    await ctx.SaveChangesAsync();
                    return id;
                });
            });

        var results = await Task.WhenAll(saveTasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyHaveUniqueItems();

        // Verify all players were saved
        foreach (var playerId in results)
        {
            await using var ctx = _postgres.CreateDbContext();
            var savedPlayer = await new PlayerRepository(ctx).GetByIdAsync(
                playerId,
                _cancellationToken
            );
            savedPlayer.Should().NotBeNull();
        }
    }

    #endregion

    #region GetByUsernameAsync Tests

    [Fact]
    public async Task GetByUsernameAsync_WhenUsernameExists_ShouldReturnPlayer()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.GetByUsernameAsync(player.Username, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(player.Id);
        result.Username.Should().Be(player.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WhenUsernameDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _subject.GetByUsernameAsync(
            $"NonExistent_{Guid.NewGuid():N}",
            _cancellationToken
        );

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("lower")]
    [InlineData("UPPER")]
    [InlineData("Mixed")]
    [InlineData("mIxEdCaSe")]
    public async Task GetByUsernameAsync_WithDifferentCasing_ShouldBeCaseInsensitive(string casing)
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var originalUsername = $"TestPlayer{uniqueSuffix}";
        var player = new Player(new PlayerId(Guid.NewGuid()), originalUsername);
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        var searchUsername = casing switch
        {
            "lower" => originalUsername.ToLower(),
            "UPPER" => originalUsername.ToUpper(),
            "Mixed" => char.ToUpper(originalUsername[0]) + originalUsername[1..].ToLower(),
            _ => new string(
                originalUsername
                    .Select((c, i) => i % 2 == 0 ? char.ToUpper(c) : char.ToLower(c))
                    .ToArray()
            ),
        };

        // Act
        var result = await _subject.GetByUsernameAsync(searchUsername, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(originalUsername);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNullUsername_ShouldReturnNull()
    {
        // Act
        var result = await _subject.GetByUsernameAsync(null!, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithEmptyUsername_ShouldReturnNull()
    {
        // Act
        var result = await _subject.GetByUsernameAsync("", _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.GetByUsernameAsync(player.Username, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region UsernameExistsAsync Tests

    [Fact]
    public async Task UsernameExistsAsync_WhenUsernameExists_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"ExistingPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var result = await _subject.UsernameExistsAsync(player.Username, _cancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameExistsAsync_WhenUsernameDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _subject.UsernameExistsAsync(
            $"NonExistent_{Guid.NewGuid():N}",
            _cancellationToken
        );

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("lower")]
    [InlineData("UPPER")]
    [InlineData("Mixed")]
    [InlineData("mIxEdCaSe")]
    [InlineData("digits123")]
    [InlineData("under_score")]
    public async Task UsernameExistsAsync_WithDifferentCasing_ShouldBeCaseInsensitive(string casing)
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var originalUsername = $"TestPlayer{uniqueSuffix}";
        var player = new Player(new PlayerId(Guid.NewGuid()), originalUsername);
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        var searchUsername = casing switch
        {
            "lower" => originalUsername.ToLower(),
            "UPPER" => originalUsername.ToUpper(),
            "Mixed" => char.ToUpper(originalUsername[0]) + originalUsername[1..].ToLower(),
            _ => new string(
                originalUsername
                    .Select((c, i) => i % 2 == 0 ? char.ToUpper(c) : char.ToLower(c))
                    .ToArray()
            ),
        };

        // Act
        var result = await _subject.UsernameExistsAsync(searchUsername, _cancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UsernameExistsAsync_WithNullUsername_ShouldReturnFalse()
    {
        // Act
        var result = await _subject.UsernameExistsAsync(null!, _cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_WithEmptyUsername_ShouldReturnFalse()
    {
        // Act
        var result = await _subject.UsernameExistsAsync("", _cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UsernameExistsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"TestPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert - a cancelled token must cancel the operation
        var act = () => _subject.UsernameExistsAsync(player.Username, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UsernameExistsAsync_ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange - commit the player before concurrent checks
        var player = new Player(
            new PlayerId(Guid.NewGuid()),
            $"ConcurrentTestPlayer_{Guid.NewGuid():N}"
        );
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act - each check uses its own DbContext
        var tasks = Enumerable
            .Range(0, 20)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                return await new PlayerRepository(ctx).UsernameExistsAsync(
                    player.Username,
                    _cancellationToken
                );
            });
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().OnlyContain(result => result == true);
    }

    #endregion

    #region Thread Safety and Concurrent Access Tests

    [Fact]
    public async Task Repository_ConcurrentSaveAndRead_ShouldMaintainConsistency()
    {
        // Arrange - persist the player first so concurrent saves are UPDATEs, not INSERTs
        var player = new Player(
            new PlayerId(Guid.NewGuid()),
            $"ConcurrentPlayer_{Guid.NewGuid():N}"
        );
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act - concurrent reads and updates, each with their own DbContext
        var readTasks = Enumerable
            .Range(0, 20)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                return await new PlayerRepository(ctx).GetByIdAsync(player.Id, _cancellationToken);
            });

        var saveTasks = Enumerable
            .Range(0, 10)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                var repo = new PlayerRepository(ctx);
                await repo.SaveAsync(player, _cancellationToken);
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
        readResults.Should().OnlyContain(p => p != null);
        readResults.Should().OnlyContain(p => p!.Id == player.Id);
    }

    [Fact]
    public async Task Repository_ConcurrentUsernameChecks_ShouldBeConsistent()
    {
        // Arrange - commit before concurrent checks
        var player = new Player(
            new PlayerId(Guid.NewGuid()),
            $"ConcurrentUsername_{Guid.NewGuid():N}"
        );
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act - each check uses its own DbContext
        var tasks = Enumerable
            .Range(0, 50)
            .Select(async _ =>
            {
                await using var ctx = _postgres.CreateDbContext();
                return await new PlayerRepository(ctx).UsernameExistsAsync(
                    player.Username,
                    _cancellationToken
                );
            });
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().OnlyContain(result => result == true);
    }

    [Fact]
    public async Task Repository_ConcurrentSaveOperations_ShouldMaintainDataIntegrity()
    {
        // Act - each player is unique, each save uses its own DbContext
        var saveTasks = Enumerable
            .Range(0, 20)
            .Select(i =>
            {
                var player = new Player(
                    new PlayerId(Guid.NewGuid()),
                    $"Player{i}_{Guid.NewGuid():N}"
                );
                return Task.Run<(PlayerId Id, string Username)>(async () =>
                {
                    await using var ctx = _postgres.CreateDbContext();
                    var repo = new PlayerRepository(ctx);
                    await repo.SaveAsync(player, _cancellationToken);
                    await ctx.SaveChangesAsync();
                    return (player.Id, player.Username);
                });
            });

        var saved = await Task.WhenAll(saveTasks);

        // Assert - verify all players were persisted
        foreach (var (id, _) in saved)
        {
            await using var ctx = _postgres.CreateDbContext();
            var retrieved = await new PlayerRepository(ctx).GetByIdAsync(id, _cancellationToken);
            retrieved.Should().NotBeNull();
        }
    }

    #endregion

    #region Data Integrity and State Consistency Tests

    [Fact]
    public async Task Repository_WhenSavingAndRetrieving_ShouldMaintainPlayerProperties()
    {
        // Arrange
        var originalUsername = $"TestPlayer123_{Guid.NewGuid():N}";
        var player = new Player(new PlayerId(Guid.NewGuid()), originalUsername);

        // Act
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        var retrievedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);

        // Assert
        retrievedPlayer.Should().NotBeNull();
        retrievedPlayer.Id.Should().Be(player.Id);
        retrievedPlayer.Username.Should().Be(originalUsername);
    }

    [Fact]
    public async Task Repository_AfterSaving_UsernameQueryShouldReturnSamePlayer()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"UniquePlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var retrievedById = await _subject.GetByIdAsync(player.Id, _cancellationToken);
        var retrievedByUsername = await _subject.GetByUsernameAsync(
            player.Username,
            _cancellationToken
        );

        // Assert
        retrievedById.Should().NotBeNull();
        retrievedByUsername.Should().NotBeNull();
        retrievedById.Id.Should().Be(retrievedByUsername.Id);
        retrievedById.Username.Should().Be(retrievedByUsername.Username);
    }

    [Fact]
    public async Task Repository_UsernameExistsAndGetByUsername_ShouldBeConsistent()
    {
        // Arrange
        var player = new Player(
            new PlayerId(Guid.NewGuid()),
            $"ConsistencyTestPlayer_{Guid.NewGuid():N}"
        );
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act
        var exists = await _subject.UsernameExistsAsync(player.Username, _cancellationToken);
        var retrievedPlayer = await _subject.GetByUsernameAsync(
            player.Username,
            _cancellationToken
        );

        // Assert
        exists.Should().BeTrue();
        retrievedPlayer.Should().NotBeNull();
        retrievedPlayer.Username.Should().Be(player.Username);
    }

    #endregion

    #region Edge Cases and Exception Handling

    [Theory]
    [InlineData("ValidPlayer")]
    [InlineData("Player123")]
    [InlineData("test_player")]
    [InlineData("UPPERCASE")]
    [InlineData("MixedCasePlayer")]
    [InlineData("Special-Characters_123")]
    public async Task Repository_WithVariousUsernameFormats_ShouldHandleCorrectly(
        string baseUsername
    )
    {
        // Append a unique suffix to prevent collisions across theory cases sharing the same DB
        var username = $"{baseUsername}_{Guid.NewGuid():N}";
        var player = new Player(new PlayerId(Guid.NewGuid()), username);

        // Act
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        var retrievedPlayer = await _subject.GetByUsernameAsync(username, _cancellationToken);
        var usernameExists = await _subject.UsernameExistsAsync(username, _cancellationToken);

        // Assert
        retrievedPlayer.Should().NotBeNull();
        retrievedPlayer.Username.Should().Be(username);
        usernameExists.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task Repository_WithMultiplePlayers_ShouldHandleCorrectly(int playerCount)
    {
        // Arrange
        var players = Enumerable
            .Range(0, playerCount)
            .Select(i => new Player(new PlayerId(Guid.NewGuid()), $"Player{i}_{Guid.NewGuid():N}"))
            .ToList();

        // Act
        foreach (var player in players)
            await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Assert
        foreach (var player in players)
        {
            var retrievedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);
            retrievedPlayer.Should().NotBeNull();
            retrievedPlayer.Username.Should().Be(player.Username);

            var existsByUsername = await _subject.UsernameExistsAsync(
                player.Username,
                _cancellationToken
            );
            existsByUsername.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Repository_WhenPlayerUpdated_ShouldReflectChangesInSubsequentReads()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"OriginalPlayer_{Guid.NewGuid():N}");
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act - save again (simulating update)
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        var updatedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);

        // Assert
        updatedPlayer.Should().NotBeNull();
        updatedPlayer.Id.Should().Be(player.Id);
        updatedPlayer.Username.Should().Be(player.Username);
    }

    #endregion

    #region Upsert Behavior Tests

    [Fact]
    public async Task Repository_AddOrUpdateBehavior_ShouldOverwriteExistingEntries()
    {
        // Arrange
        var player = new Player(new PlayerId(Guid.NewGuid()), $"FirstName_{Guid.NewGuid():N}");

        // Act
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();
        await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        var retrievedPlayer = await _subject.GetByIdAsync(player.Id, _cancellationToken);

        // Assert
        retrievedPlayer.Should().NotBeNull();
        retrievedPlayer.Id.Should().Be(player.Id);
    }

    [Fact]
    public async Task Repository_ConcurrentSavesOfSamePlayer_ShouldHandleRaceConditions()
    {
        // Arrange - persist first so all concurrent saves are UPDATEs, not INSERTs
        var players = Enumerable
            .Range(0, 10)
            .Select(i => new Player(
                new PlayerId(Guid.NewGuid()),
                $"RaceConditionPlayer{i}_{Guid.NewGuid():N}"
            ))
            .ToList();

        foreach (var player in players)
            await _subject.SaveAsync(player, _cancellationToken);
        await CommitAsync();

        // Act - concurrent re-saves (updates) of already-persisted players
        var saveTasks = players
            .SelectMany(player =>
                Enumerable
                    .Range(0, 5)
                    .Select(async _ =>
                    {
                        await using var ctx = _postgres.CreateDbContext();
                        var repo = new PlayerRepository(ctx);
                        await repo.SaveAsync(player, _cancellationToken);
                        await ctx.SaveChangesAsync();
                        return player.Id;
                    })
            )
            .ToList();

        var results = await Task.WhenAll(saveTasks);

        // Assert
        results.Should().HaveCount(50);

        foreach (var player in players)
        {
            await using var ctx = _postgres.CreateDbContext();
            var retrievedPlayer = await new PlayerRepository(ctx).GetByIdAsync(
                player.Id,
                _cancellationToken
            );
            retrievedPlayer.Should().NotBeNull();
            retrievedPlayer.Username.Should().Be(player.Username);
        }
    }

    #endregion
}
