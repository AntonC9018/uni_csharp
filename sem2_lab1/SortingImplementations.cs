using System.Threading.Tasks;

namespace Laborator1;

public static class SortingImplementations
{
    private sealed class _InternalSortContext<T>
    {
        public SortingContext<T> Context;

        public ListSegment<T?> WholeSpan => new(Context.Items);
        public int StartIndex;
        public int SpanLength;
        public ListSegment<T?> Span => WholeSpan.Slice(StartIndex, SpanLength);

        public _InternalSortContext(SortingContext<T> context)
        {
            Context = context;
            SpanLength = WholeSpan.Length;
            StartIndex = 0;
        }

        public Task StartIteration()
        {
            return Context.Display.RecordIteration();
        }

        public Task SwapLocal(int localIndex0, int localIndex1)
        {
            return Swap(localIndex0 + StartIndex, localIndex1 + StartIndex);
        }

        public Task<int> CompareLocal(int localIndex0, int localIndex1)
        {
            return Compare(localIndex0 + StartIndex, localIndex1 + StartIndex);
        }

        public async Task Swap(int index0, int index1)
        {
            await Context.Display.BeginSwap(index0, index1);
            WholeSpan.Swap(index0, index1);
            await Context.Display.EndSwap(index0, index1);
        }

        public async Task<int> Compare(int index0, int index1)
        {
            int comparisonResult = Context.Comparer.Compare(WholeSpan[index0], WholeSpan[index1]);
            await Context.Display.RecordComparison(index0, index1, comparisonResult);
            return comparisonResult;
        }
    } 

    private static async Task<int> quick_partition<T>(_InternalSortContext<T> context)
    {
        var lastIndex = context.Span.Length - 1;
        var pivotIndex = 0;

        for (int leftIndex = 0; leftIndex < lastIndex; leftIndex++)
        {
            if (await context.CompareLocal(leftIndex, lastIndex) < 0)
            {   
                await context.SwapLocal(pivotIndex, leftIndex);
                pivotIndex++;
            }
        }

        await context.SwapLocal(context.Span.Length - 1, pivotIndex);

        return pivotIndex;
    }

    private static async Task quick_sort<T>(_InternalSortContext<T> context)
    {
        if (context.SpanLength <= 1)
            return;

        await context.StartIteration();

        int partitionIndex = await quick_partition(context);

        int ownStart = context.StartIndex;
        int ownLength = context.SpanLength;
        {
            context.StartIndex = ownStart;
            context.SpanLength = partitionIndex;
            await quick_sort(context);
        }
        {
            context.StartIndex = ownStart + partitionIndex + 1;
            context.SpanLength = ownLength - (partitionIndex + 1);
            await quick_sort(context);
        }
    }

    public static Task QuickSort<T>(SortingContext<T> context)
    {
        return quick_sort<T>(new(context));
    }

    // Interprets SpanLength as the length to consider,
    // and the StartIndex as the current index.
    // So doing `.Span` is invalid in this function.
    private static async Task heapify<T>(_InternalSortContext<T> context)
    {
        await context.StartIteration();

        int indexLeft = context.StartIndex * 2 + 1;
        int indexRight = context.StartIndex * 2 + 2;
        int indexLargest = context.StartIndex;

        if (indexLeft < context.SpanLength)
        {
            if (await context.Compare(indexLeft, indexLargest) > 0)
                indexLargest = indexLeft;
        }

        if (indexRight < context.SpanLength)
        {
            if (await context.Compare(indexRight, indexLargest) > 0)
                indexLargest = indexRight;
        }

        if (indexLargest != context.StartIndex)
        {
            await context.Swap(context.StartIndex, indexLargest);
            context.StartIndex = indexLargest;
            await heapify(context);
        }
    }

    
    private static async Task heap_sort_internal<T>(_InternalSortContext<T> context)
    {
        int size = context.SpanLength;
        for (int i = size / 2; i > 0; i--)
        {
            context.StartIndex = i - 1;
            await heapify(context);
        }

        for (int i = size - 1; i > 0; i--)
        {
            await context.Swap(0, i);
            context.StartIndex = 0;
            context.SpanLength = i;
            await heapify(context);
        }
    }

    public static Task HeapSort<T>(SortingContext<T> context)
    {
        return heap_sort_internal(new _InternalSortContext<T>(context)); 
    }

    public static async Task SelectionSort<T>(SortingContext<T> context)
    {
        var ctx = new _InternalSortContext<T>(context);
        while (ctx.SpanLength != 0)
        {
            // find the minimum
            int minIndex = 0;
            for (int i = 1; i < ctx.SpanLength; i++)
            {
                if (await ctx.CompareLocal(i, minIndex) < 0)
                    minIndex = i;
            }

            await ctx.SwapLocal(minIndex, 0);

            ctx.StartIndex++;
            ctx.SpanLength--;
        }
    }
}