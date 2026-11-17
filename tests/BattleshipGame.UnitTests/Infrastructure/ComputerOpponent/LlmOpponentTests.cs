using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.ComputerOpponent;
using BattleshipGame.UnitTests.Domain.DomainModel;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.UnitTests.Infrastructure.ComputerOpponent;

public class LlmOpponentTests
{
    private const string SystemPrompt = "system prompt";
    private const string StrategicPrompt = "strategic prompt";

    private readonly GameFixture _fixture = new();
    private readonly ILlmChat _llmChat = A.Fake<ILlmChat>();
    private readonly LlmOpponent _opponent;

    public LlmOpponentTests()
    {
        var promptBuilder = A.Fake<IPromptBuilder>();
        A.CallTo(() => promptBuilder.BuildSystemPrompt()).Returns(SystemPrompt);
        A.CallTo(() => promptBuilder.BuildStrategicPrompt(A<GameSnapshot>._))
            .Returns(StrategicPrompt);

        _opponent = new LlmOpponent(promptBuilder, _llmChat, A.Fake<ILogger<LlmOpponent>>());
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenReplyIsValid_ShouldReturnCell()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        LlmReplies("""{"cell":"C4","reasoning":"probing the centre"}""");

        // Act
        var cell = await _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        cell.Should().Be("C4");
    }

    [Fact]
    public async Task SelectNextAttackAsync_ShouldSendSystemAndStrategicPrompts()
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        using var cts = new CancellationTokenSource();
        LlmReplies("""{"cell":"C4","reasoning":"r"}""");

        // Act
        await _opponent.SelectNextAttackAsync(game, cts.Token);

        // Assert
        A.CallTo(() => _llmChat.CompleteAsync(SystemPrompt, StrategicPrompt, cts.Token))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("""{"cell":"c4","reasoning":"r"}""")]
    [InlineData("""{"cell":"  C4 ","reasoning":"r"}""")]
    [InlineData("""{"Cell":"C4","Reasoning":"r"}""")]
    public async Task SelectNextAttackAsync_ShouldNormalizeCellCaseAndWhitespace(string reply)
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        LlmReplies(reply);

        // Act
        var cell = await _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        cell.Should().Be("C4");
    }

    [Theory]
    [InlineData("I think C4 looks promising!")]
    [InlineData("```json\n{\"cell\":\"C4\"}\n```")]
    [InlineData("")]
    public async Task SelectNextAttackAsync_WhenReplyIsNotJson_ShouldThrowAiOpponentException(
        string reply
    )
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        LlmReplies(reply);

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiOpponentException>();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("""{"cell":null,"reasoning":"r"}""")]
    [InlineData("""{"cell":"   ","reasoning":"r"}""")]
    [InlineData("""{"reasoning":"no cell"}""")]
    public async Task SelectNextAttackAsync_WhenCellIsMissing_ShouldThrowAiOpponentException(
        string reply
    )
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        LlmReplies(reply);

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiOpponentException>();
    }

    [Theory]
    [InlineData("K1")] // outside a 10x10 board
    [InlineData("A11")]
    [InlineData("not-a-cell")]
    public async Task SelectNextAttackAsync_WhenCellIsNotOnBoard_ShouldThrowAiOpponentException(
        string selectedCell
    )
    {
        // Arrange
        var game = _fixture.CreateGameInStateStarted();
        LlmReplies($$"""{"cell":"{{selectedCell}}","reasoning":"r"}""");

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiOpponentException>().WithMessage("*unavailable*");
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenCellWasAlreadyAttacked_ShouldThrowAiOpponentException()
    {
        // Arrange: one full round, so the opponent has already attacked A1 on the player's board.
        var game = _fixture.CreateGameInStateStarted();
        game.Attack(BoardSide.Opponent, "A1");
        game.Attack(BoardSide.Player, "A1");
        LlmReplies("""{"cell":"A1","reasoning":"r"}""");

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AiOpponentException>().WithMessage("*unavailable*A1*");
    }

    [Fact]
    public async Task SelectNextAttackAsync_WhenLlmCallFails_ShouldPropagateException()
    {
        // Arrange: transport errors are left to the resilience decorator to retry or fall back.
        var game = _fixture.CreateGameInStateStarted();
        A.CallTo(() => _llmChat.CompleteAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new HttpRequestException("boom"));

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private void LlmReplies(string content) =>
        A.CallTo(() => _llmChat.CompleteAsync(A<string>._, A<string>._, A<CancellationToken>._))
            .Returns(content);
}
