namespace Laborator2;

using System.Collections;
using System.Diagnostics;
using System.Numerics;

public static class BitHelper
{
    public static SetBitIndicesIterator GetSetBitIndices(this uint i)
    {
        return new SetBitIndicesIterator(i);
    }
}

public struct BitArray32
{
    private uint _bits;
    private int _length;

    private BitArray32(uint bits, int length)
    {
        Debug.Assert(length <= sizeof(uint) * 8);
        _bits = bits;
        _length = length;
    }

    public void Set(int index)
    {
        Debug.Assert(index < _length);
        _bits |= (1u << index);
    }

    public readonly BitArray32 WithSet(int index)
    {
        var r = this;
        r.Set(index);
        return r;
    }

    public void Clear(int index)
    {
        Debug.Assert(index < _length);
        _bits &= ~(1u << index);
    }

    public readonly BitArray32 WithClear(int index)
    {
        var r = this;
        r.Clear(index);
        return r;
    }

    public readonly SetBitIndicesIterator GetEnumerator() => BitHelper.GetSetBitIndices(_bits);
    public readonly SetBitIndicesIterator SetBitIndices => GetEnumerator();
    
    public static BitArray32 AllSet(int length)
    {
        return new BitArray32(~default(uint) >> (sizeof(uint) * 8 - length), length);
    }

    public static BitArray32 Empty(int length)
    {
        return new BitArray32(default, length);
    }

    public static bool CanCreate(int length)
    {
        return length <= MaxLength;
    }

    public const int MaxLength = sizeof(uint) * 8;
}


public struct SetBitIndicesIterator : IEnumerable<int>, IEnumerator<int>
{
    private uint _bits;
    private int _current;

    public SetBitIndicesIterator(uint bits)
    {
        _bits = bits;
        _current = default;
    }

    public bool MoveNext()
    {
        if (_bits == 0)
            return false;
        _current = (sizeof(uint) * 8) - BitOperations.LeadingZeroCount(_bits) - 1;
        _bits &= ~(1u << _current);
        return true;
    }

    public readonly int Current => _current;


    public readonly SetBitIndicesIterator GetEnumerator() => this;

    readonly IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    readonly object IEnumerator.Current => Current;

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}