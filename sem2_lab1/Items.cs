using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Shared;

namespace Laborator1;

public interface IItems
{
    void ResizeArray(int newSize);
    System.Type ElementType { get; }
    IList List { get; }
    INotifyCollectionChanged ListAsObservable { get; }
    event Action<int> CountChanged;
}

public interface IShuffle
{
    void Shuffle();
}

public sealed class Shuffler<T> : IShuffle
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
        _random.Shuffle(_items.Items);
    }
}

public sealed class ItemCountObservableValue : IObservableValue<int>
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

public sealed class ItemsData<T> : IItems
{
    public ObservableCollection<T?> Items { get; set; }
    public IGetter<T> NewItemFactory { get; set; }

    public ItemsData(ObservableCollection<T?> items, IGetter<T> newItemFactory)
    {
        Items = items;
        NewItemFactory = newItemFactory;
    }

    public void ResizeArray(int newSize)
    {
        // This is stupid!
        while (Items.Count < newSize)
            Items.Add(NewItemFactory.Get());
        while (Items.Count > newSize)
            Items.RemoveAt(Items.Count);
        
        CountChanged?.Invoke(newSize);
    }

    IList IItems.List => Items;
    INotifyCollectionChanged IItems.ListAsObservable => Items;
    public event Action<int>? CountChanged;
    System.Type IItems.ElementType => typeof(T);
}


public sealed class ItemsStringsFiller
{
    private readonly ObservableCollection<string> _target;
    private readonly Func<object?, string> _factory;
    
    public ItemsStringsFiller(ObservableCollection<string> target, IObservableValue<IItems> items)
    {
        _target = target;
        _factory = (object? value) => value?.ToString() ?? "";
        items.ValueChanged += ResetItems;
        ResetItems(items.Get());
    }

    private void ResetItems(IItems? items)
    {
        _target.Clear();

        if (items is null)
            return;

        items.ListAsObservable.CollectionChanged += _target.WrapCollectionChanged(_factory);
        foreach (var s in items.List)
            _target.Add(_factory(s));
    }

    public static void Apply(ObservableCollection<string> target, IObservableValue<IItems> items)
    {
        new ItemsStringsFiller(target, items);
    }
}