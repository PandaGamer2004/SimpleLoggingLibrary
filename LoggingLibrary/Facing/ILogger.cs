namespace LoggingLibrary;

public interface ILogger
{
    public void Log(string message, LogLevel logLevel);

    public void LogInformation(string message);

    public void LogTrace(string message);

    public void LogError(string message);
}