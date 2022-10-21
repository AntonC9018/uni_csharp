using System.Diagnostics;
using static IterationHelper;

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

class Program
{
    static void Main()
    {
        var a = new RationalNumber(1, 2);
        var b = new RationalNumber(-6, 4);
        var c0 = a - b;
        var c1 = a + b;
        var c2 = a * b;
        var c3 = a / b;
        Console.WriteLine(c0);
        Console.WriteLine(c1);
        Console.WriteLine(c2);
        Console.WriteLine(c3);

        {
            var arr = new RationalNumber[3, 3]
            {
                { 3,  1,  1 },
                { 4, -2,  5 },
                { 2,  8,  7 },
            };
            var matrix = new Matrix(arr);

            var det = Helper.GetDeterminant(matrix);
            Console.WriteLine(det.ToString());
        }

        {
            var arr = new RationalNumber[2, 3]
            {
                { 1,  1,  5 },
                { 2, -3, -4 },
            };
            var r = Helper.SolveCramer(arr);
            switch (r.SolutionsKind)
            {
                case Helper.CramerSolutionKind.InfinitelyManySolutions:
                case Helper.CramerSolutionKind.NoSolution:
                {
                    Console.WriteLine("Something went wrong");
                    break;
                }
                case Helper.CramerSolutionKind.Solution:
                {
                    Console.WriteLine(string.Join(", ", r.Solutions));
                    break;
                }
            }
        }


        // foreach (int y in Range(0, matrix.Height))
        // {
        //     foreach (int x in Range(0, matrix.Width))
        //     {
        //         var t = matrix[x, y];
        //         Console.Write(t.ToString().PadLeft(5));
        //     }
        //     Console.WriteLine();
        // }
    }
}

public static class Helper
{
    public static int GCD(int a, int b)
    {
        (int r0, int r1) = (a, b);
        while (r1 != 0)
        {
            int q = r0 / r1;
            (r0, r1) = (r1, r0 - q * r1);
        }
        return r0;
    }

    public static (int GCD, int FirstDiv, int SecondDiv) EGCD(int a, int b)
    {
        (int r0, int r1) = (a, b);
        (int s0, int s1) = (1, 0);
        (int t0, int t1) = (0, 1);

        while (r1 != 0)
        {
            int q = r0 / r1;

            (r0, r1) = (r1, r0 - q * r1);
            (t0, t1) = (t1, t0 - q * t1);
            (s0, s1) = (s1, s0 - q * s1);
        }
        
        return (r0, t1, s1);
    }

    private struct MatrixWithoutRowsCols
    {
        public readonly Matrix Matrix;
        // Maybe use HashSet?
        public BitArray32 XIndices;
        public BitArray32 YIndices;

        public MatrixWithoutRowsCols(Matrix matrix)
        {
            Matrix = matrix;
            XIndices = BitArray32.AllSet(length: matrix.Width);
            YIndices = XIndices;
        }
    }

    public static RationalNumber GetDeterminant(Matrix m)
    {
        int size = m.Width;
        Debug.Assert(size == m.Height, "Different dimensions.");

        var vars = new MatrixWithoutRowsCols(m);
        return _GetDeterminant(ref vars);
    }

    private static RationalNumber _GetDeterminant(ref MatrixWithoutRowsCols vars)
    {
        var xs = vars.XIndices.SetBitIndices;
        var ys = vars.YIndices.SetBitIndices;

        Debug.Assert(xs.MoveNext());
        Debug.Assert(ys.MoveNext());

        int x0 = xs.Current;
        if (!xs.MoveNext())
            return vars.Matrix[x0, ys.Current];

        var result = RationalNumber.Zero;
        int sign = 1;
        {
            vars.XIndices.Clear(x0);

            foreach (int j in vars.YIndices.SetBitIndices)
            {
                vars.YIndices.Clear(j);

                sign = -sign;
                result += sign * vars.Matrix[x0, j] * _GetDeterminant(ref vars);

                vars.YIndices.Set(j);
            }

            vars.XIndices.Set(x0);
        }
        return result;
    }

    public enum CramerSolutionKind
    {
        Solution,
        NoSolution,
        InfinitelyManySolutions,
    }

    public static CramerSolutionKind SolveCramer(Matrix matrix, Span<RationalNumber> outResult)
    {
        var size = matrix.Size with { X = matrix.Width - 1 };
        Debug.Assert(size.X == size.Y, "The last column must be the equality column");
        var rhs = new RationalNumber[size.Y];
        var lhs = matrix.Slice(default, size);
        for (int i = 0; i < size.Y; i++)
            rhs[i] = matrix[matrix.Width - 1, i];
        return _SolveCramer(ref lhs, outResult, rhs);
    }

    public static (CramerSolutionKind SolutionsKind, RationalNumber[] Solutions) SolveCramer(Matrix matrix)
    {
        var result = new RationalNumber[matrix.Height];
        return (SolveCramer(matrix, result), result);
    }

    private static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    private static CramerSolutionKind _SolveCramer(
        ref Matrix m,
        Span<RationalNumber> outResult,
        Span<RationalNumber> rhs)
    {
        var det = GetDeterminant(m);
        for (int i = 0; i < m.Width; i++)
        {
            for (int j = 0; j < m.Height; j++)
                Swap(ref m[i, j], ref rhs[j]);

            var deti = GetDeterminant(m);
            if (det == 0)
            {
                if (deti != 0)
                    return CramerSolutionKind.InfinitelyManySolutions;
            }
            else
            {
                outResult[i] = deti / det;
            }

            for (int j = 0; j < m.Height; j++)
                Swap(ref m[i, j], ref rhs[j]);
        }

        if (det == 0)
            return CramerSolutionKind.NoSolution;
        
        return CramerSolutionKind.Solution;
    }
}

