namespace LoggingLibrary;

public interface ILogReplicationStrategy
{
    public Task RunReplicationAsync(CancellationToken ct = default);
}