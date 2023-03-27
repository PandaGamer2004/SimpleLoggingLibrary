namespace LoggingLibrary;

public class ReplicationOptions
{
    public const string Replication = "Replication";

    public int LogEntriesCountThresholdToRunReplication { get; set; }
}