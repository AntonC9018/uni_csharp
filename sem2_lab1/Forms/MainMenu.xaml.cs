using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Laborator1;

public struct MainMenuModelData
{
    public ISortingAlgorithm? SortingAlgorithm;
    public SortingAlgorithmKind? SortingAlgorithmKind; 
    public ISortDisplay? SortDisplay;
    public ISelectionFilter? SelectionFilter;
    public SelectionFilterKind? SelectionFilterKind;
    public IItems? Items;
    public IItemRandomizer? ItemRandomizer;
    public IDirectionalComparer? DirectionalComparerDecorator;
    public SortDirection SortDirection;
}

public enum SortDirection
{
    Ascending,
    Descending,
}

public interface IDirectionalComparer
{
    SortDirection Direction { get; set; }
}

public class DirectionalComparerDecorator<T> : IComparer<T>, IDirectionalComparer
{
    public DirectionalComparerDecorator(IComparer<T> comparer)
    {
        Comparer = comparer;
    }

    public IComparer<T> Comparer { get; set; }
    public SortDirection Direction { get; set; }

    public int Compare(T? x, T? y)
    {
        var result = Comparer.Compare(x, y);
        return Direction == SortDirection.Ascending ? result : -result;
    }
}

public interface IItems
{
    void InvokeSort(ISortingAlgorithm algorithm, ISortDisplay sortDisplay);
    void ResizeArray(int newSize);
    System.Type ElementType { get; }
    IList List { get; }
}

public interface IItemRandomizer
{
    void Randomize();
}

public abstract class ItemsRandomizerBase<T> : IItemRandomizer
{
    protected readonly ItemsData<T> _items;

    protected ItemsRandomizerBase(ItemsData<T> items)
    {
        _items = items;
    }

    public virtual void Randomize()
    {
        var items = _items.Items;
        for (var i = 0; i < items.Length; i++)
            RandomizeItem(ref items[i], i);
    }

    protected abstract void RandomizeItem(ref T currentItem, int index);
}

public class IntItemsRandomizer : ItemsRandomizerBase<int>
{
    private readonly Random _random = new();

    public IntItemsRandomizer(ItemsData<int> items) : base(items)
    {
    }
    
    protected override void RandomizeItem(ref int currentItem, int index)
    {
        currentItem = _random.Next();
    }
}

public class StringItemsRandomizer : ItemsRandomizerBase<string>
{
    private static readonly string[] _Strings = new[]
    {
        "big",
        "alpha",
        "beta",
        "dinosaur",
        "elephant",
        "fox",
        "giraffe",
        "horse",
        "dog",
        "cat",
        "house",
        "car",
        "plane",
        "train",
        "boat",
        "ship",
        "truck",
        "ticket",
        "Earth",
        "Mars",
        "Saturn",
        "Jupiter",
        "Uranus",
        "Neptune",
        "bitcoin",
        "ethereum",
        "youtube",
        "facebook",
        "instagram",
        "twitter",
        "tiktok",
    };
    private readonly Random _random = new();

    public StringItemsRandomizer(ItemsData<string> items) : base(items)
    {
    }

    protected override void RandomizeItem(ref string currentItem, int index)
    {
        currentItem = _Strings[_random.NextInt64(0, _Strings.LongLength)].ToString();
    }
}

public class FloatItemsRandomizer : ItemsRandomizerBase<float>
{
    private readonly Random _random = new();

    public FloatItemsRandomizer(ItemsData<float> items) : base(items)
    {
    }

    protected override void RandomizeItem(ref float currentItem, int index)
    {
        currentItem = (float) _random.NextDouble();
    }
}

public class ItemsData<T> : IItems
{
    public T[] Items { get; set; }
    public IComparer<T> Comparer { get; set; }

    public ItemsData(T[] items, IComparer<T> comparer)
    {
        Items = items;
        Comparer = comparer;
    }

    public void InvokeSort(ISortingAlgorithm algorithm, ISortDisplay sortDisplay)
    {
        var context = new SortingContext<T>
        {
            SortDisplay = sortDisplay,
            Items = Items.AsMemory(),
            Comparer = Comparer,
        };
    }

    public void ResizeArray(int newSize)
    {
        var it = Items;
        Array.Resize(ref it, newSize);
        Items = it;
    }

    IList IItems.List => Items;
    System.Type IItems.ElementType => typeof(T);
}

