using System;

namespace Laborator1;

public interface IGetter<out T>
{
    T? Get();
}

public interface IChangedEvent<out T>
{
    event Action<T?> ValueChanged;
}

public interface IChangingEvent<out T>
{
    event Action<T?, T?> ValueChanging;
}

public interface ISetter<in T>
{
    void Set(T? value);
}

public interface IObservableValue<out T> : IGetter<T>, IChangedEvent<T>, IChangingEvent<T>
{
}

public interface IObservableRepo<T> : ISetter<T>, IObservableValue<T>
{
}

public static class ProviderExtensions
{
    public static T GetRequired<T>(this IGetter<T> p)
    {
        var value = p.Get();
        if (value is null)
            throw new InvalidOperationException("Value is null");
        return value;
    }
}

public sealed class ObservableValue<T> : IObservableRepo<T>
{
    private T? _value;
    public T? Value
    {
        get => _value;
        set
        {
            ValueChanging?.Invoke(_value, value);
            _value = value;
            ValueChanged?.Invoke(value);
        }
    }

    public event Action<T?>? ValueChanged;
    public void Set(T? value) => Value = value;
    public T? Get() => Value;
    public event Action<T?, T?>? ValueChanging;
}

public sealed class ValueGetter<T> : IGetter<T>
{
    public T? Value { get; }
    public ValueGetter(T value)
    {
        Value = value;
    }

    public T? Get() => Value;
}