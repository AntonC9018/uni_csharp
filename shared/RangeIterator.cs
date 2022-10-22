using System;

namespace Shared;

public struct RangeIterator
{
    private int _start;
    private int _end;
    private int _increment;
    private int _current;

    public RangeIterator(int start, int end, int increment)
    {
        _start = start;
        _end = end;
        _increment = increment;
        _current = start - increment;
    }

    public bool MoveNext()
    {
        _current += _increment;
        return _increment > 0
            ? _current < _end
            : _current > _end;
    }

    public readonly int Current => _current;
    public RangeIterator GetEnumerator() => this;
}

public static class IterationHelper
{
    public static RangeIterator Range(int start, int end, int increment = 1)
    {
        return new RangeIterator(start, end, increment);
    } 
}
