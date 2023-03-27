using Microsoft.Extensions.Options;

namespace LoggingLibrary;

public class LogEntryWrittenCollectionHandler: IEventHandler<LogEntryWrittenEvent>, IDisposable
{
    private int countOfEntriesWritten = 0;
    
    private int thresholdWrites = 0;

    private readonly IDisposable configurationTracker;
        
    private IEventHandler<LogEntryThresholdReached> threasholdReachedHandler;
    public LogEntryWrittenCollectionHandler(IEventHandler<LogEntryThresholdReached> threasholdReachedHandler,
        IOptionsMonitor<LoggingEntriesThresholdOptions> thresholdEventNumberOptions)
    {
        IDisposable? changeTracker = thresholdEventNumberOptions.OnChange(_ =>
        {
            //in theory this updates couldn't be fired on concurrent threads so here synchronization not important
            this.thresholdWrites = thresholdEventNumberOptions.CurrentValue.EntriesCount;
        });
        this.configurationTracker = changeTracker ?? throw new LoggingConfigurationException(LoggingErrorMessages.LoggingThresholdNotConfigured);
        this.threasholdReachedHandler = threasholdReachedHandler;
    }
    public void Handle(LogEntryWrittenEvent @event)
    {
        int countOfEvent = Interlocked.Increment(ref countOfEntriesWritten);
        if (countOfEvent % thresholdWrites == 0)
        {
            this.threasholdReachedHandler.Handle(new LogEntryThresholdReached(this.thresholdWrites));
        }
    }

    public void Dispose()
        => this.configurationTracker.Dispose();
}

