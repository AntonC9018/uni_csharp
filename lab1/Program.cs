﻿/*
    Tema: Tablouri unidimensionale

    De elaborat o aplicație ce permite vizualizarea procesului de sortare (compararea, permutarea elementelor)
    pentru fiecare dintre trei metode de sortare a unui tablou unidimensional.
    Este utilizată o singură imagine (vizualizare) a tabloului, în care se văd procesele de comparare și permutare cînd este necesar.
    Alte imagini (vizualizări) ale tabloului nu sunt utilizate.
    De realizat posibilitatea alegerii diferitor metode de iniţializare a elementelor tabloului: iniţializare de la tastatură, inițializare aleatoare.
*/

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

class Program
{
    static void Main()
    {
        int[] array;
        switch (InputHelper.ReadInitializationKind())
        {
            case null:
                return;
            case ArrayInitializationKind.Random:
            {
                int? size = InputHelper.GetPositiveNumber("What should be the size of the array? ");
                if (!size.HasValue)
                    return;
                
                #if false
                {
                    int? min = InputHelper.GetPositiveNumber("What should be the minimum value? ");
                    if (!min.HasValue)
                        return;

                    // We need the values to be unique.
                    int? max;
                    {
                        int lowerBound = min.Value + size.Value;
                        var lowerBoundConstraint = new InputHelper.LowerBoundConstraint(lowerBound);
                        var constraints = new InputHelper.IInputConstraint<int>[] { lowerBoundConstraint };
                        max = InputHelper.GetNumberWithConstraints($"What should be the maximum value? (> {lowerBound}) ", constraints);
                        if (!max.HasValue)
                            return;
                    }

                    var rand = new Random(808080);
                    var valuesMet = new HashSet<int>();
                    array = new int[size.Value];
                    foreach (ref int v in array.AsSpan())
                    {
                        int t;
                        // Keep generating random values until we get one that has not been encountered before.
                        // We are guaranteed to be able to fill up the array randomly, because 
                        while (!valuesMet.Add(
                            t = rand.Next(min.Value, max.Value)))
                        {
                            v = t;
                        }
                    }
                }
                #endif

                array = Enumerable.Range(0, size.Value).ToArray();
                var rand = new Random(808080);
                rand.Shuffle(array.AsSpan());
                
                break;
            }
            case ArrayInitializationKind.Console:
            {
                int? size = InputHelper.GetPositiveNumber("What should be the size of the array? ");
                if (!size.HasValue)
                    return;

                array = new int[size.Value];
                for (int i = 0; i < size.Value; i++)
                {
                    int? input = InputHelper.GetPositiveNumber($"array[{i}] = ");
                    if (!input.HasValue)
                        return;
                    
                    array[i] = input.Value;
                }
                break;
            }

            // Never happens
            default:
                Debug.Fail("This should never happen");
                return;
        }

        {
            var comparer = Comparer<int>.Default;
            var display = new ThreadSleep_ConsoleCursor_Display();
            while (true)
            {
                void WriteArray(int[] arr) => Console.WriteLine("The array: " + string.Join(',', arr.Select(i => i.ToString())));
                
                WriteArray(array);
                Console.Write("Enter the sort kind to perform (quick).");
                string? input = Console.ReadLine();
                display.SetDrawToCurrentPosition();
                switch (input)
                {
                    case "quick":
                    {
                        var arrCopy = array.ToArray();
                        display.ArrayRef = arrCopy;
                        Sorting.QuickSort(arrCopy, comparer, display);
                        display.CompleteDrawing();
                        WriteArray(arrCopy);
                        break;
                    }

                    case null:
                        return;
                    
                    default:
                    {
                        Console.WriteLine(input + " is not a valid option.");
                        break;
                    }
                }

            }
        }

    }
}

public static class RandomHelper
{
    public static void Shuffle<T>(this Random rand, Span<T> array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = rand.Next(0, array.Length);
            Helper.Swap(ref array[i], ref array[j]);
        }
    }
}

public static class InputHelper
{
    public static ArrayInitializationKind? ReadInitializationKind()
    {
        while (true)
        {
            Console.Write("Should I generate the values randomly, or read them from the console? (random / console): ");
            string? input = Console.ReadLine();
            switch (input)
            {
                case "random":
                    return ArrayInitializationKind.Random;
                case "console":
                    return ArrayInitializationKind.Console;
                case "q" or null:
                    return null;
                default:
                    Console.WriteLine($"Invalid input: '{input}'");
                    continue;
            }
        }
    }

