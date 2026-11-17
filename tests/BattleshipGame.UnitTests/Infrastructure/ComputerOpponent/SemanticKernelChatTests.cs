using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Infrastructure.ComputerOpponent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BattleshipGame.UnitTests.Infrastructure.ComputerOpponent;

public class SemanticKernelChatTests
{
    private readonly IChatCompletionService _chatCompletion = A.Fake<IChatCompletionService>();
    private readonly SemanticKernelChat _chat;
    private ChatHistory? _sentHistory;
    private PromptExecutionSettings? _sentSettings;

    public SemanticKernelChatTests() => _chat = new SemanticKernelChat(_chatCompletion);

    [Fact]
    public async Task CompleteAsync_ShouldSendSystemThenUserMessage()
    {
        // Arrange
        ModelReplies("reply");

        // Act
        await _chat.CompleteAsync("system prompt", "user prompt", CancellationToken.None);

        // Assert
        _sentHistory.Should().NotBeNull();
        _sentHistory!
            .Select(m => (m.Role, m.Content))
            .Should()
            .Equal((AuthorRole.System, "system prompt"), (AuthorRole.User, "user prompt"));
    }

    [Fact]
    public async Task CompleteAsync_ShouldRequestShortLowTemperatureReply()
    {
        // Arrange
        ModelReplies("reply");

        // Act
        await _chat.CompleteAsync("s", "u", CancellationToken.None);

        // Assert
        var settings = _sentSettings.Should().BeOfType<OpenAIPromptExecutionSettings>().Subject;
        settings.MaxTokens.Should().Be(100);
        settings.Temperature.Should().Be(0.3);
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnReplyContent()
    {
        // Arrange
        ModelReplies("""{"cell":"C4"}""");

        // Act
        var reply = await _chat.CompleteAsync("s", "u", CancellationToken.None);

        // Assert
        reply.Should().Be("""{"cell":"C4"}""");
    }

    [Fact]
    public async Task CompleteAsync_WhenReplyHasNoContent_ShouldReturnEmptyString()
    {
        // Arrange
        ModelReplies(null);

        // Act
        var reply = await _chat.CompleteAsync("s", "u", CancellationToken.None);

        // Assert
        reply.Should().BeEmpty();
    }

    private void ModelReplies(string? content) =>
        A.CallTo(() =>
                _chatCompletion.GetChatMessageContentsAsync(
                    A<ChatHistory>._,
                    A<PromptExecutionSettings>._,
                    A<Kernel>._,
                    A<CancellationToken>._
                )
            )
            .Invokes(
                (
                    ChatHistory history,
                    PromptExecutionSettings? settings,
                    Kernel? _,
                    CancellationToken _
                ) =>
                {
                    _sentHistory = history;
                    _sentSettings = settings;
                }
            )
            .Returns(
                Task.FromResult<IReadOnlyList<ChatMessageContent>>([
                    new ChatMessageContent(AuthorRole.Assistant, content),
                ])
            );
}