public struct RationalNumber
{
    public int Sign;
    public int Top;
    public int Bottom;

    public RationalNumber(int a)
    {
        Top = Math.Abs(a);
        Sign = Math.Sign(a);
        Bottom = 1;
    }

    public RationalNumber(int a, int b)
        : this(Math.Abs(a), Math.Abs(b), Math.Sign(a) * Math.Sign(b))
    {
    }

    public RationalNumber(int a, int b, int sign)
    {
        Debug.Assert(a >= 0);
        Debug.Assert(b >= 0);
        var gcd = Helper.GCD(a, b);
        Top = a / gcd;
        Bottom = b / gcd;
        Sign = sign;
    }

    public static readonly RationalNumber One = new RationalNumber(1);
    public static readonly RationalNumber Zero = new RationalNumber(0);
    public static RationalNumber Invalid => default;
    public static RationalNumber PositiveInfinity => new()
    {
        Sign = 0,
        Top = 1,
        Bottom = 0,
    };
    public static RationalNumber NegativeInfinity => new()
    {
        Sign = 1,
        Top = 1,
        Bottom = 0,
    };

    public static implicit operator RationalNumber(int i) => new RationalNumber(i);
    public static RationalNumber operator +(RationalNumber a, RationalNumber b)
    {
        int top = a.Sign * a.Top * b.Bottom + b.Sign * b.Top * a.Bottom;
        int bottom = a.Bottom * b.Bottom;
        return new RationalNumber(top, bottom);
    }
    public static RationalNumber operator -(RationalNumber a, RationalNumber b)
    {
        return a + -b;
    }
    public static RationalNumber operator *(RationalNumber a, RationalNumber b)
    {
        int top = a.Top * b.Top;
        int bottom = a.Bottom * b.Bottom;
        return new RationalNumber(top, bottom, a.Sign * b.Sign);
    }
    public static RationalNumber operator /(RationalNumber a, RationalNumber b)
    {
        int top = a.Top * b.Bottom;
        int bottom = a.Bottom * b.Top;
        return new RationalNumber(top, bottom, a.Sign * b.Sign);
    }
    public static RationalNumber operator -(RationalNumber a)
    {
        a.Sign = -a.Sign;
        return a;
    }
    public readonly override string ToString()
    {
        string signString = Sign == 1 ? "" : "-";
        if (Top == 0)
        {
            if (Bottom != 0)
                return "0";
            else
                return signString + "inf";
        }
        else
        {
            if (Bottom == 1)
                return signString + Top.ToString();
            else
                return signString + string.Format("{0}/{1}", Top, Bottom);
        }
    }

    public static bool operator==(RationalNumber a, RationalNumber b)
    {
        // infinity
        if (a.Bottom == 0 && a.Bottom == 0)
            return a.Sign == b.Sign;
        // zero
        if (a.Top == 0 && b.Top == 0)
            return true;
        return a.Top == b.Top
            && a.Bottom == b.Bottom
            && b.Sign == a.Sign;
    }
    public static bool operator!=(RationalNumber a, RationalNumber b) => !(a == b);
    public readonly override bool Equals(object? a)
    {
        if (a is not RationalNumber b)
            return false;
        return this == b;
    }
    public readonly override int GetHashCode()
    {
        return base.GetHashCode();
    }
}


public readonly record struct IntV2(int X, int Y)
{
    public static IntV2 operator+(IntV2 a, IntV2 b)
    {
        return new IntV2(a.X + b.X, a.Y + b.Y);
    }
    public static IntV2 operator-(IntV2 a, IntV2 b)
    {
        return new IntV2(a.X - b.X, a.Y - b.Y);
    }
};

public readonly struct Matrix
{
    private readonly RationalNumber[,] _underlyingArray;
    private readonly IntV2 _offset;
    private readonly IntV2 _size;

    public Matrix(RationalNumber[,] underlyingArray)
    {
        _underlyingArray = underlyingArray;
        _offset = default;
        
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new IntV2(width, height);
    }

    public Matrix(RationalNumber[,] underlyingArray, IntV2 offset)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new IntV2(width - offset.X, height - offset.Y);
    }

    public Matrix(RationalNumber[,] underlyingArray, IntV2 offset, IntV2 size)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        _size = size;
    }

    public ref RationalNumber this[IntV2 pos]
    {
        get
        {
            Debug.Assert(pos.Y < _size.Y && pos.Y >= 0);
            Debug.Assert(pos.X < _size.X && pos.X >= 0);
            return ref _underlyingArray[(pos.Y + _offset.Y), (pos.X + _offset.X)];
        }
    }

    public ref RationalNumber this[int i, int j]
    {
        get
        {
            return ref this[new IntV2(i, j)];
        }
    }

    public readonly int Width => Size.X;
    public readonly int Height => Size.Y;
    public readonly IntV2 Size => _size;

    public readonly Matrix Slice(IntV2 localOffset)
    {
        return Slice(localOffset, _size - localOffset);
    }

    public readonly Matrix Slice(IntV2 localOffset, IntV2 newSize)
    {
        Debug.Assert(localOffset.Y < _size.Y && localOffset.Y >= 0);
        Debug.Assert(localOffset.X < _size.X && localOffset.X >= 0);
        Debug.Assert(newSize.Y <= _size.Y && newSize.Y >= 0);
        Debug.Assert(newSize.X <= _size.X && newSize.X >= 0);
        IntV2 offset = localOffset + _offset;
        IntV2 size = newSize;
        return new Matrix(_underlyingArray, offset, size);
    }

    public static implicit operator Matrix(RationalNumber[,] m) => new Matrix(m);
}
