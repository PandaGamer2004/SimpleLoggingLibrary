namespace LoggingLibrary;

public interface ILogFileStreamFactory
{ 
    public FileStream CreateLoggingFileStream();
}