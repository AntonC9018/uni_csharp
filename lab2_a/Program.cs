namespace Laborator2;

using System.Diagnostics;
using static Shared.IterationHelper;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        int matrixSize;
        {
            int? matrixSizeInput = Helper.ReadMatrixSize("Enter the size of the matrix: ");
            if (matrixSizeInput is null)
                return;
            matrixSize = matrixSizeInput.Value;
        }
        var arr = new RationalNumber[matrixSize, matrixSize];
        var equationsRhs = new RationalNumber[matrixSize];

        {
            var initKind = InputHelper.ReadInitializationKind();
            switch (initKind)
            {
                case null:
                    return;
                case ArrayInitializationKind.Console:
                {
                    foreach (var (x, y) in Helper.SquareMatrixIndices(matrixSize))
                    {
                        var input = Helper.ReadRationalNumber($"arr[{x}, {y}] = ");
                        if (input is null)
                            return;
                        arr[x, y] = input.Value;
                    }
                    foreach (var y in Range(0, matrixSize))
                    {
                        var input = Helper.ReadRationalNumber($"RHS of row[{y}] = ");
                        if (input is null)
                            return;
                        equationsRhs[y] = input.Value;
                    }
                    break;
                }
                case ArrayInitializationKind.Random:
                {
                    var rng = new Random(69_69_55);
                    RationalNumber GetRandom()
                    {
                        return new RationalNumber(
                            rng.Next() % 5,
                            rng.Next() % 10 + 1,
                            sign: rng.NextDouble() > 0.5 ? 1 : -1
                        );
                    }

                    foreach (var (x, y) in Helper.SquareMatrixIndices(matrixSize))
                        arr[x, y] = GetRandom();

                    foreach (var y in Range(0, matrixSize))
                        equationsRhs[y] = GetRandom();
                    
                    break;
                }
            }
        }

        var matrix = new Slice2(arr);
        var stringCache = matrix.GetStringCache();
        stringCache.WriteToConsole();

        Console.WriteLine($"RHS = " + string.Join(", ", equationsRhs));
        Console.WriteLine($"The determinant is {matrix.GetDeterminant()}");

        var solutions = new RationalNumber[matrixSize];
        var solutionKind = matrix.SolveCramer_SeparateRhs(equationsRhs, solutions);

        switch (solutionKind)
        {
            case Helper.CramerSolutionKind.InfinitelyManySolutions:
            {
                Console.WriteLine("There are infinitely many solutions");
                break;
            }
            case Helper.CramerSolutionKind.NoSolution:
            {
                Console.WriteLine("There are no solutions");
                break;
            }
            case Helper.CramerSolutionKind.Solution:
            {
                Console.WriteLine("Solutions: " + string.Join(", ", solutions));
                break;
            }
        }
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

    public static long GCD(long a, long b)
    {
        (long r0, long r1) = (a, b);
        while (r1 != 0)
        {
            long q = r0 / r1;
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

    private struct ColsRowsMasks
    {
        public int XLargest;
        public BitArray32 YIndices;

        public ColsRowsMasks(in Slice2 matrix)
        {
            XLargest = matrix.Width - 1;
            YIndices = BitArray32.AllSet(length: matrix.Height);
        }
    }

    public static RationalNumber GetDeterminant(this in Slice2 matrix)
    {
        int size = matrix.Width;
        Debug.Assert(size == matrix.Height, "Different dimensions.");

        var vars = new ColsRowsMasks(matrix);
        return _GetDeterminant(matrix, vars);
    }

    private static RationalNumber _GetDeterminant(in Slice2 matrix, ColsRowsMasks masks)
    {
        var ys = masks.YIndices.SetBitIndices;
        var x0 = masks.XLargest;

        Debug.Assert(ys.MoveNext());

        if (x0 == 0)
            return matrix[x0, ys.Current];

        var result = RationalNumber.Zero;
        int sign = 1;
        {
            masks.XLargest--;
            foreach (int y in masks.YIndices.SetBitIndices)
            {
                var ybefore = masks.YIndices;
                masks.YIndices.Clear(y);

                sign = -sign;
                result += sign * matrix[x0, y] * _GetDeterminant(matrix, masks);

                masks.YIndices = ybefore;
            }
        }
        return result;
    }

    public enum CramerSolutionKind
    {
        Solution,
        NoSolution,
        InfinitelyManySolutions,
    }

    public static CramerSolutionKind SolveCramer(this in Slice2 matrix, Span<RationalNumber> outResult)
    {
        var size = matrix.Size with { X = matrix.Width - 1 };
        Debug.Assert(size.X == size.Y, "The last column must be the equality column");
        var rhs = new RationalNumber[size.Y];
        var lhs = matrix.Slice(default, size);
        for (int i = 0; i < size.Y; i++)
            rhs[i] = matrix[matrix.Width - 1, i];
        return _SolveCramer(lhs, rhs, outResult);
    }

    public static (CramerSolutionKind SolutionsKind, RationalNumber[] Solutions) SolveCramer(this in Slice2 matrix)
    {
        var result = new RationalNumber[matrix.Height];
        return (SolveCramer(matrix, result), result);
    }

    public static CramerSolutionKind SolveCramer_SeparateRhs(
        this in Slice2 matrix,
        Span<RationalNumber> rhs,
        Span<RationalNumber> outResult)
    {
        var size = matrix.Size;
        Debug.Assert(size.X == size.Y);
        Debug.Assert(rhs.Length == size.X);
        Debug.Assert(outResult.Length == size.X);
        return _SolveCramer(matrix, rhs, outResult);
    }

    private static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    private static CramerSolutionKind _SolveCramer(
        in Slice2 m,
        Span<RationalNumber> rhs,
        Span<RationalNumber> outResult)
    {
        var det = GetDeterminant(m);
        for (int i = 0; i < m.Width; i++)
        {
            for (int j = 0; j < m.Height; j++)
                Swap(ref m[i, j], ref rhs[j]);

            outResult[i] = GetDeterminant(m);
            
            for (int j = 0; j < m.Height; j++)
                Swap(ref m[i, j], ref rhs[j]);
        }

        if (det == 0)
        {
            foreach (var deti in outResult)
            {
                if (deti != 0)
                    return CramerSolutionKind.NoSolution;
            }
            return CramerSolutionKind.InfinitelyManySolutions;
        }

        foreach (ref var deti in outResult)
            deti /= det;
        
        return CramerSolutionKind.Solution;
    }

    public static IEnumerable<IntV2> SquareMatrixIndices(int matrixSize)
    {
        foreach (var y in Range(0, matrixSize))
        foreach (var x in Range(0, matrixSize))
        {
            yield return new IntV2(x, y);
        }
    }

    public readonly struct MatrixStringCache
    {
        public readonly IntV2 Size;
        public readonly string[] Strings;

        public MatrixStringCache(in Slice2 m)
            : this(m.Size)
        {
        }

        public MatrixStringCache(IntV2 size)
        {
            Size = size;
            Strings = new string[size.X * size.Y];
        }
    }

    public static MatrixStringCache GetStringCache(this in Slice2 m)
    {
        var c = new MatrixStringCache(m);
        SetPaddedStrings(c, m);
        return c;
    }

    public static void SetPaddedStrings(this in MatrixStringCache cache, in Slice2 m)
    {
        Debug.Assert(cache.Strings.Length == m.NumItems);

        int index = 0;
        foreach (var y in Range(0, m.Height))
        foreach (var x in Range(0, m.Width))
            cache.Strings[index++] = m[x, y].ToString();

        int cellWidth = cache.Strings.Max(s => s.Length) + 1;
        foreach (ref string s in cache.Strings.AsSpan())
            s = s.PadLeft(cellWidth);
    }

    public static void WriteToConsole(this in MatrixStringCache cache)
    {
        int index = 0;
        foreach (var y in Range(0, cache.Size.Y))
        {
            foreach (var x in Range(0, cache.Size.X))
                Console.Write(cache.Strings[index++]);
            Console.WriteLine();
        }
    }

    public static RationalNumber? ReadRationalNumber(string message)
    {
        while (true)
        {
            Console.Write(message);
            string? input = Console.ReadLine();
            if (input is null || input == "q")
                return null;
            
            if (double.TryParse(input, out double d))
            {
                var result = RationalNumber.FromDouble(d, precision: 1_000);
                return result;
            }
            
            const string divisionDelimiter = "/";
            int indexOfDelimiter = input.IndexOf(divisionDelimiter);
            if (indexOfDelimiter == -1)
            {
                Console.WriteLine(input + " is not a number");
                continue;
            }

            if (input.IndexOf(divisionDelimiter, indexOfDelimiter + 1) != -1)
            {
                Console.WriteLine("The rational number must only include a single " + divisionDelimiter);
                continue;
            }

            var topString = input.AsSpan(0, indexOfDelimiter);
            if (!int.TryParse(topString, out int topNum))
            {
                Console.WriteLine(string.Concat(topString, " must be an integer."));
                continue;
            }
            var bottomString = input.AsSpan(indexOfDelimiter + divisionDelimiter.Length);
            if (!int.TryParse(bottomString, out int bottomNum))
            {
                Console.WriteLine(string.Concat(bottomString, " must be an integer."));
                continue;
            }

            return new RationalNumber(topNum, bottomNum);
        }
    }

    public static readonly InputHelper.IInputConstraint<int>[] _SquareMatrixInputConstraints =
    {
        InputHelper.PositiveNumberConstraint.Instance,
        new InputHelper.UpperBoundConstraint(BitArray32.MaxLength),
    };

    public static int? ReadMatrixSize(string message)
    {
        return InputHelper.GetNumberWithConstraints(message, _SquareMatrixInputConstraints);
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

public readonly struct Slice2
{
    private readonly RationalNumber[,] _underlyingArray;
    private readonly IntV2 _offset;
    private readonly IntV2 _size;

    public Slice2(RationalNumber[,] underlyingArray)
    {
        _underlyingArray = underlyingArray;
        _offset = default;
        
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new IntV2(width, height);
    }

    public Slice2(RationalNumber[,] underlyingArray, IntV2 offset)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        int width = _underlyingArray.GetLength(1);
        int height = _underlyingArray.GetLength(0);
        _size = new IntV2(width - offset.X, height - offset.Y);
    }

    public Slice2(RationalNumber[,] underlyingArray, IntV2 offset, IntV2 size)
    {
        _underlyingArray = underlyingArray;
        _offset = offset;
        _size = size;
    }

    public readonly ref RationalNumber this[IntV2 pos]
    {
        get
        {
            Debug.Assert(pos.Y < _size.Y && pos.Y >= 0);
            Debug.Assert(pos.X < _size.X && pos.X >= 0);
            return ref _underlyingArray[(pos.Y + _offset.Y), (pos.X + _offset.X)];
        }
    }

    public readonly ref RationalNumber this[int i, int j]
    {
        get
        {
            return ref this[new IntV2(i, j)];
        }
    }

    public readonly int Width => Size.X;
    public readonly int Height => Size.Y;
    public readonly IntV2 Size => _size;
    public readonly int NumItems => _size.X * _size.Y;

    public readonly Slice2 Slice(IntV2 localOffset)
    {
        return Slice(localOffset, _size - localOffset);
    }

    public readonly Slice2 Slice(IntV2 localOffset, IntV2 newSize)
    {
        Debug.Assert(localOffset.Y < _size.Y && localOffset.Y >= 0);
        Debug.Assert(localOffset.X < _size.X && localOffset.X >= 0);
        Debug.Assert(newSize.Y <= _size.Y && newSize.Y >= 0);
        Debug.Assert(newSize.X <= _size.X && newSize.X >= 0);
        IntV2 offset = localOffset + _offset;
        IntV2 size = newSize;
        return new Slice2(_underlyingArray, offset, size);
    }

    public static implicit operator Slice2(RationalNumber[,] m) => new Slice2(m);
}
