using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Application.Common;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.ComputerOpponent;
using BattleshipGame.UnitTests.Domain.DomainModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BattleshipGame.UnitTests.Infrastructure.ComputerOpponent;

public class SemanticKernelOpponentTests
{
    private const string SystemPrompt = "system prompt";
    private const string StrategicPrompt = "strategic prompt";

    private readonly GameFixture _fixture = new();
    private readonly IChatCompletionService _chatService = A.Fake<IChatCompletionService>();
    private readonly SemanticKernelOpponent _opponent;
    private ChatHistory? _sentHistory;

    public SemanticKernelOpponentTests()
    {
        var promptBuilder = A.Fake<IPromptBuilder>();
        A.CallTo(() => promptBuilder.BuildSystemPrompt()).Returns(SystemPrompt);
        A.CallTo(() => promptBuilder.BuildStrategicPrompt(A<GameSnapshot>._))
            .Returns(StrategicPrompt);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(_chatService);

        _opponent = new SemanticKernelOpponent(
            promptBuilder,
            kernelBuilder.Build(),
            A.Fake<ILogger<SemanticKernelOpponent>>()
        );
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
        LlmReplies("""{"cell":"C4","reasoning":"r"}""");

        // Act
        await _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        _sentHistory.Should().NotBeNull();
        _sentHistory!
            .Select(m => (m.Role, m.Content))
            .Should()
            .Equal((AuthorRole.System, SystemPrompt), (AuthorRole.User, StrategicPrompt));
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
    public async Task SelectNextAttackAsync_WhenChatServiceFails_ShouldPropagateException()
    {
        // Arrange: transport errors are left to the resilience decorator to retry or fall back.
        var game = _fixture.CreateGameInStateStarted();
        A.CallTo(() =>
                _chatService.GetChatMessageContentsAsync(
                    A<ChatHistory>._,
                    A<PromptExecutionSettings>._,
                    A<Kernel>._,
                    A<CancellationToken>._
                )
            )
            .ThrowsAsync(new HttpRequestException("boom"));

        // Act
        var act = () => _opponent.SelectNextAttackAsync(game, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private void LlmReplies(string content)
    {
        A.CallTo(() =>
                _chatService.GetChatMessageContentsAsync(
                    A<ChatHistory>._,
                    A<PromptExecutionSettings>._,
                    A<Kernel>._,
                    A<CancellationToken>._
                )
            )
            .Invokes(
                (ChatHistory history, PromptExecutionSettings? _, Kernel? _, CancellationToken _) =>
                    _sentHistory = history
            )
            .Returns(
                Task.FromResult<IReadOnlyList<ChatMessageContent>>([
                    new ChatMessageContent(AuthorRole.Assistant, content),
                ])
            );
    }
}