    public class PositiveNumberConstraint : IInputConstraint<int>
    {
        public static readonly PositiveNumberConstraint Instance = new();
        private PositiveNumberConstraint(){}
        public bool Check(int parsedValue)
        {
            return parsedValue >= 0;
        }

        public string FormatError(string rawValue, int parsedValue)
        {
            return rawValue + " should be positive.";
        }
    }

    public class LowerBoundConstraint : IInputConstraint<int>
    {
        public int LowerBound { get; set; }

        public LowerBoundConstraint(int lowerBound)
        {
            LowerBound = lowerBound;
        }

        public bool Check(int parsedValue)
        {
            return parsedValue >= LowerBound;
        }

        public string FormatError(string rawValue, int parsedValue)
        {
            return rawValue + " should be larger than " + LowerBound;
        }
    }

    private static readonly IInputConstraint<int>[] PositiveNumberConstraints = { PositiveNumberConstraint.Instance };
    public static int? GetPositiveNumber(string message)
    {
        return GetNumberWithConstraints(message, PositiveNumberConstraints);
    }

    public static int? GetNumberWithConstraints(string message, ReadOnlySpan<IInputConstraint<int>> constraints)
    {
        while (true)
        {
            Console.Write(message);
            string? input = Console.ReadLine();
            switch (input)
            {
                case "q" or null:
                    return null;
                default:
                {
                    if (int.TryParse(input, out int value))
                    {
                        bool bad = false;
                        foreach (var constraint in constraints)
                        {
                            if (!constraint.Check(value))
                            {
                                bad = true;
                                var error = constraint.FormatError(input, value);
                                Console.WriteLine(error);
                            }
                        }
                        if (bad)
                            continue;
                        return value;
                    }

                    Console.WriteLine($"'{input}' is not a number");
                    continue;
                }
            }
        }
    }

    public interface IInputConstraint<in T>
    {
        string FormatError(string rawValue, T? parsedValue);
        bool Check(T parsedValue);
    }
}

public static class Helper
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    public static int GetNumDecimalDigits(int value)
    {
        int numDigits = 1;
        int current = value;
        while ((current /= 10) != 0)
            numDigits++;
        return numDigits;
    }
}


public static class Sorting
{
    internal ref struct SortContext<T>
    {
        public Span<T> WholeSpan;
        public int StartIndex;
        public int SpanLength;
        public readonly Span<T> Span => WholeSpan.Slice(StartIndex, SpanLength);

        public IComparer<T> Comparer;
        public IDisplay<T> Display;

        public readonly void SwapLocal(int localIndex0, int localIndex1)
        {
            Swap(localIndex0 + StartIndex, localIndex1 + StartIndex);
        }

        public readonly int CompareLocal(int localIndex0, int localIndex1)
        {
            return Compare(localIndex0 + StartIndex, localIndex1 + StartIndex);
        }

        public readonly void Swap(int index0, int index1)
        {
            Display.BeginSwap(index0, index1);
            Helper.Swap(ref WholeSpan[index0], ref WholeSpan[index1]);
            Display.EndSwap(index0, index1);
        }

        public readonly int Compare(int index0, int index1)
        {
            int comparisonResult = Comparer.Compare(WholeSpan[index0], WholeSpan[index1]);
            Display.RecordComparison(index0, index1, comparisonResult);
            return comparisonResult;
        }
    } 

    internal static int quick_partition<T>(in SortContext<T> context)
    {
        var span = context.Span;
        var lastIndex = span.Length - 1;
        var pivotIndex = 0;

        for (int leftIndex = 0; leftIndex < lastIndex; leftIndex++)
        {
            if (context.CompareLocal(leftIndex, lastIndex) < 0)
            {   
                context.SwapLocal(pivotIndex, leftIndex);
                pivotIndex++;
            }
        }

        context.SwapLocal(span.Length - 1, pivotIndex);

        return pivotIndex;
    }

    internal static void quick_sort<T>(ref SortContext<T> context)
    {
        if (context.SpanLength <= 1)
            return;

        context.Display.RecordIteration();

        int partitionIndex = quick_partition(context);

        int ownStart = context.StartIndex;
        int ownLength = context.SpanLength;
        {
            context.StartIndex = ownStart;
            context.SpanLength = partitionIndex;
            quick_sort(ref context);
        }
        {
            context.StartIndex = ownStart + partitionIndex + 1;
            context.SpanLength = ownLength - (partitionIndex + 1);
            quick_sort(ref context);
        }
    }

