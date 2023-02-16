using System;
using System.Collections;
using System.Collections.Generic;
using Shared;

namespace Laborator1;

public interface IItems
{
    void ResizeArray(int newSize);
    System.Type ElementType { get; }
    IList List { get; }
    event Action<int> CountChanged;
}

public interface IShuffle
{
    void Shuffle();
}

public class Shuffler<T> : IShuffle
{
    private readonly ItemsData<T> _items;
    private readonly Random _random;
    
    public Shuffler(ItemsData<T> items)
    {
        _items = items;
        _random = Random.Shared;
    }
    
    public void Shuffle()
    {
        _random.Shuffle(_items.Items.AsSpan());
    }
}

public class ItemCountObservableValue : IObservableValue<int>
{
    private IItems? _items;

    public ItemCountObservableValue(IObservableValue<IItems> items)
    {
        items.ValueChanged += OnItemsChanged;
        OnItemsChanged(items.Get());
    }

    private void OnItemsChanged(IItems? items)
    {
        _items = items;
        if (items is not null)
            items.CountChanged += OnCountChanged;
    }

    private void OnCountChanged(int newValue)
    {
        ValueChanged?.Invoke(newValue);
    }

    public int Value => _items?.List.Count ?? 0;
    public event Action<int>? ValueChanged;
    public int Get() => Value;
}

public class ItemsData<T> : IItems
{
    public T[] Items { get; set; }
    public IComparer<T> Comparer { get; set; }

    public ItemsData(T[] items, IComparer<T> comparer)
    {
        Items = items;
        Comparer = comparer;
    }

    public void ResizeArray(int newSize)
    {
        var it = Items;
        Array.Resize(ref it, newSize);
        Items = it;
        CountChanged?.Invoke(newSize);
    }

    IList IItems.List => Items;
    public event Action<int>? CountChanged;
    System.Type IItems.ElementType => typeof(T);
}