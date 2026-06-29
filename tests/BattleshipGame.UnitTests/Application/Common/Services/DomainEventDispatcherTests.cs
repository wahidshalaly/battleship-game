using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.SharedKernel;
using BattleshipGame.UnitTests.Domain.DomainModel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.UnitTests.Application.Common.Services;

public class DomainEventDispatcherTests
{
    private readonly GameFixture _fixture = new();
    private readonly IMediator _mediator;
    private readonly DomainEventDispatcher _dispatcher;

    public DomainEventDispatcherTests()
    {
        var logger = A.Fake<ILogger<DomainEventDispatcher>>();
        _mediator = A.Fake<IMediator>();
        _dispatcher = new DomainEventDispatcher(logger, _mediator);
    }

    [Fact]
    public async Task DispatchEventsAsync_WhenAggregateHasEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = _fixture.CreateGameInStateReady(playerId);
        var ct = CancellationToken.None;
        var publishedEvents = new List<IDomainEvent>();
        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, ct))
            .Invokes(
                (IDomainEvent domainEvent, CancellationToken _) => publishedEvents.Add(domainEvent)
            )
            .Returns(Task.CompletedTask);

        // Act
        var totalBeforeDispatch = game.DomainEvents.Count;
        await _dispatcher.DispatchEventsAsync(game, ct);
        var totalAfterDispatch = game.DomainEvents.Count;

        // Assert
        publishedEvents.Should().HaveCount(totalBeforeDispatch);
        publishedEvents.Should().ContainItemsAssignableTo<IDomainEvent>();
        totalAfterDispatch.Should().Be(0);

        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, ct))
            .MustHaveHappened(totalBeforeDispatch, Times.Exactly);
    }

    [Fact]
    public async Task DispatchEventsAsync_WhenAggregateHasNoEvents_ShouldNotPublishAnyEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);
        var ct = CancellationToken.None;

        // Act
        await _dispatcher.DispatchEventsAsync(game, ct);

        // Assert
        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, ct)).MustNotHaveHappened();
    }

    [Fact]
    public async Task DispatchEventsAsync_WithMultipleAggregates_ShouldDispatchAllEventsFromAllAggregates()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game1 = _fixture.CreateGameInStateReady(playerId);
        var game2 = _fixture.CreateGameInStateReady(playerId);
        var aggregates = new[] { game1, game2 };
        var ct = CancellationToken.None;

        var publishedEvents = new List<IDomainEvent>();
        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, ct))
            .Invokes(
                (IDomainEvent domainEvent, CancellationToken _) => publishedEvents.Add(domainEvent)
            )
            .Returns(Task.CompletedTask);

        // Act
        var totalBeforeDispatch = game1.DomainEvents.Count + game2.DomainEvents.Count;
        await _dispatcher.DispatchEventsAsync(aggregates, ct);
        var totalAfterDispatch = game1.DomainEvents.Count + game2.DomainEvents.Count;

        // Assert
        publishedEvents.Should().HaveCount(totalBeforeDispatch);
        publishedEvents.Should().ContainItemsAssignableTo<IDomainEvent>();
        totalAfterDispatch.Should().Be(0);

        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, ct))
            .MustHaveHappened(totalBeforeDispatch, Times.Exactly);
    }
}
