namespace LoggingLibrary;

public interface ILoggingSource
{
    public void WriteEntry(LogEntry entry);

    public Task WriteEntryAsync(LogEntry entry, CancellationToken ct);
}


