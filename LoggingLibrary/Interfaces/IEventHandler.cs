namespace LoggingLibrary;


public interface IEventHandler<TEvent>
    where TEvent : IEvent
{
    public void Handle(TEvent @event);
}