public static class InvokeSortHelper
{
    public static void InvokeSort<T>(this ISortingAlgorithm algorithm, System.Type t, SortingContext<T> context)
    {
        algorithm.Sort(context);
    }
}

public class MainMenuModel : ObservableObject
{
    private MainMenuModelData _data;

    public MainMenuModel(MainMenuModelData data)
    {
        _data = data;
    }
    
    public ISortingAlgorithm? SortingAlgorithm
    {
        get => _data.SortingAlgorithm;
        // set => SetProperty(ref _data.SortingAlgorithm, value);
    }

    public ISelectionFilter? SelectionFilter
    {
        get => _data.SelectionFilter;
        // set => SetProperty(ref _data.SelectionFilter, value);
    }

    public void SetSortingAlgorithm((SortingAlgorithmKind kind, ISortingAlgorithm algorithm)? t)
    {
        _data.SortingAlgorithmKind = t?.kind ?? null;
        _data.SortingAlgorithm = t?.algorithm ?? null;
        OnPropertyChanged(nameof(SortingAlgorithmKind));
        OnPropertyChanged(nameof(SortingAlgorithm));
    }

    public ISortDisplay? SortDisplay
    {
        get => _data.SortDisplay;
        set => SetProperty(ref _data.SortDisplay, value);
    }

    public SortingAlgorithmKind? SortingAlgorithmKind
    {
        get => _data.SortingAlgorithmKind;
        // set => SetProperty(ref _data.SortingAlgorithmKind, value);
    }

    public SelectionFilterKind? SelectionFilterKind
    {
        get => _data.SelectionFilterKind;
        // set => SetProperty(ref _data.SelectionFilterKind, value);
    }

    public void SetSelectionFilter((SelectionFilterKind kind, ISelectionFilter filter)? t)
    {
        _data.SelectionFilterKind = t?.kind ?? null;
        _data.SelectionFilter = t?.filter ?? null;
        OnPropertyChanged(nameof(SelectionFilterKind));
        OnPropertyChanged(nameof(SelectionFilter));
    }

    public IItems? Items
    {
        get => _data.Items;
        // set => SetProperty(ref _data.Items, value);
    }

    public IItemRandomizer? Randomizer
    {
        get => _data.ItemRandomizer;
        // set => SetProperty(ref _data.Randomizer, value);
    }
    
    public void SetItems((IItems items, IItemRandomizer randomizer, IDirectionalComparer comparer)? t)
    {
        _data.Items = t?.items ?? null;
        _data.ItemRandomizer = t?.randomizer ?? null;
        _data.DirectionalComparerDecorator = t?.comparer ?? null;
        if (t.HasValue)
            t.Value.comparer.Direction = _data.SortDirection;

        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(Randomizer));
    }

    public SortDirection SortDirection
    {
        get => _data.SortDirection;
        set
        {
            SetProperty(ref _data.SortDirection, value);
            if (_data.DirectionalComparerDecorator is not null)
                _data.DirectionalComparerDecorator.Direction = value;
        }
    }
}

public class MainMenuService
{
    public MainMenuModel Model { get; }
    public IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm> SortingAlgorithmFactory { get; }
    public IKeyedProvider<SelectionFilterKind, ISelectionFilter> SelectionFilterFactory { get; }

    public MainMenuService(
        MainMenuModel model,
        IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm> sortingAlgorithmFactory,
        IKeyedProvider<SelectionFilterKind, ISelectionFilter> selectionFilterFactory)
    {
        SortingAlgorithmFactory = sortingAlgorithmFactory;
        SelectionFilterFactory = selectionFilterFactory;
        Model = model;
    }

    public void SelectAlgorithm(SortingAlgorithmKind? kind)
    {
        if (kind is null)
            ResetAlgorithm();
        else
            SelectAlgorithm(kind.Value);
    }

    public void SelectAlgorithm(SortingAlgorithmKind kind)
    {
        var factory = SortingAlgorithmFactory.GetService(kind);
        Model.SetSortingAlgorithm((kind, factory));
    }

    public void ResetAlgorithm()
    {
        Model.SetSortingAlgorithm(null);
    }

    public void SelectSelectionFilter(SelectionFilterKind? kind)
    {
        if (kind is null)
            ResetSelectionFilter();
        else
            SelectFilter(kind.Value);
    }

