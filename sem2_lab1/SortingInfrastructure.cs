using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Laborator1;


public struct SortingContext<T>
{
    public ISortDisplay SortDisplay;
    public Memory<T> Items;
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
    void EnableUI(Panel viewport);
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

public class SortingAlgorithmFactory : IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm>
{
    public static SortingAlgorithmFactory Instance { get; } = new SortingAlgorithmFactory();

    private readonly SortingAlgorithmKind[] _keys;
    private readonly ISortingAlgorithm[] _algorithms;
    
    private SortingAlgorithmFactory()
    {
        _keys = (SortingAlgorithmKind[]) Enum.GetValues(typeof(SortingAlgorithmKind));
        _algorithms = new ISortingAlgorithm[_keys.Length];
        _algorithms[(int) SortingAlgorithmKind.Quick] = new QuickSortAlgorithm();
        _algorithms[(int) SortingAlgorithmKind.Heap] = new HeapSortAlgorithm();
        _algorithms[(int) SortingAlgorithmKind.Selection] = new SelectionSortAlgorithm();
        Debug.Assert(_algorithms.All(x => x != null));
    }

    public IEnumerable<SortingAlgorithmKind> GetKeys() => _keys;
    public ISortingAlgorithm GetService(SortingAlgorithmKind key) => _algorithms[(int) key];
}

public class QuickSortAlgorithm : ISortingAlgorithm
{
    public Task Sort<T>(SortingContext<T> context)
    {
        throw new NotImplementedException();
    }
}

public class HeapSortAlgorithm : ISortingAlgorithm
{
    public Task Sort<T>(SortingContext<T> context)
    {
        throw new NotImplementedException();
    }
}

public class SelectionSortAlgorithm : ISortingAlgorithm
{
    public Task Sort<T>(SortingContext<T> context)
    {
        throw new NotImplementedException();
    }
}

public enum SelectionFilterKind
{
    Range,
    Arbitrary,
}

public class SelectionFilterFactory : IKeyedProvider<SelectionFilterKind, ISelectionFilter>
{
    private readonly SelectionFilterKind[] _keys;
    
    public SelectionFilterFactory()
    {
        _keys = (SelectionFilterKind[]) Enum.GetValues(typeof(SelectionFilterKind));
    }

    public IEnumerable<SelectionFilterKind> GetKeys() => _keys;
    public ISelectionFilter GetService(SelectionFilterKind key)
    {
        switch (key)
        {
            case SelectionFilterKind.Range:
                return new RangeSelectionFilter();
            case SelectionFilterKind.Arbitrary:
                return new ArbitrarySelectionFilter();
            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }
}

public class RangeSelectionFilter : ISelectionFilter
{
    public void EnableUI(Panel viewport)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<int> GetEnabledIndices()
    {
        throw new NotImplementedException();
    }
}

public class ArbitrarySelectionFilter : ISelectionFilter
{
    public void EnableUI(Panel viewport)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<int> GetEnabledIndices()
    {
        throw new NotImplementedException();
    }
}

public class SortDisplay : ISortDisplay
{
    public Task BeginSwap(int index0, int index1)
    {
        throw new NotImplementedException();
    }

    public Task EndSwap(int index0, int index1)
    {
        throw new NotImplementedException();
    }

    public Task RecordComparison(int index0, int index1, int comparisonResult)
    {
        throw new NotImplementedException();
    }

    public Task RecordIteration()
    {
        throw new NotImplementedException();
    }

    public Task Reset()
    {
        throw new NotImplementedException();
    }
}