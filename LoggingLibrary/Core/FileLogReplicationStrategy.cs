using Microsoft.Extensions.Options;

namespace LoggingLibrary;


public class FileLogReplicationStrategy : ILogReplicationStrategy
{
    private readonly IFileOperationsSynchronizer fileOperationsSynchronizer;
    private int currentReplicationOrder = 1;

    private readonly string outputDirectoryPath;
    public FileLogReplicationStrategy(IOptions<LoggingConfigurationOptions> loggingReplicationOptions,
                                      IFileOperationsSynchronizer fileOperationsSynchronizer)
    {
        this.fileOperationsSynchronizer = fileOperationsSynchronizer;
        this.outputDirectoryPath = loggingReplicationOptions.Value.LogsOuputFilePath;
    }

    private string GenerateReplicationFilePath()
    {
        var nextFileName = $"{DateTime.Now:dd:MM:yyyy}_{currentReplicationOrder}";
        return Path.Combine(outputDirectoryPath, nextFileName);
    }

    public Task RunReplicationAsync(CancellationToken ct = default)
        => this.fileOperationsSynchronizer.ScheduleAsyncFileOperation((logFileStream, ct) =>
        {
            var replicationFilePath = GenerateReplicationFilePath();
            using FileStream replicationFileStream = File.Create(replicationFilePath);
            //here used continuation to see that updates is applied
            return logFileStream.CopyToAsync(replicationFileStream, ct)
                .ContinueWith(_ => replicationFileStream.Close(), ct);
        }, ct);
    
    
    
}