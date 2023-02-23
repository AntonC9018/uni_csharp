using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using lab1.Forms;

namespace Laborator1;


public struct SortingContext<T>
{
    public ISortDisplay Display;
    public IList<T?> Items;
    public IComparer<T> Comparer;
}

public interface ISortingAlgorithm
{
    Task Sort<T>(SortingContext<T> context);
}

public interface ISortDisplay
{
    Task Reset();
    Task RecordIteration();
    Task RecordComparison(int index0, int index1, int comparisonResult);
    Task BeginSwap(int index0, int index1);
    Task EndSwap(int index0, int index1);
}

public interface ISelectionFilter
{
    void EnableUi(Panel viewport, FrameworkElement itemsRoot);
    void DisableUi(Panel viewport, FrameworkElement itemsRoot);
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