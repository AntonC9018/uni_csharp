using System.Collections.Generic;

namespace Laborator1;


public enum SortDirection
{
    Ascending,
    Descending,
}

public interface IDirectionalComparer
{
    SortDirection Direction { get; set; }
}

public sealed class DirectionalComparerDecorator<T> : IComparer<T>, IDirectionalComparer
{
    public DirectionalComparerDecorator(IComparer<T> comparer)
    {
        Comparer = comparer;
    }

    public IComparer<T> Comparer { get; set; }
    public SortDirection Direction { get; set; }

    public int Compare(T? x, T? y)
    {
        var result = Comparer.Compare(x, y);
        return Direction == SortDirection.Ascending ? result : -result;
    }
}