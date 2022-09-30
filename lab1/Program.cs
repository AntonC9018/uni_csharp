/*
    Tema: Tablouri unidimensionale

    De elaborat o aplicație ce permite vizualizarea procesului de sortare (compararea, permutarea elementelor)
    pentru fiecare dintre trei metode de sortare a unui tablou unidimensional.
    Este utilizată o singură imagine (vizualizare) a tabloului, în care se văd procesele de comparare și permutare cînd este necesar.
    Alte imagini (vizualizări) ale tabloului nu sunt utilizate.
    De realizat posibilitatea alegerii diferitor metode de iniţializare a elementelor tabloului: iniţializare de la tastatură, inițializare aleatoare.
*/

using System.Diagnostics;

class Program
{
    static void Main()
    {
        int[] array;
        switch (ReadInitializationKind())
        {
            case null:
                return;
            case ArrayInitializationKind.Random:
            {
                int? size = GetPositiveNumber("What should be the size of the array? ");
                if (!size.HasValue)
                    return;
                
                int? min = GetPositiveNumber("What should be the minimum value? ");
                if (!min.HasValue)
                    return;

                int? max = GetPositiveNumber("What should be the maximum value? ");
                if (!max.HasValue)
                    return;

                var rand = new Random(808080);
                array = new int[size.Value];
                foreach (ref int v in array.AsSpan())
                    v = rand.Next(min.Value, max.Value);
                break;
            }
            case ArrayInitializationKind.Console:
            {
                int? size = GetPositiveNumber("What should be the size of the array? ");
                if (!size.HasValue)
                    return;

                array = new int[size.Value];
                for (int i = 0; i < size.Value; i++)
                {
                    int? input = GetPositiveNumber($"array[{i}] = ");
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
            var display = new Display();
            while (true)
            {
                Console.WriteLine("The array: " + string.Join(',', array.Select(i => i.ToString())));
                Console.Write("Enter the sort kind to perform (quick).");
                string? input = Console.ReadLine();
                switch (input)
                {
                    case "quick":
                    {
                        var arrCopy = array.ToArray();
                        Sorting.QuckSort(arrCopy, comparer, display);
                        Console.WriteLine("The array: " + string.Join(',', arrCopy.Select(i => i.ToString())));
                        continue;
                    }
                    case null:
                        return;
                    
                    default:
                    {
                        Console.WriteLine(input + " is not a valid option.");
                        continue;
                    }
                }
            }
        }

    }

    static ArrayInitializationKind? ReadInitializationKind()
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

    static int? GetPositiveNumber(string message)
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
                        if (value < 0)
                        {
                            Console.WriteLine("The values should be positive");
                            continue;
                        }
                        return value;
                    }

                    Console.WriteLine($"'{input}' is not a number");
                    continue;
                }
            }
        }
    }
}


public static class Sorting
{
    internal ref struct SortContext<T>
    {
        public Span<T> Span;
        public IComparer<T> Comparer;
        public IDisplay<T> Display;

        public readonly void Swap(int a, int b)
        {
            Display.RecordSwap(Span, a, b);
            Sorting.Swap(ref Span[a], ref Span[b]);
        }
    } 

    internal static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    internal static int quick_partition<T>(SortContext<T> context)
    {
        var pivotValue = context.Span[^1];
        var pivotIndex = 0;
        int begin = 0;

        while (context.Comparer.Compare(context.Span[pivotIndex], pivotValue) != 0)
        {
            int comparisonResult = context.Comparer.Compare(context.Span[pivotIndex], pivotValue);
            context.Display.RecordComparisonValueIndex(context.Span, pivotValue, pivotIndex, comparisonResult);

            if (comparisonResult < 0)
            {   
                context.Swap(pivotIndex, begin);
                pivotIndex++;
            }
            begin++;
        }

        context.Swap(context.Span.Length - 1, pivotIndex);

        return pivotIndex;
    }

    internal static void quick_sort<T>(SortContext<T> context)
    {
        if (context.Span.Length == 0)
            return;

        context.Display.RecordIteration(context.Span);

        int partitionIndex = quick_partition(context);

        var contextTemp = context;
        {
            contextTemp.Span = context.Span[.. partitionIndex];
            quick_sort(contextTemp);
        }
        {
            contextTemp.Span = context.Span[(partitionIndex + 1) ..];
            quick_sort(contextTemp);
        }
    }

    public static void QuckSort<T>(
        Span<T> span,
        IComparer<T> comparer,
        IDisplay<T> display)
    {
        quick_sort<T>(new()
        {
            Span = span,
            Comparer = comparer,
            Display = display,
        });
    }
}

public interface IDisplay<T>
{
    void RecordIteration(ReadOnlySpan<T> state);
    void RecordComparisonIndexIndex(ReadOnlySpan<T> state, int index0, int index1, int comparisonResult);
    void RecordComparisonValueIndex(ReadOnlySpan<T> state, T value, int index, int comparisonResult);
    void RecordComparisonValueValue(ReadOnlySpan<T> state, T value0, T value1, int comparisonResult);
    void RecordSwap(ReadOnlySpan<T> state, int index0, int index1);
}

public class Display : IDisplay<int>
{
    private int _iterationCount;
    private int _comparisonCount;
    private int _swapCount;

    private void WriteState(ReadOnlySpan<int> state)
    {
        for (int i = 0; i < state.Length; i++)
        {
            if (i != 0)
                Console.Write(", ");
            Console.Write(state[i]);
        }
    }

    private void WriteStateSwapped(ReadOnlySpan<int> state, int a, int b)
    {
        for (int i = 0; i < state.Length; i++)
        {
            if (i != 0)
                Console.Write(", ");
            if (i == a)
                Console.Write(state[b]);
            else if (i == b)
                Console.Write(state[a]);
            else 
                Console.Write(state[i]);
        }
    }

    public void RecordIteration(ReadOnlySpan<int> state)
    {
        Console.Write($"Iteration {_iterationCount++}. State: ");
        WriteState(state);
        Console.WriteLine();
    }

    public void RecordSwap(ReadOnlySpan<int> state, int index0, int index1)
    {
        Console.WriteLine($"Swap {_swapCount++}. [{index0}] <-> [{index1}].");
        
        Console.Write("State before: ");
        WriteState(state);
        Console.WriteLine();

        Console.Write("State after: ");
        WriteStateSwapped(state, index0, index1);
        Console.WriteLine();
    }

    public void RecordComparisonIndexIndex(ReadOnlySpan<int> state, int index0, int index1, int comparisonResult)
    {
        RecordComparisonValueValue(state, state[index0], state[index1], comparisonResult);
    }

    public void RecordComparisonValueIndex(ReadOnlySpan<int> state, int value, int index, int comparisonResult)
    {
        RecordComparisonValueValue(state, value, state[index], comparisonResult);
    }

    public void RecordComparisonValueValue(ReadOnlySpan<int> state, int value0, int value1, int comparisonResult)
    {
        Console.WriteLine($"Comparison {_iterationCount++}: {value0} > {value1} = {comparisonResult}.");
    }
}

public class IntComparer : IComparer<int>
{
    public int Compare(int x, int y)
    {
        return x.CompareTo(y);
    }
}

enum ArrayInitializationKind
{
    Random,
    Console,
}

