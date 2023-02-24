using System;
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

public sealed class ArbitrarySelectionFilter : ISelectionFilter
{
    private readonly ArbitrarySelectionFilterModel _model;
    private FrameworkElement? _itemsRoot;

    public ArbitrarySelectionFilter(ArbitrarySelectionFilterModel model)
    {
        _model = model;
    }

    public void EnableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        _itemsRoot = itemsRoot;
        ((ItemsControl) itemsRoot).ItemContainerGenerator.StatusChanged += OnItemsUIChanged;
    }
    
    public void DisableUi(Panel viewport, FrameworkElement itemsRoot)
    {
        ((ItemsControl) itemsRoot).ItemContainerGenerator.StatusChanged += OnItemsUIChanged;
    }
    
    private void OnItemsUIChanged(object? sender, EventArgs e)
    {
        // If any element in the array has changed, then the corresponding control
        // in the ItemsControl has been changed. We have to find it and add a checkbox to it,
        // bound to the corresponding element in _model.CheckedStates.

        void CreateCheckBox(StackPanel panel, int index)
        {
            var checkBox = new CheckBox();
            checkBox.HorizontalAlignment = HorizontalAlignment.Left;
            checkBox.VerticalAlignment = VerticalAlignment.Center;
            checkBox.Margin = new Thickness(0, 0, 0, 0);
            checkBox.Width = 20;
            checkBox.Height = 20;
            // The position of the checkbox must be to the left of the itemControl.
            panel.Children.Insert(0, checkBox);
            SetBinding(checkBox, index);
        }

        void SetBinding(CheckBox checkBox, int index)
        {
            var binding = new Binding($"[{index}]") {Source = _model.CheckedStates};
            checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);
        }
        
        var items = (ItemsControl) _itemsRoot!;
        var g = items.ItemContainerGenerator;
        if (g.Status != GeneratorStatus.ContainersGenerated)
            return;

        for (int i = 0; i < g.Items.Count; i++)
        {
            var child = (Visual) g.ContainerFromIndex(i);
            var stackPanel = child.SelfAndDescendants().OfType<StackPanel>().First();
            var checkBox = stackPanel.Descendats().OfType<CheckBox>().FirstOrDefault();
            if (checkBox is null)
            {
                CreateCheckBox(stackPanel, i);
                continue;
            }
            
            // Check if the binding property has the correct index
            var binding = (Binding) checkBox.GetBindingExpression(ToggleButton.IsCheckedProperty)!.ParentBinding;
            var pathParams = binding.Path.PathParameters;
            if (pathParams.Count > 0 && (int) pathParams[0] == i)
                continue;

            BindingOperations.ClearBinding(checkBox, ToggleButton.IsCheckedProperty);
            SetBinding(checkBox, i);
        }
    }

    public IEnumerable<int> GetEnabledIndices() => _model.SelectedIndices;
}

// http://drwpf.com/blog/2008/07/20/itemscontrol-g-is-for-generator/
public static class HierarchyHelper
{
    public static IEnumerable<Visual> SelfAndDescendants(this Visual element)
    {
        yield return element;
        foreach (var descendant in Descendats(element))
            yield return descendant;
    }
    public static IEnumerable<Visual> Descendats(this Visual element)
    {
        if (element is FrameworkElement felement)
            felement.ApplyTemplate();
        
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
        {
            var visual = (Visual) VisualTreeHelper.GetChild(element, i);
            yield return visual;
            foreach (var descendant in Descendats(visual))
                yield return descendant;
        }
    }
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