namespace LoggingLibrary;

public class NotificationDecorator: ILoggingSource
{
    private readonly IEventHandler<LogEntryWrittenEvent> logEntryWrittenHandler;
    private readonly ILoggingSource loggingSource;

    public NotificationDecorator(IEventHandler<LogEntryWrittenEvent> logEntryWrittenHandler,
        ILoggingSource loggingSource)
    {
        this.logEntryWrittenHandler = logEntryWrittenHandler;
        this.loggingSource = loggingSource;
    }


    public void WriteEntry(LogEntry entry)
    {
        this.loggingSource.WriteEntry(entry);
        this.logEntryWrittenHandler.Handle(new LogEntryWrittenEvent());
    }

    //With TaskScheduler.Default all handling will be executed on thread pool 
    public Task WriteEntryAsync(LogEntry entry, CancellationToken ct = default)
        => this.loggingSource.WriteEntryAsync(entry, ct)
            .ContinueWith(_ => this.logEntryWrittenHandler.Handle(new LogEntryWrittenEvent()), 
                ct,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
}


