using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Players.Commands;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Application.Features.Players.Commands;

public class CreatePlayerCommandHandlerTests
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly IPlayerRepository _repository;
    private readonly CreatePlayerCommandHandler _handler;

    public CreatePlayerCommandHandlerTests()
    {
        _repository = A.Fake<IPlayerRepository>();
        _handler = new CreatePlayerCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenValidUsername_ShouldCreatePlayerAndReturnResult()
    {
        // Arrange
        const string username = "TestPlayer";
        var command = new CreatePlayerCommand(username);
        var expectedPlayerId = new PlayerId(Guid.NewGuid());

        A.CallTo(() => _repository.UsernameExistsAsync(username, _ct)).Returns(false);
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _ct)).Returns(expectedPlayerId);

        // Act
        var result = await _handler.Handle(command, _ct);

        // Assert
        result.Should().NotBeNull();
        result.PlayerId.Should().NotBe(new PlayerId(Guid.Empty));

        A.CallTo(() => _repository.UsernameExistsAsync(username, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repository.SaveAsync(
            A<Player>.That.Matches(p => p.Username == username), _ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenUsernameExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string username = "ExistingPlayer";
        var command = new CreatePlayerCommand(username);

        A.CallTo(() => _repository.UsernameExistsAsync(username, _ct)).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, _ct));

        exception.Message.Should().Contain($"A player with username '{username}' already exists.");

        A.CallTo(() => _repository.UsernameExistsAsync(username, _ct))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _ct))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenNullOrEmptyOrWhitespaceUsername_ShouldThrowsException(string? username)
    {
        // Arrange
        var command = new CreatePlayerCommand(username!);

        // Act
        var act = () => _handler.Handle(command, _ct);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        A.CallTo(() => _repository.UsernameExistsAsync(username!, _ct))
            .MustNotHaveHappened();

        A.CallTo(() => _repository.SaveAsync(
            A<Player>.That.Matches(p => p.Username == username), _ct))
            .MustNotHaveHappened();
    }
}