    public void SelectFilter(SelectionFilterKind kind)
    {
        var factory = SelectionFilterFactory.GetService(kind);
        Model.SetSelectionFilter((kind, factory));
    }

    public void ResetSelectionFilter()
    {
        Model.SetSelectionFilter(null);
    }

    public void SelectItemCount(int count)
    {
        var items = Model.Items;
        if (items is null)
            return;
        items.ResizeArray(count);
        Model.Randomizer?.Randomize();
    }

    public void SelectItemType(ItemType? itemType)
    {
        if (itemType is null)
            ResetItemType();
        else
            SelectItemType(itemType.Value);
    }

    public void SelectItemType(ItemType itemType)
    {
        var currentType = Model.Items?.ElementType;
        const int defaultItemCount = 10;
        var itemCount = Model.Items?.List.Count ?? defaultItemCount;

        switch (itemType)
        {
            case ItemType.Int:
            {
                if (currentType == typeof(int))
                    return;
                var comparer = new DirectionalComparerDecorator<int>(Comparer<int>.Default);
                var items = new ItemsData<int>(new int[itemCount], comparer);
                var randomizer = new IntItemsRandomizer(items);
                Model.SetItems((items, randomizer, comparer));
                break;
            }
            case ItemType.String:
            {
                if (currentType == typeof(string))
                    return;
                var comparer = new DirectionalComparerDecorator<string>(Comparer<string>.Default);
                var items = new ItemsData<string>(new string[itemCount], comparer);
                var randomizer = new StringItemsRandomizer(items);
                Model.SetItems((items, randomizer, comparer));
                break;
            }
            case ItemType.Float:
            {
                if (currentType == typeof(float))
                    return;
                var comparer = new DirectionalComparerDecorator<float>(Comparer<float>.Default);
                var items = new ItemsData<float>(new float[itemCount], comparer);
                var randomizer = new FloatItemsRandomizer(items);
                Model.SetItems((items, randomizer, comparer));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
    }

    public void ResetItemType()
    {
        Model.SetItems(null);
    }
}

public enum ItemType
{
    Int,
    String,
    Float,
}

public class MainMenuViewModel : ObservableObject
{
    private MainMenuModel _model;
    private MainMenuService _service;

    public MainMenuViewModel(
        MainMenuModel model,
        MainMenuService service)
    {
        _model = model;
        _service = service;
        
        model.PropertyChanged += (_, context) =>
        {
            switch (context.PropertyName)
            {
                default:
                    OnPropertyChanged(context);
                    return;

                case nameof(MainMenuModel.SortingAlgorithmKind):
                    OnPropertyChanged(nameof(AlgorithmName));
                    // How do I check when this actually changes?
                    OnPropertyChanged(nameof(IsAlgorithmSelected));
                    OnPropertyChanged(nameof(AlgorithmKind));
                    return;

                case nameof(MainMenuModel.SelectionFilterKind):
                    OnPropertyChanged(nameof(FilterName));
                    // How do I check when this actually changes?
                    OnPropertyChanged(nameof(IsFilterSelected));
                    OnPropertyChanged(nameof(SelectionFilterKind));
                    return;
            }
        };
    }

    public bool StatusBarVisibility => true;
    
    public string AlgorithmName => _model.SortingAlgorithmKind?.ToString() ?? "None";
    public IEnumerable<SortingAlgorithmKind> AlgorithmKinds => _service.SortingAlgorithmFactory.GetKeys();
    public bool IsAlgorithmSelected => _model.SortingAlgorithmKind is not null;
    public SortingAlgorithmKind? AlgorithmKind
    {
        get => _model.SortingAlgorithmKind;
        set => _service.SelectAlgorithm(value);
    }
    public IEnumerable<SelectionFilterKind> SelectionFilterKinds => _service.SelectionFilterFactory.GetKeys();
    public string FilterName => _model.SelectionFilterKind?.ToString() ?? "None";
    public bool IsFilterSelected => _model.SelectionFilterKind is not null;
    public SelectionFilterKind? SelectionFilterKind
    {
        get => _model.SelectionFilterKind;
        set => _service.SelectSelectionFilter(value);
    }
}


public partial class MainMenu : Window
{
    public MainMenuViewModel ViewModel { get; }

    public MainMenu(MainMenuViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void InitializeComponent()
    {

    }

    public void SelectAlgorithm(object sender, RoutedEventArgs e)
    {
    }
}