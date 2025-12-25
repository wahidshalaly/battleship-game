using Xunit.Abstractions;

namespace BattleshipGame.IntegrationTests;

public sealed class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new XunitLogger(categoryName, output);

    public void Dispose() { }

    private sealed class XunitLogger : ILogger
    {
        private readonly string _category;
        private readonly ITestOutputHelper _output;

        public XunitLogger(string category, ITestOutputHelper output)
        {
            _category = category;
            _output = output;
        }

        IDisposable ILogger.BeginScope<TState>(TState state) => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            _output.WriteLine($"[{logLevel}] {_category}: {formatter(state, exception)}");

            if (exception != null)
            {
                _output.WriteLine(exception.ToString());
            }
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();

            private NoopDisposable() { }

            public void Dispose() { }
        }
    }
}
