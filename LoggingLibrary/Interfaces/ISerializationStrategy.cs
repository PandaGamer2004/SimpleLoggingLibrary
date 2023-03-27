namespace LoggingLibrary;
public interface ISerializationStrategy<TLogEntry>
{
    byte[] GetSerializedLogEntry(TLogEntry entry);
}
