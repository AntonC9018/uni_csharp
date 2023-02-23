using System;

namespace Laborator1;

public interface IItemRandomizer
{
    void Randomize();
}

public interface IItemRandomizer<T> : IItemRandomizer
{
}

public interface IRandomGetter<out T> : IGetter<T>
{
}

// Customizing this requires extracting the constants into separate properties
// on all of the view models, and making new view models responsible for each item
// in the item list. It's good enough as is.
public static class Ranges
{
    public const int Min = 0;
    public const int Max = 100;
}

public sealed class RandomIntGetter : IRandomGetter<int>
{
    private readonly Random _random;

    public RandomIntGetter(Random? random = null)
    {
        _random = random ?? new();
    }

    public int Get()
    {
        return (_random.Next() - Ranges.Min) % (Ranges.Min - Ranges.Max) + Ranges.Min;
    }
}

public sealed class RandomFloatGetter : IRandomGetter<float>
{
    private readonly Random _random;

    public RandomFloatGetter(Random? random = null)
    {
        _random = random ?? new();
    }

    public float Get()
    {
        return (float) _random.NextDouble() * (Ranges.Max - Ranges.Min) + Ranges.Min;
    }
}

public sealed class RandomStringGetter : IRandomGetter<string>
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
    private readonly Random _random;

    public RandomStringGetter(Random? random = null)
    {
        _random = random ?? new();
    }

    public string Get()
    {
        return _Strings[_random.NextInt64(0, _Strings.LongLength)];
    }
}


public sealed class ItemsRandomizer<T> : IItemRandomizer<T>
{
    private readonly ItemsData<T> _items;
    private readonly IRandomGetter<T> _randomGetter;

    public ItemsRandomizer(ItemsData<T> items, IRandomGetter<T> randomGetter)
    {
        _items = items;
        _randomGetter = randomGetter;
    }

    public void Randomize()
    {
        var items = _items.Items;
        for (var i = 0; i < items.Count; i++)
            items[i] = _randomGetter.Get();
    }
}