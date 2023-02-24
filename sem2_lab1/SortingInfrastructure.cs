using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Laborator1;


public struct SortingContext<T>
{
    public ISortDisplay Display;
    public IList<T?> Items;
    public IComparer<T> Comparer;
    public CancellationToken CancellationToken;
}

public interface ISortingAlgorithm
{
    Task Sort<T>(SortingContext<T> context);
}

public interface ISortDisplay
{
    Task Reset(CancellationToken token);
    Task RecordIteration(CancellationToken token);
    Task RecordComparison(int index0, int index1, int comparisonResult, CancellationToken token);
    Task Swap(int index0, int index1, CancellationToken token);
}

public interface ISelectionFilter
{
    void EnableUi(Panel viewport, ItemsControl itemsRoot);
    void DisableUi(Panel viewport, ItemsControl itemsRoot);
    IEnumerable<int> GetEnabledIndices();
}

public interface IKeyedProvider<TKey, out TService>
{
    IEnumerable<TKey> GetKeys();
    TService GetService(TKey key);
}

public enum SortingAlgorithmKind
{
    Quick,
    Heap,
    Selection,
}

public sealed class SortingAlgorithmFactory : IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm>
{
    public static SortingAlgorithmFactory Instance { get; } = new SortingAlgorithmFactory();

    private readonly SortingAlgorithmKind[] _keys;
    private readonly ISortingAlgorithm[] _algorithms;
    
    private SortingAlgorithmFactory()
    {
        _keys = (SortingAlgorithmKind[]) Enum.GetValues(typeof(SortingAlgorithmKind));
        _algorithms = new ISortingAlgorithm[_keys.Length];
        _algorithms[(int) SortingAlgorithmKind.Quick] = QuickSortAlgorithm.Instance;
        _algorithms[(int) SortingAlgorithmKind.Heap] = HeapSortAlgorithm.Instance;
        _algorithms[(int) SortingAlgorithmKind.Selection] = SelectionSortAlgorithm.Instance;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Debug.Assert(_algorithms.All(x => x is not null));
    }

    public IEnumerable<SortingAlgorithmKind> GetKeys() => _keys;
    public ISortingAlgorithm GetService(SortingAlgorithmKind key) => _algorithms[(int) key];
}

public sealed class QuickSortAlgorithm : ISortingAlgorithm
{
    public static QuickSortAlgorithm Instance { get; } = new();

    public async Task Sort<T>(SortingContext<T> context)
    {
        await SortingImplementations.QuickSort(context);
    }
}

public sealed class HeapSortAlgorithm : ISortingAlgorithm
{
    public static HeapSortAlgorithm Instance { get; } = new();

    public async Task Sort<T>(SortingContext<T> context)
    {
        await SortingImplementations.HeapSort(context);
    }
}

public sealed class SelectionSortAlgorithm : ISortingAlgorithm
{
    public static SelectionSortAlgorithm Instance { get; } = new();

    public async Task Sort<T>(SortingContext<T> context)
    {
        await SortingImplementations.SelectionSort(context);
    }
}