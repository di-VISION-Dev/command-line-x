using Microsoft.Extensions.Logging;

namespace diVISION.CommandLineX.Tests.Mocks
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LoggerMock<TCategory> : ILogger<TCategory>
    {
        private readonly List<KeyValuePair<LogLevel, string>> _messages = [];

        public IList<KeyValuePair<LogLevel, string>> Messages => _messages;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _messages.Add(new(logLevel, formatter(state, exception)));
        }
    }
}
