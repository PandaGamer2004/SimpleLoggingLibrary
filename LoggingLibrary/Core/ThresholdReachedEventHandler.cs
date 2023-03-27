namespace LoggingLibrary;

public class ThresholdReachedEventHandler : IEventHandler<LogEntryThresholdReached>
{
    private readonly ILogReplicationStrategy logReplicationStrategy;

    public ThresholdReachedEventHandler(ILogReplicationStrategy logReplicationStrategy)
    {
        this.logReplicationStrategy = logReplicationStrategy;
    }
    //Just a simple task will run on same thread or on concurrent thread
    public void Handle(LogEntryThresholdReached @event)
    {
        logReplicationStrategy.RunReplicationAsync();
    }
}
