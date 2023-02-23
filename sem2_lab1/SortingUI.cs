﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using lab1.Forms;

namespace Laborator1;


public enum SelectionFilterKind
{
    None,
    Range,
    Arbitrary,
}

public sealed class SelectionFilterFactory : IKeyedProvider<SelectionFilterKind, ISelectionFilter>
{
    private readonly SelectionFilterKind[] _keys;
    private readonly IGetter<RangeSelectionFilterModel> _rangeModel;
    private readonly IGetter<ArbitrarySelectionFilterModel> _arbitraryModel;
    private readonly ISortingUiEventsProvider _eventsProvider;
    
    public SelectionFilterFactory(
        IGetter<RangeSelectionFilterModel> rangeModel,
        IGetter<ArbitrarySelectionFilterModel> arbitraryModel,
        ISortingUiEventsProvider eventsProvider)
    {
        _rangeModel = rangeModel;
        _arbitraryModel = arbitraryModel;
        _eventsProvider = eventsProvider;
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
                return new ArbitrarySelectionFilter(_arbitraryModel.GetRequired(), _eventsProvider);
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

public interface ISortingUiEventsProvider
{
    event NotifyCollectionChangedEventHandler? ItemsUIChanged;
    void Bind(INotifyCollectionChanged items);
}

public class SortingUiEventsProvider : ISortingUiEventsProvider
{
    public event NotifyCollectionChangedEventHandler? ItemsUIChanged;

    private NotifyCollectionChangedEventHandler _handler;

    public SortingUiEventsProvider()
    {
        _handler = OnItemsUIChanged;
    }
    
    public void OnItemsUIChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ItemsUIChanged?.Invoke(this, e);
    }
    
    // Effectively moves the event handler to the end of the invocation list.
    public void Bind(INotifyCollectionChanged items)
    {
        items.CollectionChanged += _handler;
    }
}

public sealed class ArbitrarySelectionFilter : ISelectionFilter
{
    private readonly ArbitrarySelectionFilterModel _model;
    private readonly ISortingUiEventsProvider _eventsProvider;
    private FrameworkElement? _itemsRoot;

    public ArbitrarySelectionFilter(ArbitrarySelectionFilterModel model, ISortingUiEventsProvider eventsProvider)
    {
        _model = model;
        _eventsProvider = eventsProvider;
    }

    public void EnableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        _itemsRoot = itemsRoot;
        _eventsProvider.ItemsUIChanged += OnItemsUIChanged;
    }
    
    public void DisableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        _eventsProvider.ItemsUIChanged -= OnItemsUIChanged;
    }
    
    private void OnItemsUIChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // If any element in the array has changed, then the corresponding control
        // in the ItemsControl has been changed. We have to find it and add a checkbox to it,
        // bound to the corresponding element in _model.CheckedStates.
        
        void AddCheckBox(int index)
        {
            var child = VisualTreeHelper.GetChild(_itemsRoot!, index);
            var childPanel = (StackPanel) child;
            
            var checkBox = new CheckBox();
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding($"[{index}]") {Source = _model.CheckedStates});
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.Margin = new Thickness(0, 0, 0, 0);
            checkBox.Width = 20;
            checkBox.Height = 20;
            // The position of the checkbox must be to the left of the itemControl.
            childPanel.Children.Insert(0, checkBox);
        }

        CheckBox GetCheckbox(int index)
        {
            var child = VisualTreeHelper.GetChild(_itemsRoot!, index);
            var childPanel = (StackPanel) child;
            var checkBox = childPanel.Children.OfType<CheckBox>().First();
            return checkBox;
        }

        void ResetCheckboxBinding(int atIndex, int toIndex)
        {
            var checkBox = GetCheckbox(atIndex);
            BindingOperations.ClearBinding(checkBox, ToggleButton.IsCheckedProperty);
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding($"[{toIndex}]") {Source = _model.CheckedStates});
        }
        
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                for (int i = 0; i < e.NewItems!.Count; i++)
                    AddCheckBox(i + e.NewStartingIndex);
                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {
                for (int i = 0; i < e.OldItems!.Count; i++)
                    _model.CheckedStates.RemoveAt(e.OldStartingIndex);
                for (int i = e.OldStartingIndex; i < _model.CheckedStates.Count; i++)
                    ResetCheckboxBinding(i, i);
                break;
            }
            case NotifyCollectionChangedAction.Replace:
            {
                break;
            }
            case NotifyCollectionChangedAction.Move:
            {
                // The order of the items in the ItemsControl has changed.
                // We have to move the corresponding checkboxes.
                var fromIndex = e.OldStartingIndex;
                var toIndex = e.NewStartingIndex;
                var count = e.NewItems!.Count;
                var checkBox = GetCheckbox(fromIndex);
                _model.CheckedStates.RemoveAt(fromIndex);
                _model.CheckedStates.Insert(toIndex, (bool) checkBox.IsChecked!);
                for (int i = 0; i < count; i++)
                    ResetCheckboxBinding(fromIndex + i, toIndex + i);
                break;
            }
            case NotifyCollectionChangedAction.Reset:
            {
                // ig all items are recreated in this case, so we have to recreate all checkboxes.
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_itemsRoot!); i++)
                    AddCheckBox(i);
                break;
            }
        }
    }

    public IEnumerable<int> GetEnabledIndices() => _model.SelectedIndices;
}

public interface IListItemSwapper
{
    void Swap(int index0, int index1);
}

public sealed class ListItemSwapper<T> : IListItemSwapper
{
    private readonly ItemsData<T> _items;
    public ListItemSwapper(ItemsData<T> items)
    {
        _items = items;
    }
    public void Swap(int index0, int index1)
    {
        var arr = _items.Items;
        (arr[index0], arr[index1]) = (arr[index1], arr[index0]);
    }
}

public sealed class SortDisplay : ISortDisplay
{
    private readonly IGetter<IListItemSwapper> _swapper;
    private readonly TimeSpan _animationDelay;

    public SortDisplay(IGetter<IListItemSwapper> swapper, TimeSpan animationDelay)
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
        _swapper.GetRequired().Swap(index0, index1);
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