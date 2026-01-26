using System;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.Resilience;
using BattleshipGame.UnitTests.Domain.DomainModel;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Xunit;

namespace BattleshipGame.UnitTests.Infrastructure.Resilience;

public class ResilientComputerOpponentDecoratorTests
{
    private readonly GameFixture _fixture = new();
    private readonly IComputerOpponent _innerOpponent;
    private readonly IComputerOpponent _fallbackOpponent;
    private readonly ILogger<ResilientComputerOpponentDecorator> _logger;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ResilientComputerOpponentDecoratorTests()
    {
        _innerOpponent = A.Fake<IComputerOpponent>();
        _fallbackOpponent = A.Fake<IComputerOpponent>();
        _logger = A.Fake<ILogger<ResilientComputerOpponentDecorator>>();

        A.CallTo(() => _innerOpponent.Strategy).Returns(OpponentStrategy.SemanticKernel);
        A.CallTo(() => _fallbackOpponent.Strategy).Returns(OpponentStrategy.Random);
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenInnerOpponentSucceeds_ShouldReturnResult()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        var expectedCell = "A1";

        A.CallTo(() => _innerOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Returns(expectedCell);

        var pipeline = CreatePassThroughPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        var result = await decorator.SelectNextAttackAsync(game, _cancellationToken);

        // Assert
        result.Should().Be(expectedCell);
        A.CallTo(() => _innerOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(A<Game>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenAiOpponentExceptionThrown_ShouldUseFallback()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        var fallbackCell = "B2";

        A.CallTo(() => _innerOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Throws(new AiOpponentException("LLM selected unavailable cell"));

        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Returns(fallbackCell);

        var pipeline = CreatePassThroughPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        var result = await decorator.SelectNextAttackAsync(game, _cancellationToken);

        // Assert
        result.Should().Be(fallbackCell);
        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenBrokenCircuitExceptionThrown_ShouldUseFallback()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();
        var fallbackCell = "C3";

        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Returns(fallbackCell);

        var pipeline = CreateBrokenCircuitPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        var result = await decorator.SelectNextAttackAsync(game, _cancellationToken);

        // Assert
        result.Should().Be(fallbackCell);
        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenAiOpponentExceptionThrown_ShouldLogWarning()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();

        A.CallTo(() => _innerOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Throws(new AiOpponentException("Test exception"));

        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Returns("A1");

        var pipeline = CreatePassThroughPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        await decorator.SelectNextAttackAsync(game, _cancellationToken);

        // Assert
        // Verify warning was logged (using FakeItEasy's call matching)
        A.CallTo(_logger).Where(call => call.Method.Name == "Log").MustHaveHappened();
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenBrokenCircuitExceptionThrown_ShouldLogWarning()
    {
        // Arrange
        var game = _fixture.CreateGameInStateReady();

        A.CallTo(() => _fallbackOpponent.SelectNextAttackAsync(game, _cancellationToken))
            .Returns("A1");

        var pipeline = CreateBrokenCircuitPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        await decorator.SelectNextAttackAsync(game, _cancellationToken);

        // Assert
        // Verify warning was logged
        A.CallTo(_logger).Where(call => call.Method.Name == "Log").MustHaveHappened();
    }

    [Fact]
    public void Strategy_ShouldReturnInnerOpponentStrategy()
    {
        // Arrange
        var pipeline = CreatePassThroughPipeline();
        var decorator = new ResilientComputerOpponentDecorator(
            _innerOpponent,
            pipeline,
            _fallbackOpponent,
            _logger
        );

        // Act
        var strategy = decorator.Strategy;

        // Assert
        strategy.Should().Be(OpponentStrategy.SemanticKernel);
    }

    private static ResiliencePipeline<string> CreatePassThroughPipeline()
    {
        return new ResiliencePipelineBuilder<string>().Build();
    }

    private static ResiliencePipeline<string> CreateBrokenCircuitPipeline()
    {
        return new ResiliencePipelineBuilder<string>()
            .AddStrategy(_ => new BrokenCircuitThrowingStrategy())
            .Build();
    }

    /// <summary>
    /// Custom strategy that always throws BrokenCircuitException
    /// </summary>
    private class BrokenCircuitThrowingStrategy : ResilienceStrategy<string>
    {
        protected override async ValueTask<Outcome<string>> ExecuteCore<TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<string>>> callback,
            ResilienceContext context,
            TState state
        )
        {
            await Task.CompletedTask;
            throw new BrokenCircuitException("Circuit is open for testing");
        }
    }
}
