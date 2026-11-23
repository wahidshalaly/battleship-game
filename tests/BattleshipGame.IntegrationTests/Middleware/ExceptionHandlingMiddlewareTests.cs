using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Domain.Exceptions;
using BattleshipGame.WebAPI.Middleware;

namespace BattleshipGame.IntegrationTests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly RecordingLogger _logger = new();

    public static TheoryData<Exception, int> ClientErrors =>
        new()
        {
            { new InvalidCredentialsException(), StatusCodes.Status401Unauthorized },
            { new ForbiddenAccessException("no profile"), StatusCodes.Status403Forbidden },
            { new PlayerNotFoundException(Guid.NewGuid()), StatusCodes.Status404NotFound },
            {
                new PlayerHasActiveGameException(Guid.NewGuid(), Guid.NewGuid()),
                StatusCodes.Status409Conflict
            },
            { new ArgumentException("bad input"), StatusCodes.Status400BadRequest },
        };

    [Theory]
    [MemberData(nameof(ClientErrors))]
    public async Task InvokeAsync_WhenClientError_ShouldLogWarningWithoutStackTrace(
        Exception exception,
        int expectedStatus
    )
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await InvokeThrowing(exception, context);

        // Assert
        context.Response.StatusCode.Should().Be(expectedStatus);
        var entry = _logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Exception.Should().BeNull();
        entry.Message.Should().Contain(expectedStatus.ToString()).And.Contain(exception.Message);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedError_ShouldLogErrorWithException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new NullReferenceException("boom");

        // Act
        await InvokeThrowing(exception, context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var entry = _logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Error);
        entry.Exception.Should().BeSameAs(exception);
    }

    private Task InvokeThrowing(Exception exception, HttpContext context)
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, _logger);
        return middleware.InvokeAsync(context);
    }

    private sealed class RecordingLogger : ILogger<ExceptionHandlingMiddleware>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => Entries.Add((logLevel, exception, formatter(state, exception)));
    }
}
