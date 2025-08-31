using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Xunit;

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
        var game = _fixture.CreateReadyGame(playerId);
        var cancellationToken = CancellationToken.None;
        var publishedEvents = new List<IDomainEvent>();

        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, cancellationToken))
            .Invokes((IDomainEvent domainEvent, CancellationToken _) => publishedEvents.Add(domainEvent))
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchEventsAsync(game, cancellationToken);

        // Assert
        publishedEvents.Should().HaveCount(game.DomainEvents.Count);
        publishedEvents.Should().ContainItemsAssignableTo<IDomainEvent>();

        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, cancellationToken))
            .MustHaveHappened(game.DomainEvents.Count, Times.Exactly);
    }

    [Fact]
    public async Task DispatchEventsAsync_WhenAggregateHasNoEvents_ShouldNotPublishAnyEvents()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game = new Game(playerId);
        var cancellationToken = CancellationToken.None;

        // Act
        await _dispatcher.DispatchEventsAsync(game, cancellationToken);

        // Assert
        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, cancellationToken)).MustNotHaveHappened();
    }

    [Fact]
    public async Task DispatchEventsAsync_WithMultipleAggregates_ShouldDispatchAllEventsFromAllAggregates()
    {
        // Arrange
        var playerId = new PlayerId(Guid.NewGuid());
        var game1 = _fixture.CreateReadyGame(playerId);
        var game2 = _fixture.CreateReadyGame(playerId);
        var aggregates = new[] { game1, game2 };
        var cancellationToken = CancellationToken.None;

        var publishedEvents = new List<IDomainEvent>();
        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, cancellationToken))
            .Invokes((IDomainEvent domainEvent, CancellationToken _) => publishedEvents.Add(domainEvent))
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchEventsAsync(aggregates, cancellationToken);

        // Assert
        var totalExpectedEvents = game1.DomainEvents.Count + game2.DomainEvents.Count;
        publishedEvents.Should().HaveCount(totalExpectedEvents);

        A.CallTo(() => _mediator.Publish(A<IDomainEvent>._, cancellationToken))
            .MustHaveHappened(totalExpectedEvents, Times.Exactly);
    }
}
