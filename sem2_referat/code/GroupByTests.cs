using Xunit;

namespace code;

public class GroupByTests
{
    private record Record(int Id, string Name, float Value);

    private static readonly Record[] _Records = new[]
    {
        new Record(1, "A", 1.0f),
        new Record(2, "A", 1.0f),
        new Record(3, "A", 2.0f),
        new Record(4, "A", 2.0f),
        new Record(5, "B", 1.0f),
        new Record(6, "B", 1.0f),
        new Record(7, "B", 2.0f),
        new Record(8, "B", 2.0f),
    };
    
    [Fact]
    public void GroupBy_IsPure()
    {
        var recordsCopy = _Records.ToArray();
        foreach (var _ in _Records.GroupBy(r => r.Name)) {}
        Assert.Equal(recordsCopy, _Records);
    }
    
    [Fact]
    public void GroupBy_WithSimpleKey_DefaultComparison()
    {
        var groups = _Records
            .GroupBy(r => r.Name)
            .ToArray();
        
        // GroupBy groups are not supposed to be sorted by key
        // Internally, they are stored in a hash map (called Lookup in the source code)
        // that puts the buckets with the same hash next to each other,
        // as opposed to a dictionary, which puts them in a linked list.
        Array.Sort(groups, (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
        
        Assert.Equal(2, groups.Length);
        Assert.Equal("A", groups[0].Key);
        Assert.Equal("B", groups[1].Key);
        Assert.Equal(4, groups[0].Count());
        Assert.Equal(4, groups[1].Count());
        
        // The items are stored in order of their appearance in the source
        Assert.Equal(Enumerable.Range(1, 4), groups[0].Select(x => x.Id));
        Assert.Equal(Enumerable.Range(5, 4), groups[1].Select(x => x.Id));
    }

    [Fact]
    public void GroupBy_ComplexKey_DefaultComparison()
    {
        var groups = _Records
            .GroupBy(r => new { Name = r.Name, Value = r.Value })
            .ToArray();
        
        Assert.Equal(_Records.Take(2), 
            groups.Single(g => g.Key is { Name: "A", Value: 1.0f }));
        Assert.Equal(_Records.Skip(2).Take(2), 
            groups.Single(g => g.Key is { Name: "A", Value: 2.0f }));
        Assert.Equal(_Records.Skip(4).Take(2), 
            groups.Single(g => g.Key is { Name: "B", Value: 1.0f }));
        Assert.Equal(_Records.Skip(6).Take(2), 
            groups.Single(g => g.Key is { Name: "B", Value: 2.0f }));
    }

    [Fact]
    public void GroupBy_ComplexKeyOtherThanAnonymousObject()
    {
        var groups = _Records
            .GroupBy(r => (r.Name, r.Value))
            .ToArray();
        
        Assert.Equal(_Records.Take(2), 
            groups.Single(g => g.Key is { Name: "A", Value: 1.0f }));
        Assert.Equal(_Records.Skip(2).Take(2), 
            groups.Single(g => g.Key is { Name: "A", Value: 2.0f }));
        Assert.Equal(_Records.Skip(4).Take(2), 
            groups.Single(g => g.Key is { Name: "B", Value: 1.0f }));
        Assert.Equal(_Records.Skip(6).Take(2), 
            groups.Single(g => g.Key is { Name: "B", Value: 2.0f }));
    }


    private class MyKeyType
    {
        internal readonly int _nameLength;
        
        public MyKeyType(string name)
        {
            _nameLength = name.Length;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is MyKeyType other && _nameLength == other._nameLength;
        }

        protected bool Equals(MyKeyType other)
        {
            return _nameLength == other._nameLength;
        }

        public override int GetHashCode()
        {
            return _nameLength;
        }
    }
    
    [Fact]
    public void GroupBy_KeyThatImplementsDifferentEquals()
    {
        var groups = _Records
            .GroupBy(r => new MyKeyType(r.Name))
            .ToArray();
        
        Assert.Single(groups);
    }

    private record NameValue(string Name, float Value);
    
    private class KeyComparer : IEqualityComparer<NameValue>
    {
        public bool Equals(NameValue? x, NameValue? y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;
            
            return x.Name.Length == y.Name.Length 
                   && Math.Abs(x.Value - y.Value) < 0.1f;
        }

        public int GetHashCode(NameValue obj)
        {
            return HashCode.Combine(obj.Name.Length, obj.Value);
        }
    }
    
    [Fact]
    public void GroupBy_WithCustomComparerClass()
    {
        var groups = _Records
            .GroupBy(r => new NameValue(r.Name, r.Value), new KeyComparer())
            .ToArray();
        
        Assert.Equal(2, groups.Length);
        
        // 1.0f, 2.0f
        Array.Sort(groups, (a, b) => a.Key.Value.CompareTo(b.Key.Value));
        
        var group1 = _Records.Take(2).Concat(_Records.Skip(4).Take(2));
        Assert.Equal(group1, groups[0]);
        
        var group2 = _Records.Skip(2).Take(2).Concat(_Records.Skip(6).Take(2));
        Assert.Equal(group2, groups[1]);
    }

    [Fact]
    public void Lookup_IsTheSameThinsAsGroupBy()
    {
        // In fact, GetEnumerator implementation of GroupBy looks like this:
        /*
            public IEnumerator<IGrouping<TKey, TSource>> GetEnumerator() =>
                Lookup<TKey, TSource>.Create(_source, _keySelector, _comparer).GetEnumerator(); 
        */
        var lookup = _Records.ToLookup(x => new { x.Name });
        
        Assert.Equal(_Records.Take(4), lookup[new { Name = "A" }]);
        Assert.Equal(_Records.Skip(4).Take(4), lookup[new { Name = "B" }]);
    }
}