using System.Collections;
using System.Collections.Generic;

namespace Laborator1;

// This required a custom impl for the IList type.
// .NET doesn't have an array segment for IList. 
public readonly struct ListSegment<T> : IEnumerable<T>
{
    public readonly IList<T> List;
    public readonly int StartIndex;
    public readonly int SpanLength;
    
    public ListSegment(IList<T> list, int startIndex, int spanLength)
    {
        List = list;
        StartIndex = startIndex;
        SpanLength = spanLength;
    }
    
    public ListSegment(IList<T> list)
    {
        List = list;
        StartIndex = 0;
        SpanLength = list.Count;
    }
    
    public int Length => SpanLength;
    
    public ListSegment<T> Slice(int startIndex, int spanLength)
    {
        return new ListSegment<T>(List, StartIndex + startIndex, spanLength);
    }
    
    public T this[int index]
    {
        get => List[StartIndex + index];
        set => List[StartIndex + index] = value;
    }
    
    public void Swap(int index0, int index1)
    {
        (this[index0], this[index1]) = (this[index1], this[index0]);
    }
    
    // iterator
    public ListSpanEnumerator GetEnumerator() => new ListSpanEnumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ListSpanEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new ListSpanEnumerator(this);

    public struct ListSpanEnumerator : IEnumerator<T>
    {
        private readonly ListSegment<T> _segment;
        private int _index;

        public ListSpanEnumerator(ListSegment<T> segment)
        {
            _segment = segment;
            _index = 0;
        }

        public void Reset()
        {
            _index = 0;
        }

        public readonly T Current => _segment[_index];

        public bool MoveNext()
        {
            _index++;
            return _index < _segment.SpanLength;
        }

        object? IEnumerator.Current => Current;
        public void Dispose()
        {
        }
    }
}