    public static void QuickSort<T>(
        Span<T> span,
        IComparer<T> comparer,
        IDisplay<T> display)
    {
        var context = new SortContext<T>
        {
            Comparer = comparer,
            Display = display, 
            WholeSpan = span,
            SpanLength = span.Length,
            StartIndex = 0,
        };
        quick_sort<T>(ref context);
    }
}

public interface IDisplay<T>
{
    void RecordIteration();
    void RecordComparison(int index0, int index1, int comparisonResult);
    void BeginSwap(int index0, int index1);
    void EndSwap(int index0, int index1);
}

public static class DisplayHelper
{
    public struct Counters
    {
        public int Iteration;
        public int Comparison;
        public int Swap;
    }
}

public sealed class PrintAllOnNewLinesDisplay : IDisplay<int>
{
    private DisplayHelper.Counters _counters;
    public int[] ArrayRef { get; set; }

    public PrintAllOnNewLinesDisplay(int[] arrayRef)
    {
        ArrayRef = arrayRef;
    }

    public void ResetCounters()
    {
        _counters = default;
    }

    private void WriteState(ReadOnlySpan<int> state)
    {
        for (int i = 0; i < state.Length; i++)
        {
            if (i != 0)
                Console.Write(", ");
            Console.Write(state[i]);
        }
    }

    public void RecordIteration()
    {
        Console.Write($"Iteration {_counters.Iteration++}.");
        WriteState(ArrayRef);
        Console.WriteLine();
    }

    public void RecordComparison(int index0, int index1, int comparisonResult)
    {
        Console.WriteLine($"Comparison {_counters.Comparison++}: {ArrayRef[index0]} > {ArrayRef[index1]} = {comparisonResult}.");
        WriteState(ArrayRef);
        Console.WriteLine();
    }

    public void BeginSwap(int index0, int index1)
    {
        Console.WriteLine($"Swap {_counters.Swap++}. [{index0}] <-> [{index1}].");
        WriteState(ArrayRef);
        Console.WriteLine();
    }

    public void EndSwap(int index0, int index1)
    {
        WriteState(ArrayRef);
        Console.WriteLine();
    }
}

public sealed class ThreadSleep_ConsoleCursor_Display : IDisplay<int>
{
    private int[]? _arrayRef;
    private int _cachedElementWidth;

    public int[]? ArrayRef
    {
        get
        {
            return _arrayRef;
        }
        set
        {
            if (value is not null)
                _cachedElementWidth = Helper.GetNumDecimalDigits(value.Max());
            _arrayRef = value;
        }
    }

    public struct DrawPosition
    {
        public int Top;
        public int Left;
    }
    public DrawPosition? Position { get; set; }

    public void SetDrawToCurrentPosition()
    {
        Position = new()
        {
            Top = Console.CursorTop,
            Left = Console.CursorLeft,
        };
    }

    public void CompleteDrawing()
    {
        Debug.Assert(Position.HasValue);
        var pos = Position.Value;
        Console.CursorTop = pos.Top + 1;
        Console.CursorLeft = 0;
        Position = null;
    }

    private static void DrawState(DrawPosition position, int elementWidth, ReadOnlySpan<int> array)
    {
        Console.CursorTop = position.Top;
        Console.CursorLeft = position.Left;
        Console.Write("[");
        for (int i = 0; i < array.Length; i++)
        {
            if (i != 0)
                Console.Write(" ");
            int value = array[i];
            string strValue = value.ToString().PadLeft(elementWidth);
            Console.Write(strValue);
        }
        Console.Write("]");
    }

    [MemberNotNull(nameof(_arrayRef))]
    [MemberNotNull(nameof(Position))]
    private void AssertInitialized()
    {
        Debug.Assert(_arrayRef is not null);
        Debug.Assert(Position.HasValue);
    }

    private void DrawAndSleep(int sleepDuration)
    {
        AssertInitialized();
        DrawState(Position.Value, _cachedElementWidth, _arrayRef);
        Thread.Sleep(sleepDuration);
    }

    public void BeginSwap(int index0, int index1)
    {
        // DrawAndSleep(1000);
    }

    public void EndSwap(int index0, int index1)
    {
        if (index0 != index1)
            DrawAndSleep(1000);
    }

    public void RecordComparison(int index0, int index1, int comparisonResult)
    {
        // DrawAndSleep(1000);
    }

    public void RecordIteration()
    {
        // DrawAndSleep(1000);
    }
}


public enum ArrayInitializationKind
{
    Random,
    Console,
}

