// A / B
using System.Diagnostics;

public static class Helper
{
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
        
        return (r0, Math.Abs(t1), Math.Abs(s1));
    }

    private readonly struct MatrixWithoutRowsCols
    {
        public readonly Matrix Matrix;
        // Maybe use HashSet?
        public readonly List<int> XIndices;
        public readonly List<int> YIndices;

        public MatrixWithoutRowsCols(Matrix matrix, List<int> xIndices, List<int> yIndices)
        {
            Matrix = matrix;
            XIndices = xIndices;
            YIndices = yIndices;
        }

        public MatrixWithoutRowsCols(Matrix matrix)
            : this(matrix, new(), new())
        {
        }
    }

    public static RationalNumber GetDeterminant(Matrix m)
    {
        int size = m.Width;
        Debug.Assert(size == m.Height, "Different dimensions.");

        var vars = new MatrixWithoutRowsCols(m);
        var zToSize = Enumerable.Range(0, size); 
        vars.XIndices.AddRange(zToSize);
        vars.YIndices.AddRange(zToSize);

        return _GetDeterminant(vars);
        
        // if (size == 1)
        //     return m[0, 0];

        // if (size == 2)
        //     return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
    }

    private static RationalNumber _GetDeterminant(in MatrixWithoutRowsCols vars)
    {
        if (vars.XIndices.Count == 1)
            return vars.Matrix[vars.XIndices[0], vars.YIndices[0]];

        if (vars.XIndices.Count == 2)
        {
            int x0 = vars.XIndices[0];
            int x1 = vars.XIndices[1];
            int y0 = vars.YIndices[0];
            int y1 = vars.YIndices[1];
            var m = vars.Matrix;
            return m[x0, y0] * m[x1, y1] - m[x0, y1] * m[x1, y0];
        }

        var result = RationalNumber.One;
        int sign = 1;
        for (int i = 0; i < vars.XIndices.Count; i++)
        {
            int x = vars.XIndices[i];
            vars.XIndices.RemoveAt(i);

            for (int j = 0; j < vars.YIndices.Count; j++)
            {
                int y = vars.XIndices[j];
                vars.XIndices.RemoveAt(j);

                result += sign * _GetDeterminant(vars);
                sign = -sign;

                vars.XIndices.Insert(j, y);
            }

            vars.XIndices.Insert(i, x);
        }
        return result;
    }
}

class Program
{
    static void Main()
    {
        var a = new RationalNumber(1, 2);
        var b = new RationalNumber(6, 4);
        var c0 = a - b;
        var c1 = a + b;
        var c2 = a * b;
        var c3 = a / b;
        Console.WriteLine(c0);
        Console.WriteLine(c1);
        Console.WriteLine(c2);
        Console.WriteLine(c3);
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
    {
        var e = Helper.EGCD(Math.Abs(a), Math.Abs(b));
        Top = e.FirstDiv;
        Bottom = e.SecondDiv;
        Sign = Math.Sign(a) * Math.Sign(b);
    }

    public RationalNumber(int a, int b, int sign)
    {
        Debug.Assert(a >= 0);
        Debug.Assert(b >= 0);
        var e = Helper.EGCD(a, b);
        Top = e.FirstDiv;
        Bottom = e.SecondDiv;
        Sign = sign;
    }

    public static readonly RationalNumber One = new RationalNumber(1);
    public static readonly RationalNumber Zero = new RationalNumber(0);

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
        if (Top == 0)
            return "0";
        else
        {
            string signString = Sign == 1 ? "" : "-";
            if (Bottom == 1)
                return signString + Top.ToString();
            else
                return signString + string.Format("{0}/{1}", Top, Bottom);
        }
    }
}


public readonly record struct V2(int X, int Y)
{
    public static V2 operator+(V2 a, V2 b)
    {
        return new V2(a.X + b.X, a.Y + b.Y);
    }
    public static V2 operator-(V2 a, V2 b)
    {
        return new V2(a.X - b.X, a.Y - b.Y);
    }
};

public readonly struct Matrix
{
    private readonly RationalNumber[,] _underlyingArray;
    private readonly V2 _offset;
    private readonly V2 _size;

    public Matrix(RationalNumber[,] underlyingArray)
    {
        _underlyingArray = underlyingArray;
        _offset = default;
        
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new V2(width, height);
    }

    public Matrix(RationalNumber[,] underlyingArray, V2 offset)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new V2(width - offset.X, height - offset.Y);
    }

    public Matrix(RationalNumber[,] underlyingArray, V2 offset, V2 size)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        _size = size;
    }

    public ref RationalNumber this[V2 pos]
    {
        get
        {
            Debug.Assert(pos.Y < _size.Y && pos.Y > 0);
            Debug.Assert(pos.X < _size.X && pos.X > 0);
            return ref _underlyingArray[(pos.Y + _offset.Y), (pos.X + _offset.X)];
        }
    }

    public ref RationalNumber this[int x, int y]
    {
        get
        {
            return ref this[new V2(x, y)];
        }
    }

    public readonly int Width => Size.X;
    public readonly int Height => Size.Y;
    public readonly V2 Size => _size;

    public readonly Matrix Slice(V2 localOffset)
    {
        Debug.Assert(localOffset.Y < _size.Y && localOffset.Y > 0);
        Debug.Assert(localOffset.X < _size.X && localOffset.X > 0);
        V2 offset = localOffset + _offset;
        V2 size = _size - localOffset;
        return new Matrix(_underlyingArray, offset, size);
    }
}
