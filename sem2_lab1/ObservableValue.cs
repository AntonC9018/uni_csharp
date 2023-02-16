using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Laborator1;


public interface IProvider<out T>
{
    T? Get();
}

public interface IProperty<out T>
{
    event Action<T?> ValueChanged;
}

public interface ISetter<in T>
{
    void Set(T? value);
}

public interface IObservableValue<out T> : IProvider<T>, IProperty<T>
{
}

public interface IObservableRepo<T> : ISetter<T>, IObservableValue<T>
{
}

public static class ProviderExtensions
{
    public static T GetRequired<T>(this IProvider<T> p)
    {
        var value = p.Get();
        if (value is null)
            throw new InvalidOperationException("Value is null");
        return value;
    }
}

public class ObservableValue<T> : IObservableRepo<T>
{
    private T? _value;
    public T? Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueChanged?.Invoke(value);
        }
    }

    public event Action<T?>? ValueChanged;
    public void Set(T value) => Value = value;
    public T? Get() => Value;
}

public class ValueProvider<T> : IProvider<T>
{
    public T? Value { get; }
    public ValueProvider(T value)
    {
        Value = value;
    }

    public T? Get() => Value;
}