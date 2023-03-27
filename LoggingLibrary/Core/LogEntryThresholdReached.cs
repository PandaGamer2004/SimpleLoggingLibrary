namespace LoggingLibrary;

public record LogEntryThresholdReached(int CountOfEntriesWritten): IEvent;