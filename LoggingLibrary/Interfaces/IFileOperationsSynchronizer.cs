namespace LoggingLibrary;

public interface IFileOperationsSynchronizer
{
    public Task ScheduleAsyncFileOperation(Func<FileStream, CancellationToken, Task> asyncOperation,
        CancellationToken ct);
}