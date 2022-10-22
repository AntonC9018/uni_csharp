/*
    Tema: Tablouri unidimensionale

    De elaborat o aplicație ce permite vizualizarea procesului de sortare (compararea, permutarea elementelor)
    pentru fiecare dintre trei metode de sortare a unui tablou unidimensional.
    Este utilizată o singură imagine (vizualizare) a tabloului, în care se văd procesele de comparare și permutare cînd este necesar.
    Alte imagini (vizualizări) ale tabloului nu sunt utilizate.
    De realizat posibilitatea alegerii diferitor metode de iniţializare a elementelor tabloului: iniţializare de la tastatură, inițializare aleatoare.
*/

namespace Laborator1;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Shared;

internal class Program
{
    public static void Main()
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
                        // We are guaranteed to be able to fill up the array randomly, because the max value
                        // has been capped manually such that there are at least as many random values possible
                        // as the size of the array.
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

        var arrCopy = array.ToArray();
        IDisplay<int>? display = null;
        while (display is null)
        {
            Console.Write("Which kind of visualization to use (color, animation, dump)? ");
            string? input = Console.ReadLine();
            switch (input)
            {
                case "color":
                {
                    var color = new ThreadSleep_ConsoleCursor_Colorful_Display(arrCopy);

                    int? sleepDuration = InputHelper.GetPositiveNumber("Sleep duration (ms): ");
                    if (!sleepDuration.HasValue)
                        return;
                    color.SleepDurationMilliseconds = sleepDuration.Value;

                    display = color;
                    break;
                }
                case "animation":
                {
                    var animation = new ThreadSleep_ConsoleCursor_Animated_Display(arrCopy);

                    int? actionDuration = InputHelper.GetPositiveNumber("Action duration (ms): ");
                    if (!actionDuration.HasValue)
                        return;
                    animation.ActionAnimationDurationMilliseconds = actionDuration.Value;
                    
                    int? swapDuration = InputHelper.GetPositiveNumber("Swap duration (ms): ");
                    if (!swapDuration.HasValue)
                        return;
                    animation.SwapAnimationDurationMilliseconds = swapDuration.Value;
                    
                    display = animation;
                    break;
                }
                case "dump":
                {
                    display = new PrintAllOnNewLinesDisplay(arrCopy);
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
        }
        {
            var comparer = Comparer<int>.Default;
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

public static class DrawHelper
{
    public struct DrawPosition
    {
        public int Top;
        public int Left;
    }

    public static DrawPosition GetCurrentDrawPosition()
    {
        return new()
        {
            Top = Console.CursorTop,
            Left = Console.CursorLeft,
        };
    }

    public static void SetConsolePosition(DrawPosition position)
    {
        Console.CursorTop = position.Top;
        Console.CursorLeft = position.Left;
    }

    public static void WriteValuePadded<T>(T? value, int elementWidth)
    {
        string strValue = (value?.ToString() ?? "").PadLeft(elementWidth);
        Console.Write(strValue);
    }

    public struct ComparisonColors
    {
        public ConsoleColor EqualColor;
        public ConsoleColor LessColor;
        public ConsoleColor GreaterColor;
        public ConsoleColor HighlightColor;
    }

    public static readonly ComparisonColors DefaultComparisonColors = new()
    {
        EqualColor = ConsoleColor.Yellow,
        LessColor = ConsoleColor.Red,
        GreaterColor = ConsoleColor.Green,
        HighlightColor = ConsoleColor.Cyan,
    };

    public struct CachedStringsInfo
    {
        public string[] StringArray;
        public int ElementWidth;

        public CachedStringsInfo(string[] stringArray, int elementWidth)
        {
            StringArray = stringArray;
            ElementWidth = elementWidth;
        }
    }

    public static CachedStringsInfo CacheStrings(int[] array)
    {
        var elementWidth = Helper.GetNumDecimalDigits(array.Max());
        var stringArray = array.Select(t => t.ToString().PadLeft(elementWidth)).ToArray();
        return new CachedStringsInfo(stringArray, elementWidth);
    }

    public static void DrawArrayState(DrawHelper.DrawPosition position, int elementWidth, IStringIndexer array)
    {
        Console.ResetColor();
        Console.CursorTop = position.Top;
        Console.CursorLeft = position.Left;

        // decoration param?
        Console.Write("[");

        for (int i = 0; i < array.Length; i++)
            DrawArrayElement(isFirst: i == 0, array[i]);

        Console.Write("]");
    }

    public static void DrawArrayElement(bool isFirst, string value, string spacing = DefaultArrayElementSpacing)
    {
        if (!isFirst)
            Console.Write(spacing);

        Console.Write(value);
    }

    public const string DefaultArrayElementSpacing = " ";
    // How is this not a constant? Are you actually kidding me??
    // Why can a string be const, but not DefaultArrayElementSpacing.Length, that's just nonsense.
    public const int DefaultArrayElementSpacingLength = 1; // DefaultArrayElementSpacing.Length;

    public static int GetPosOfElement_InDecoratedArray(int left, int index, int elementWidth)
    {
        return left
            // [
            + 1
            
            // go to pos
            + elementWidth * index
            
            // whitespace
            + index * DefaultArrayElementSpacingLength;
    }
}

public interface IStringIndexer
{
    string this[int index] { get; }
    int Length { get; }
}

public abstract class GraphicalConsoleDisplayMixin : IDisplay<int>, IStringIndexer
{
    protected DrawHelper.CachedStringsInfo _cachedArrayInfo;
    protected int[] _indexPermutation;

    protected GraphicalConsoleDisplayMixin(DrawHelper.CachedStringsInfo cachedArrayInfo)
    {
        _cachedArrayInfo = cachedArrayInfo;
        _indexPermutation = Enumerable.Range(0, cachedArrayInfo.StringArray.Length).ToArray();
    }

    public virtual int[] ArrayValues
    {
        set => _cachedArrayInfo = DrawHelper.CacheStrings(value);
    }

    public DrawHelper.DrawPosition? Position { get; set; }
    public void SetDrawToCurrentPosition() => Position = DrawHelper.GetCurrentDrawPosition();

    protected abstract int ViewportHeight { get; }

    protected string GetElementString(int index)
    {
        return _cachedArrayInfo.StringArray[_indexPermutation[index]];
    }

    // TODO: pull out into a helper class
    string IStringIndexer.this[int index] => GetElementString(index);
    int IStringIndexer.Length => _indexPermutation.Length;

    public virtual void CompleteDrawing()
    {
        Debug.Assert(Position.HasValue);
        var pos = Position.Value;
        Console.CursorTop = pos.Top + ViewportHeight;
        Console.CursorLeft = 0;
        Position = null;
        Console.ResetColor();

        for (int i = 0; i < _indexPermutation.Length; i++)
            _indexPermutation[i] = i;
    }

    [MemberNotNull(nameof(Position))]
    protected virtual void AssertInitialized()
    {
        Debug.Assert(Position.HasValue);
    }

    public virtual void BeingDrawing()
    {
        SetDrawToCurrentPosition();
    }

    public virtual void EndDrawing()
    {
        CompleteDrawing();
    }

    public abstract void RecordIteration();
    public abstract void RecordComparison(int index0, int index1, int comparisonResult);

    void IDisplay<int>.BeginSwap(int index0, int index1)
    {
        if (index0 == index1)
            return;

        DoBeginSwap(index0, index1);
    }

    void IDisplay<int>.EndSwap(int index0, int index1)
    {
        if (index0 == index1)
            return;

        ref var v0 = ref _indexPermutation[index0];
        ref var v1 = ref _indexPermutation[index1];
        Helper.Swap(ref v0, ref v1);

        DoEndSwap(index0, index1);
    }

    protected virtual void DoBeginSwap(int index0, int index1)
    {
    }

    protected virtual void DoEndSwap(int index0, int index1)
    {
    }
}

public sealed class ThreadSleep_ConsoleCursor_Colorful_Display : GraphicalConsoleDisplayMixin
{
    public int SleepDurationMilliseconds { get; set; } = 1000;

    // This one always takes a single line.
    protected override int ViewportHeight => 1;

    // This cannot be set via the ArrayValues property because it's virtual.
    public ThreadSleep_ConsoleCursor_Colorful_Display(int[] values) : base(DrawHelper.CacheStrings(values))
    {
    }

    private void DrawAndSleep(int sleepDuration)
    {
        Redraw();
        Thread.Sleep(sleepDuration);
    }

    private static readonly ConsoleColor[] _TwoElementHightlightColors = { ConsoleColor.Magenta, ConsoleColor.Cyan, };


    private void WriteHighlighted(int index0, int index1)
    {
        WriteColored(index0, index1, _TwoElementHightlightColors[0], _TwoElementHightlightColors[1]);
    }

    private void WriteColored(int index0, int index1, ConsoleColor color0, ConsoleColor color1)
    {
        AssertInitialized();
        
        var position = Position.Value;
        Console.CursorTop = position.Top;

        int GetPosOfElement(int index)
        {
            return DrawHelper.GetPosOfElement_InDecoratedArray(
                position.Left, index, _cachedArrayInfo.ElementWidth);
        }

        Console.CursorLeft = GetPosOfElement(index0);
        Console.ForegroundColor = color0;
        Console.Write(GetElementString(index0), _cachedArrayInfo.ElementWidth);

        Console.CursorLeft = GetPosOfElement(index1);
        Console.ForegroundColor = color1;
        Console.Write(GetElementString(index1), _cachedArrayInfo.ElementWidth);
    }

    private void Redraw()
    {
        AssertInitialized();
        DrawHelper.DrawArrayState(Position.Value, _cachedArrayInfo.ElementWidth, this);
    }

    protected override void DoBeginSwap(int index0, int index1)
    {
        Redraw();
        WriteHighlighted(index0, index1);
        Thread.Sleep(SleepDurationMilliseconds);
    }

    protected override void DoEndSwap(int index0, int index1)
    {
        Redraw();
        WriteHighlighted(index1, index0);
        Thread.Sleep(SleepDurationMilliseconds);
    }

    public override void RecordComparison(int index0, int index1, int comparisonResult)
    {
        var colors = DrawHelper.DefaultComparisonColors;
        Redraw();
        WriteColored(index0, index1, colors.HighlightColor, colors.HighlightColor);
        Thread.Sleep(SleepDurationMilliseconds);

        if (comparisonResult == 0)
            WriteColored(index0, index1, colors.EqualColor, colors.EqualColor);
        else if (comparisonResult < 0)
            WriteColored(index0, index1, colors.LessColor, colors.GreaterColor);
        else
            WriteColored(index0, index1, colors.GreaterColor, colors.LessColor);
        Thread.Sleep(SleepDurationMilliseconds);        
    }

    public override void RecordIteration()
    {
        DrawAndSleep(SleepDurationMilliseconds);
    }
}

public sealed class ThreadSleep_ConsoleCursor_Animated_Display : GraphicalConsoleDisplayMixin
{
    public int SwapAnimationDurationMilliseconds { get; set; } = 1000;
    public int ActionAnimationDurationMilliseconds { get; set; } = 1000;
    private string _cachedClearString;

    public ThreadSleep_ConsoleCursor_Animated_Display(int[] values) : base(DrawHelper.CacheStrings(values))
    {
        CacheClearString();
    }

    [MemberNotNull(nameof(_cachedClearString))]
    private void CacheClearString()
    {
        int spacingWidth = _cachedArrayInfo.StringArray.Length - 1;
        int decorationWidth = 2; // [ .. ]
        int allElementsWidth = _cachedArrayInfo.ElementWidth * _cachedArrayInfo.StringArray.Length;
        _cachedClearString = new string(' ', spacingWidth + decorationWidth + allElementsWidth);
    }

    // 2 for the animated things and 1 for the main array thing.
    protected override int ViewportHeight => 3;
    public override int[] ArrayValues
    {
        set
        {
            base.ArrayValues = value;
            CacheClearString();
        }
    }

    private void ClearViewport()
    {
        AssertInitialized();
        DrawHelper.SetConsolePosition(Position.Value);

        string clearString = _cachedClearString;
        int height = ViewportHeight;

        for (int i = 0; i < height; i++)
            Console.WriteLine(clearString);
    }
    
    private struct DrawData
    {
        public int Index;
        public DrawHelper.DrawPosition Position;
        public ConsoleColor Color;
    }

    private void Redraw(Span<DrawData> manuallyDrawnIndicesAndPositions, Span<int> sortedIgnoredIndices)
    {
        AssertInitialized();
        ClearViewport();

        DrawIntervalsInBetweenIgnoredIndices(sortedIgnoredIndices);

        foreach (ref var ip in manuallyDrawnIndicesAndPositions)
            DrawElement(ip);

        void DrawIntervalsInBetweenIgnoredIndices(Span<int> sortedIgnoredIndices)
        {
            AssertInitialized();
            
            var center = Position.Value;

            center.Top += 1;
            DrawHelper.SetConsolePosition(center);
            Console.ResetColor();

            Console.Write("[");
            int j = 0;
            for (int i = 0; i < sortedIgnoredIndices.Length; i++)
            {
                for (; j < sortedIgnoredIndices[i]; j++)
                    DrawHelper.DrawArrayElement(isFirst: j == 0, GetElementString(j));
                
                int offset = _cachedArrayInfo.ElementWidth;
                if (j != 0)
                    offset += DrawHelper.DefaultArrayElementSpacingLength;
                Console.CursorLeft += offset;

                j += 1;
            }
            for (; j < _cachedArrayInfo.StringArray.Length; j++)
                DrawHelper.DrawArrayElement(isFirst: j == 0, GetElementString(j));

            Console.Write("]");
        }

        void DrawElement(in DrawData ip)
        {
            var value = GetElementString(ip.Index);
            DrawHelper.SetConsolePosition(ip.Position);
            Console.ForegroundColor = ip.Color;
            Console.Write(value);
        }
    }

    private void PrepareIndicesAndPositionsForAnimation(Span<DrawData> ips, ref int index0, ref int index1)
    {
        NormalizeIndices(ref index0, ref index1);
        SetIndices(ips, index0, index1);
        SetXs(ips);

        void NormalizeIndices(ref int index0, ref int index1)
        {
            if (index0 > index1)
                Helper.Swap(ref index0, ref index1);
        }

        void SetIndices(Span<DrawData> ips, int index0, int index1)
        {
            Debug.Assert(index0 < index1);
            Debug.Assert(ips.Length == 2);
            ips[0].Index = index0;
            ips[1].Index = index1;
        }

        void SetXs(Span<DrawData> ips)
        {
            AssertInitialized();
            var position = Position.Value;

            int GetX(int index)
            {
                return DrawHelper.GetPosOfElement_InDecoratedArray(position.Left, index, _cachedArrayInfo.ElementWidth);
            }
            foreach (ref var ip in ips)
                ip.Position.Left = GetX(ip.Index);
        }
    }

    private void DrawStateNormally()
    {
        AssertInitialized();
        ClearViewport();
        var center = Position.Value;
        center.Top += 1;
        DrawHelper.DrawArrayState(center, _cachedArrayInfo.ElementWidth, this);
        Thread.Sleep(ActionAnimationDurationMilliseconds);
    }
    
    private void DrawSwapAnimationSequence(int index0, int index1)
    {
        AssertInitialized();
        var position = Position.Value;

        const int numSwapElements = 2;
        Span<DrawData> ips = stackalloc DrawData[numSwapElements];
        PrepareIndicesAndPositionsForAnimation(ips, ref index0, ref index1);
        ips[0].Position.Top = position.Top;
        ips[1].Position.Top = position.Top + 2;
        ips[0].Color = ConsoleColor.Yellow;
        ips[1].Color = ConsoleColor.Yellow;

        Span<int> ignoredIndices = stackalloc int[]{index0, index1};

        // Animation loop
        {
            int increment = _cachedArrayInfo.ElementWidth + DrawHelper.DefaultArrayElementSpacingLength;
            int numSteps = (index1 - index0);
            int singleAnimationDuration = SwapAnimationDurationMilliseconds / numSteps;
            
            // 
            Redraw(ips, ignoredIndices);
            Thread.Sleep(singleAnimationDuration);
            
            for (int i = 0; i < numSteps; i++)
            {
                ips[0].Position.Left += increment;
                ips[1].Position.Left -= increment;
                Redraw(ips, ignoredIndices);
                Thread.Sleep(singleAnimationDuration);
            }
        }
    }

    private void DrawComparisonAnimationSequence(int index0, int index1, int comparisonResult)
    {
        // kinda messy with the same check being done in the function
        if (index1 < index0)
            comparisonResult = -comparisonResult;

        AssertInitialized();
        var position = Position.Value;
        Span<DrawData> ips = stackalloc DrawData[2];
        PrepareIndicesAndPositionsForAnimation(ips, ref index0, ref index1);
        ips[0].Position.Top = position.Top;
        ips[1].Position.Top = position.Top;

        Span<int> ignoredIndices = stackalloc int[]{index0, index1};

        var comparisonColors = DrawHelper.DefaultComparisonColors;
        foreach (ref var ip in ips)
            ip.Color = comparisonColors.HighlightColor;

        Redraw(ips, ignoredIndices);
        Thread.Sleep(ActionAnimationDurationMilliseconds);

        // Now the colors show which one was larger
        if (comparisonResult < 0)
        {
            ips[0].Color = comparisonColors.LessColor;
            ips[1].Color = comparisonColors.GreaterColor;
        }
        else if (comparisonResult > 0)
        {
            ips[1].Color = comparisonColors.LessColor;
            ips[0].Color = comparisonColors.GreaterColor;
        }
        else
        {
            ips[0].Color = comparisonColors.EqualColor;
            ips[1].Color = comparisonColors.EqualColor;
        }

        Redraw(ips, ignoredIndices);
        Thread.Sleep(ActionAnimationDurationMilliseconds);

        // And now back to normal
        DrawStateNormally();
    }

    protected override void DoBeginSwap(int index0, int index1)
    {
        DrawSwapAnimationSequence(index0, index1);
    }

    protected override void DoEndSwap(int index0, int index1)
    {
        // Draw the array normally at the end, once the elements are above / below the new positions.
        DrawStateNormally();
    }

    public override void RecordComparison(int index0, int index1, int comparisonResult)
    {
        DrawComparisonAnimationSequence(index0, index1, comparisonResult);
    }

    public override void RecordIteration()
    {
    }
}
