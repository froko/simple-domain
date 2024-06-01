namespace SimpleDomain.TestDoubles;

public class MyEvent : IEvent { }

public class OtherEvent : IEvent { }

public class ValueEvent : IEvent
{
    public ValueEvent(int value) => this.Value = value;

    public int Value { get; }
}