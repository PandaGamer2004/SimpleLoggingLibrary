using System.Text;
using Microsoft.Extensions.Options;

namespace LoggingLibrary;


public class FileLoggingSource : ILoggingSource
{
    private readonly IFileOperationsSynchronizer fileOperationsSynchronizer;
    public FileLoggingSource(IOptions<LoggingConfigurationOptions> loggingConfiguration,
                             IFileOperationsSynchronizer fileOperationsSynchronizer)
    {
        this.fileOperationsSynchronizer = fileOperationsSynchronizer;
    }
    //Here implemented wait because if we will implement io operation manually it will be sync and total wait time will be the same 
    public void WriteEntry(LogEntry entry)
        => WriteEntryAsync(entry).Wait();

    public Task WriteEntryAsync(LogEntry entry, CancellationToken ct = default)
        => fileOperationsSynchronizer.ScheduleAsyncFileOperation((fileStream, token) =>
        {
            //Set position of stream to end for append of stream for batch write of data
            fileStream.Seek(0, SeekOrigin.End);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LogLevel: " + entry.LogLevel);
            sb.AppendLine("Error message: " + entry.Message);
            var resultLine = sb.ToString();
            var binaryData = Encoding.UTF8.GetBytes(resultLine);
            return fileStream.WriteAsync(binaryData, 0, binaryData.Length, token);
        }, ct);
}