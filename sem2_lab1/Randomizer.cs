using System;

namespace Laborator1;

public interface IItemRandomizer
{
    void Randomize();
}

public interface IItemRandomizer<T> : IItemRandomizer
{
}

public abstract class ItemsRandomizerBase<T> : IItemRandomizer<T>
{
    protected readonly ItemsData<T> Items;

    protected ItemsRandomizerBase(ItemsData<T> items)
    {
        Items = items;
    }

    public virtual void Randomize()
    {
        var items = Items.Items;
        for (var i = 0; i < items.Length; i++)
            RandomizeItem(ref items[i], i);
    }

    protected abstract void RandomizeItem(ref T currentItem, int index);
}

public class IntItemsRandomizer : ItemsRandomizerBase<int>
{
    private readonly Random _random = new();

    public IntItemsRandomizer(ItemsData<int> items) : base(items)
    {
    }
    
    protected override void RandomizeItem(ref int currentItem, int index)
    {
        currentItem = _random.Next();
    }
}

public class StringItemsRandomizer : ItemsRandomizerBase<string>
{
    private static readonly string[] _Strings = new[]
    {
        "big",
        "alpha",
        "beta",
        "dinosaur",
        "elephant",
        "fox",
        "giraffe",
        "horse",
        "dog",
        "cat",
        "house",
        "car",
        "plane",
        "train",
        "boat",
        "ship",
        "truck",
        "ticket",
        "Earth",
        "Mars",
        "Saturn",
        "Jupiter",
        "Uranus",
        "Neptune",
        "bitcoin",
        "ethereum",
        "youtube",
        "facebook",
        "instagram",
        "twitter",
        "tiktok",
    };
    private readonly Random _random = new();

    public StringItemsRandomizer(ItemsData<string> items) : base(items)
    {
    }

    protected override void RandomizeItem(ref string currentItem, int index)
    {
        currentItem = _Strings[_random.NextInt64(0, _Strings.LongLength)];
    }
}

public class FloatItemsRandomizer : ItemsRandomizerBase<float>
{
    private readonly Random _random = new();

    public FloatItemsRandomizer(ItemsData<float> items) : base(items)
    {
    }

    protected override void RandomizeItem(ref float currentItem, int index)
    {
        currentItem = (float) _random.NextDouble();
    }
}