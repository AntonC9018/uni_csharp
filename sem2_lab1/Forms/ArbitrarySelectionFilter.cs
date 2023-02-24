using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Laborator1;

public class ArbitrarySelectionFilterModel
{
    private readonly BitArray _checkedStatesArray;
    private readonly ObservableCollection<bool> _checkedStates = new();

    public ArbitrarySelectionFilterModel(IChangedEvent<int> itemCount)
    {
        itemCount.ValueChanged += OnItemCountChanged;
        
        // In our case we know 100 is the max, so I can shortcut resizing this thing.
        _checkedStatesArray = new BitArray(100, true);
        
        // synchronize
        _checkedStates.CollectionChanged += (_, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    break;
                
                case NotifyCollectionChangedAction.Move:
                    throw new System.NotSupportedException();

                case NotifyCollectionChangedAction.Replace:
                {
                    for (int i = 0; i < e.NewItems!.Count; i++)
                        _checkedStatesArray[e.NewStartingIndex + i] = (bool) e.NewItems[i]!;
                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                    // might need to copy everything over?
                    break;
            }
        };
    }
    
    private void OnItemCountChanged(int v)
    {
        while (_checkedStates.Count < v)
            _checkedStates.Add(_checkedStatesArray[_checkedStates.Count]);
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