using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    void EnableUi(Panel viewport);
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

public enum SelectionFilterKind
{
    Range,
    Arbitrary,
}

public sealed class SelectionFilterFactory : IKeyedProvider<SelectionFilterKind, ISelectionFilter>
{
    private readonly SelectionFilterKind[] _keys;
    private readonly IGetter<RangeSelectionFilterModel> _rangeModel;
    
    public SelectionFilterFactory(
        IGetter<RangeSelectionFilterModel> rangeModel)
    {
        _rangeModel = rangeModel;
        _keys = (SelectionFilterKind[]) Enum.GetValues(typeof(SelectionFilterKind));
    }

    public IEnumerable<SelectionFilterKind> GetKeys() => _keys;
    public ISelectionFilter GetService(SelectionFilterKind key)
    {
        switch (key)
        {
            case SelectionFilterKind.Range:
                return new RangeSelectionFilter(_rangeModel.GetRequired());
            case SelectionFilterKind.Arbitrary:
                return new ArbitrarySelectionFilter();
            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }
}

public sealed class RangeSelectionFilter : ISelectionFilter
{
    private RangeSelectionFilterModel _model;

    public RangeSelectionFilter(RangeSelectionFilterModel model)
    {
        _model = model;
    }
    
    public void EnableUi(Panel viewport)
    {
        var vm = new RangeSelectionFilterViewModel(_model);
        var view = new RangeSelectionFilterUserControl(vm);
        viewport.Children.Add(view);
    }

    public IEnumerable<int> GetEnabledIndices()
    {
        for (int i = _model.From; i <= _model.To; i++)
            yield return i;
    }
}

public sealed class ArbitrarySelectionFilter : ISelectionFilter
{
    public void EnableUi(Panel viewport)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<int> GetEnabledIndices()
    {
        throw new NotImplementedException();
    }
}

public sealed class SortDisplay : ISortDisplay
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