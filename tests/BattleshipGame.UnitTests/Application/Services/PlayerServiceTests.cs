using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Common.Security;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.UnitTests.Application.Services;

public class PlayerServiceTests
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly IMediator _mediator = A.Fake<IMediator>();
    private readonly ICurrentUser _currentUser = A.Fake<ICurrentUser>();
    private readonly PlayerService _service;

    public PlayerServiceTests()
    {
        _service = new PlayerService(_mediator, _currentUser);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenNoSubject_ReturnsNullWithoutQuerying()
    {
        // Arrange
        A.CallTo(() => _currentUser.SubjectId).Returns((string?)null);

        // Act
        var result = await _service.GetCurrentAsync(_ct);

        // Assert
        result.Should().BeNull();
        A.CallTo(() => _mediator.Send(A<GetPlayerByIdentitySubjectQuery>._, _ct))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GetCurrentAsync_WhenSubjectPresent_ReturnsPlayerFromQuery()
    {
        // Arrange
        const string subject = "keycloak-sub";
        var player = new Player(new PlayerId(Guid.NewGuid()), "alice", subject);
        A.CallTo(() => _currentUser.SubjectId).Returns(subject);
        A.CallTo(() =>
                _mediator.Send(
                    A<GetPlayerByIdentitySubjectQuery>.That.Matches(q =>
                        q.IdentitySubject == subject
                    ),
                    _ct
                )
            )
            .Returns(player);

        // Act
        var result = await _service.GetCurrentAsync(_ct);

        // Assert
        result.Should().BeSameAs(player);
    }

    [Fact]
    public async Task GetCurrentRequiredAsync_WhenNoProfile_ThrowsForbidden()
    {
        // Arrange
        A.CallTo(() => _currentUser.SubjectId).Returns("sub-without-profile");
        A.CallTo(() => _mediator.Send(A<GetPlayerByIdentitySubjectQuery>._, _ct))
            .Returns((Player?)null);

        // Act
        var act = () => _service.GetCurrentRequiredAsync(_ct);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetCurrentRequiredAsync_WhenProfileExists_ReturnsPlayer()
    {
        // Arrange
        const string subject = "sub-with-profile";
        var player = new Player(new PlayerId(Guid.NewGuid()), "bob", subject);
        A.CallTo(() => _currentUser.SubjectId).Returns(subject);
        A.CallTo(() => _mediator.Send(A<GetPlayerByIdentitySubjectQuery>._, _ct)).Returns(player);

        // Act
        var result = await _service.GetCurrentRequiredAsync(_ct);

        // Assert
        result.Should().BeSameAs(player);
    }
}
