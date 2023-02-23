using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Laborator1;

public class ArbitrarySelectionFilterModel
{
    private readonly ObservableCollection<bool> _checkedStates = new();

    public ArbitrarySelectionFilterModel(IChangedEvent<int> itemCount)
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