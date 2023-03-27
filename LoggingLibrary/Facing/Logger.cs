namespace LoggingLibrary.Facing;

public class Logger : ILogger
{
    private ILoggingSource loggingSource;
    public void Log(string message, LogLevel logLevel)
    {
        //Just run write on concurrent threads without waiting
        _ = loggingSource.WriteEntryAsync(new LogEntry(message, logLevel), CancellationToken.None);
    }

    public void LogWarning(string message)
        => Log(message, LogLevel.Warning);

    public void LogTrace(string message)
        => Log(message, LogLevel.Trace);

    public void LogError(string message)
        => Log(message, LogLevel.Error);
}