using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Features.Players.Commands;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.UnitTests.Application.Features.Players.Commands;

public class CreatePlayerCommandHandlerTests
{
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
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
        var command = new CreatePlayerCommand(username, "auth|subject");
        var expectedPlayerId = new PlayerId(Guid.NewGuid());

        A.CallTo(() => _repository.UsernameExistsAsync(username, _cancellationToken))
            .Returns(false);
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _cancellationToken))
            .Returns(expectedPlayerId);

        // Act
        var result = await _handler.Handle(command, _cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        A.CallTo(() => _repository.UsernameExistsAsync(username, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() =>
                _repository.SaveAsync(
                    A<Player>.That.Matches(p => p.Username == username),
                    _cancellationToken
                )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenUsernameExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string username = "ExistingPlayer";
        var command = new CreatePlayerCommand(username, "auth|subject");

        A.CallTo(() => _repository.UsernameExistsAsync(username, _cancellationToken)).Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, _cancellationToken)
        );

        exception.Message.Should().Contain($"A player with username '{username}' already exists.");

        A.CallTo(() => _repository.UsernameExistsAsync(username, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _cancellationToken))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenNullOrEmptyOrWhitespaceUsername_ShouldThrowsException(
        string? username
    )
    {
        // Arrange
        var command = new CreatePlayerCommand(username!, "auth|subject");

        // Act
        var act = () => _handler.Handle(command, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();

        A.CallTo(() => _repository.UsernameExistsAsync(username!, _cancellationToken))
            .MustNotHaveHappened();

        A.CallTo(() =>
                _repository.SaveAsync(
                    A<Player>.That.Matches(p => p.Username == username),
                    _cancellationToken
                )
            )
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WhenNullOrEmptyIdentitySubject_ShouldThrowForbidden(string? subject)
    {
        // Arrange — a valid username but no authenticated identity.
        var command = new CreatePlayerCommand("ValidName", subject!);

        // Act
        var act = () => _handler.Handle(command, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _cancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenIdentityAlreadyHasProfile_ShouldThrowInvalidOperationException()
    {
        // Arrange — a player already exists for this identity subject.
        const string subject = "auth|existing";
        var command = new CreatePlayerCommand("NewName", subject);

        A.CallTo(() => _repository.GetByIdentitySubjectAsync(subject, _cancellationToken))
            .Returns(new Player(new PlayerId(Guid.NewGuid()), "existing", subject));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, _cancellationToken)
        );
        exception.Message.Should().Contain("already exists for the current identity");

        // The username check and save must be short-circuited.
        A.CallTo(() => _repository.UsernameExistsAsync(A<string>._, _cancellationToken))
            .MustNotHaveHappened();
        A.CallTo(() => _repository.SaveAsync(A<Player>._, _cancellationToken))
            .MustNotHaveHappened();
    }
}
