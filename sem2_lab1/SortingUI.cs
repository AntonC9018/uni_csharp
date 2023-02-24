using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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
    private RangeSelectionFilterViewModel? _viewModel;

    public RangeSelectionFilter(RangeSelectionFilterModel model)
    {
        _model = model;
    }
    
    public void EnableUi(Panel viewport, ItemsControl itemsRoot)
    {
        var vm = new RangeSelectionFilterViewModel(_model);
        var view = new RangeSelectionFilterUserControl(vm);
        viewport.Children.Add(view);
        _viewModel = vm;
    }
    
    public void DisableUi(Panel viewport, ItemsControl itemsRoot)
    {
        viewport.Children.Clear();
        _viewModel!.Dispose();
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
    private readonly EventHandler _itemsUiChangedHandler;
    private ItemContainerGenerator? _itemsRootContainerGenerator;

    public ArbitrarySelectionFilter(ArbitrarySelectionFilterModel model)
    {
        _model = model;
        _itemsUiChangedHandler = OnItemsUiChanged;
    }

    public void EnableUi(Panel viewport, ItemsControl itemsRoot)
    {
        _itemsRootContainerGenerator = itemsRoot.ItemContainerGenerator;
        itemsRoot.ItemContainerGenerator.StatusChanged += _itemsUiChangedHandler;
        _itemsRootContainerGenerator.CreateOrRebindFilterCheckboxes(_model);
    }
    
    public void DisableUi(Panel viewport, ItemsControl itemsRoot)
    {
        itemsRoot.ItemContainerGenerator.StatusChanged -= _itemsUiChangedHandler;
        _itemsRootContainerGenerator!.RemoveFilterCheckboxes();
    }
    
    public IEnumerable<int> GetEnabledIndices() => _model.SelectedIndices;

    private void OnItemsUiChanged(object? sender, EventArgs e)
    {
        // FIXME: very bad performance for larger collections.
        var g = _itemsRootContainerGenerator!;
        if (g.Status == GeneratorStatus.ContainersGenerated)
            g.CreateOrRebindFilterCheckboxes(_model);
    }
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

    public async Task Swap(int index0, int index1, CancellationToken token)
    {
        await Task.Delay(_animationDelay, token);
        _swapper.GetRequired().Swap(index0, index1);
    }

    public Task RecordComparison(int index0, int index1, int comparisonResult, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task RecordIteration(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task Reset(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}