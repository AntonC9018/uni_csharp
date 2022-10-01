/*
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
            var arrCopy = array.ToArray();
            IDisplay<int> display = true
                ? new ThreadSleep_ConsoleCursor_Display(arrCopy)
                : new PrintAllOnNewLinesDisplay(arrCopy);
            
            while (true)
            {
                Console.Write("Enter the sort kind to perform (quick / heap / selection): ");
                string? input = Console.ReadLine();
                switch (input)
                {
                    case "quick":
                    {
                        display.BeingDrawing();
                        Sorting.QuickSort(arrCopy, comparer, display);
                        display.EndDrawing();
                        break;
                    }

                    case "heap":
                    {
                        display.BeingDrawing();
                        Sorting.HeapSort(arrCopy, comparer, display);
                        display.EndDrawing();
                        break;
                    }

                    case "selection":
                    {
                        display.BeingDrawing();
                        Sorting.SelectionSort(arrCopy, comparer, display);
                        display.EndDrawing();
                        break;
                    }

                    case "q" or null:
                        return;
                    
                    default:
                    {
                        Console.WriteLine(input + " is not a valid option.");
                        continue;
                    }
                }

                array.CopyTo(arrCopy.AsSpan());
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

        public SortContext(Span<T> span, IComparer<T> comparer, IDisplay<T> display)
        {
            Comparer = comparer;
            Display = display; 
            WholeSpan = span;
            SpanLength = span.Length;
            StartIndex = 0;
        }

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

        public readonly (int StartIndex, int SpanLength) SaveSpan()
        {
            return (StartIndex, SpanLength);
        }

        public void RestoreSpan((int StartIndex, int SpanLength) span)
        {
            (StartIndex, SpanLength) = span;
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
        var context = new SortContext<T>(span, comparer, display);
        quick_sort<T>(ref context);
    }

    // Interprets SpanLength as the length to consider,
    // and the StartIndex as the current index.
    // So doing `.Span` is invalid in this function.
    internal static void heapify<T>(ref SortContext<T> context)
    {
        context.Display.RecordIteration();

        int indexLeft = context.StartIndex * 2 + 1;
        int indexRight = context.StartIndex * 2 + 2;
        int indexLargest = context.StartIndex;

        if (indexLeft < context.SpanLength)
        {
            if (context.Compare(indexLeft, indexLargest) > 0)
                indexLargest = indexLeft;
        }

        if (indexRight < context.SpanLength)
        {
            if (context.Compare(indexRight, indexLargest) > 0)
                indexLargest = indexRight;
        }

        if (indexLargest != context.StartIndex)
        {
            context.Swap(context.StartIndex, indexLargest);
            context.StartIndex = indexLargest;
            heapify(ref context);
        }
    }

    
    internal static void heap_sort_internal<T>(ref SortContext<T> context)
    {
        int size = context.SpanLength;
        for (int i = size / 2; i > 0; i--)
        {
            context.StartIndex = i - 1;
            heapify(ref context);
        }

        for (int i = size - 1; i > 0; i--)
        {
            context.Swap(0, i);
            context.StartIndex = 0;
            context.SpanLength = i;
            heapify(ref context);
        }
    }

    public static void HeapSort<T>(Span<T> span, IComparer<T> comparer, IDisplay<T> display)
    {
        var context = new SortContext<T>(span, comparer, display);
        heap_sort_internal(ref context); 
    }

    public static void SelectionSort<T>(Span<T> span, IComparer<T> comparer, IDisplay<T> display)
    {
        var context = new SortContext<T>(span, comparer, display);
        while (context.SpanLength != 0)
        {
            // find the minimum
            int minIndex = 0;
            for (int i = 1; i < context.SpanLength; i++)
            {
                if (context.CompareLocal(i, minIndex) < 0)
                    minIndex = i;
            }

            context.SwapLocal(minIndex, 0);

            context.StartIndex++;
            context.SpanLength--;
        }
    }
}

public interface IDisplay<T>
{
    void BeingDrawing();
    void EndDrawing();
    
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

    public void BeingDrawing()
    {
    }

    public void EndDrawing()
    {
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
    
    public ThreadSleep_ConsoleCursor_Display(int[] arrayRef)
    {
        ArrayRef = arrayRef;
    }

    public void CompleteDrawing()
    {
        Debug.Assert(Position.HasValue);
        var pos = Position.Value;
        Console.CursorTop = pos.Top + 1;
        Console.CursorLeft = 0;
        Position = null;
        Console.ResetColor();
    }

    private static void WriteValuePadded(int value, int elementWidth)
    {
        string strValue = value.ToString().PadLeft(elementWidth);
        Console.Write(strValue);
    }

    private static void DrawState(DrawPosition position, int elementWidth, ReadOnlySpan<int> array)
    {
        Console.ResetColor();
        Console.CursorTop = position.Top;
        Console.CursorLeft = position.Left;
        Console.Write("[");
        for (int i = 0; i < array.Length; i++)
        {
            if (i != 0)
                Console.Write(" ");
            WriteValuePadded(array[i], elementWidth);
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
        Redraw();
        Thread.Sleep(sleepDuration);
    }

    private static readonly ConsoleColor[] TwoElementHightlightColorMap = { ConsoleColor.Magenta, ConsoleColor.Cyan, };

    private void WriteHighlighted(int index0, int index1)
    {
        WriteColored(index0, index1, TwoElementHightlightColorMap[0], TwoElementHightlightColorMap[1]);
    }

    private void WriteColored(int index0, int index1, ConsoleColor color0, ConsoleColor color1)
    {
        AssertInitialized();
        
        var position = Position.Value;
        Console.CursorTop = position.Top;

        int GetPosOfElement(int left, int index)
        {
            return left
                // [
                + 1
                
                // go to pos
                + _cachedElementWidth * index
                
                // whitespace
                + index;
        }

        Console.CursorLeft = GetPosOfElement(position.Left, index0);
        Console.ForegroundColor = color0;
        WriteValuePadded(_arrayRef[index0], _cachedElementWidth);

        Console.CursorLeft = GetPosOfElement(position.Left, index1);
        Console.ForegroundColor = color1;
        WriteValuePadded(_arrayRef[index1], _cachedElementWidth);
    }

    private void Redraw()
    {
        AssertInitialized();
        DrawState(Position.Value, _cachedElementWidth, _arrayRef);
    }

    public void BeginSwap(int index0, int index1)
    {
        if (index0 == index1)
            return;
        
        Redraw();
        WriteHighlighted(index0, index1);
        Thread.Sleep(1000);
    }

    public void EndSwap(int index0, int index1)
    {
        if (index0 == index1)
            return;
            
        Redraw();
        WriteHighlighted(index1, index0);
        Thread.Sleep(1000);
    }

    private struct ComparisonColors
    {
        public ConsoleColor EqualColor;
        public ConsoleColor LessColor;
        public ConsoleColor GreaterColor;
        public ConsoleColor HighlightColor;
    }
    private static readonly ComparisonColors ComparisonColorMap = new()
    {
        EqualColor = ConsoleColor.Yellow,
        LessColor = ConsoleColor.Red,
        GreaterColor = ConsoleColor.Green,
        HighlightColor = ConsoleColor.Cyan,
    };

    public void RecordComparison(int index0, int index1, int comparisonResult)
    {
        Redraw();
        WriteColored(index0, index1, ComparisonColorMap.HighlightColor, ComparisonColorMap.HighlightColor);
        Thread.Sleep(1000);

        if (comparisonResult == 0)
            WriteColored(index0, index1, ComparisonColorMap.EqualColor, ComparisonColorMap.EqualColor);
        else if (comparisonResult < 0)
            WriteColored(index0, index1, ComparisonColorMap.LessColor, ComparisonColorMap.GreaterColor);
        else
            WriteColored(index0, index1, ComparisonColorMap.GreaterColor, ComparisonColorMap.LessColor);
        Thread.Sleep(1000);        
    }

    public void RecordIteration()
    {
        DrawAndSleep(1000);
    }

    void IDisplay<int>.BeingDrawing()
    {
        SetDrawToCurrentPosition();
    }

    void IDisplay<int>.EndDrawing()
    {
        CompleteDrawing();
    }
}


public enum ArrayInitializationKind
{
    Random,
    Console,
}

