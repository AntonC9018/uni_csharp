using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using lab1.Forms;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Laborator1;


public enum SelectionFilterKind
{
    Range,
    Arbitrary,
}

public sealed class SelectionFilterFactory : IKeyedProvider<SelectionFilterKind, ISelectionFilter>
{
    private readonly SelectionFilterKind[] _keys;
    private readonly IGetter<RangeSelectionFilterModel> _rangeModel;
    private readonly IGetter<ArbitrarySelectionFilterModel> _arbitraryModel;
    
    public SelectionFilterFactory(
        IGetter<RangeSelectionFilterModel> rangeModel,
        IGetter<ArbitrarySelectionFilterModel> arbitraryModel)
    {
        _rangeModel = rangeModel;
        _arbitraryModel = arbitraryModel;
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
                return new ArbitrarySelectionFilter(_arbitraryModel.GetRequired());
            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }
}

public sealed class RangeSelectionFilter : ISelectionFilter
{
    private readonly RangeSelectionFilterModel _model;
    private RangeSelectionFilterViewModel _viewModel;

    public RangeSelectionFilter(RangeSelectionFilterModel model)
    {
        _model = model;
    }
    
    public void EnableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        var vm = new RangeSelectionFilterViewModel(_model);
        var view = new RangeSelectionFilterUserControl(vm);
        viewport.Children.Add(view);
        _viewModel = vm;
    }
    
    public void DisableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        viewport.Children.Clear();
        _viewModel.Dispose();
    }

    public IEnumerable<int> GetEnabledIndices()
    {
        for (int i = _model.From; i <= _model.To; i++)
            yield return i;
    }
}

public class ArbitrarySelectionFilterModel
{
    private ObservableCollection<bool> _checkedStates = new();

    public ArbitrarySelectionFilterModel(IObservableValue<int> itemCount)
    {
        itemCount.ValueChanged += OnItemCountChanged;
    }
    
    private void OnItemCountChanged(int v)
    {
        while (_checkedStates.Count < v)
            _checkedStates.Add(true);
        while (_checkedStates.Count > v)
            _checkedStates.RemoveAt(_checkedStates.Count - 1);
    }
    
    public IEnumerable<int> SelectedIndices
    {
        get
        {
            for (int i = 0; i < _checkedStates.Count; i++)
            {
                if (_checkedStates[i])
                    yield return i;
            }
        }
    }
    
    public ObservableCollection<bool> CheckedStates => _checkedStates;
}

public sealed class ArbitrarySelectionFilter : ISelectionFilter
{
    private readonly ArbitrarySelectionFilterModel _model;

    public ArbitrarySelectionFilter(ArbitrarySelectionFilterModel model)
    {
        _model = model;
    }

    public void EnableUi(Panel viewport, FrameworkElement itemsRoot)
    {
    }
    
    public void DisableUi(Panel viewport, FrameworkElement itemsRoot)
    {
    }

    public IEnumerable<int> GetEnabledIndices() => _model.SelectedIndices;
}

public class ArbitrarySelectionFilterView
{
    private ArbitrarySelectionFilterModel _model;
    private FrameworkElement _itemsRoot;

    public ArbitrarySelectionFilterView(ArbitrarySelectionFilterModel model, ItemsControl items)
    {
        _model = model;
    }

    public IEnumerable<bool> CheckedStates => _model.CheckedStates;

    private void OnItemChanged()
    {
    }
}

public interface IListItemSwapper
{
    void Swap(int index0, int index1);
}

public sealed class SortDisplay : ISortDisplay
{
    // The 
    private readonly IListItemSwapper _swapper;
    private readonly TimeSpan _animationDelay;

    public SortDisplay(IListItemSwapper swapper, TimeSpan animationDelay)
    {
        _swapper = swapper;
        _animationDelay = animationDelay;
    }

    public async Task BeginSwap(int index0, int index1)
    {
        await Task.Delay(_animationDelay);
    }

    public Task EndSwap(int index0, int index1)
    {
        _swapper.Swap(index0, index1);
        return Task.CompletedTask;
    }

    public Task RecordComparison(int index0, int index1, int comparisonResult)
    {
        return Task.CompletedTask;
    }

    public Task RecordIteration()
    {
        return Task.CompletedTask;
    }

    public Task Reset()
    {
        return Task.CompletedTask;
    }
}