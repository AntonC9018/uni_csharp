using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laborator1;

public interface ISortingService
{
    Task StartSorting();
}

// Used to invoke the sorting algorithm
public sealed class SortingService<T> : ISortingService
{
    private readonly ItemsData<T> _items;
    private readonly IGetter<ISortingAlgorithm> _algorithm;
    private readonly IGetter<ISortDisplay> _sortDisplay;
    private readonly IGetter<ISelectionFilter> _selectionFilter;
    private readonly IComparer<T> _comparer;

    public SortingService(
        ItemsData<T> items,
        IGetter<ISortingAlgorithm> algorithm,
        IGetter<ISortDisplay> sortDisplay,
        IGetter<ISelectionFilter> selectionFilter,
        IComparer<T> comparer)
    {
        _items = items;
        _algorithm = algorithm;
        _sortDisplay = sortDisplay;
        _selectionFilter = selectionFilter;
        _comparer = comparer;
    }

    public Task StartSorting()
    {
        var selectionFilter = _selectionFilter.Get();
        var sortDisplay = _sortDisplay.GetRequired();
        var items = _items.Items;
        var algorithm = _algorithm.GetRequired();

        IList<T?> itemsCopy;
        if (selectionFilter is not null)
        {
            var indices = selectionFilter.GetEnabledIndices().ToArray();
            var filteredItems = indices.Select(i => items[i]).ToArray();
            
            // The sort display has to be able to remap the indices.
            // This is because the sorting algorithm only sees the filtered items.
            // We do this by wrapping the display in a remapping display.
            var remappingDisplay = new RemappingSortDisplay(sortDisplay, indices);
            sortDisplay = remappingDisplay;
            itemsCopy = filteredItems;
        }
        else
        {
            itemsCopy = items.ToArray();
        }

        var context = new SortingContext<T>
        {
            Display = sortDisplay,
            Items = itemsCopy,
            Comparer = _comparer,
        };
        
        return algorithm.Sort(context);
    }
}

public sealed class RemappingSortDisplay : ISortDisplay
{
    private readonly ISortDisplay _display;
    private readonly IReadOnlyList<int> _indices;

    public RemappingSortDisplay(ISortDisplay display, IReadOnlyList<int> indices)
    {
        _display = display;
        _indices = indices;
    }

    public Task Reset() => _display.Reset();
    public Task RecordIteration() => _display.RecordIteration();
    public Task RecordComparison(int index0, int index1, int comparisonResult)
        => _display.RecordComparison(_indices[index0], _indices[index1], comparisonResult);
    public Task BeginSwap(int index0, int index1)
        => _display.BeginSwap(_indices[index0], _indices[index1]);
    public Task EndSwap(int index0, int index1)
        => _display.EndSwap(_indices[index0], _indices[index1]);
}