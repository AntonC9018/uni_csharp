using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Laborator1;

public struct MainMenuModelData
{
    // Sorting algorithms are generic.
    public IObservableRepo<ISortingAlgorithm>? SortingAlgorithmProvider;
    public SortingAlgorithmKind? SortingAlgorithmKind; 
    
    public IObservableRepo<ISortDisplay>? SortDisplayProvider;
    
    // Selection filters work with indices.
    public IObservableRepo<ISelectionFilter>? SelectionFilterProvider;
    public SelectionFilterKind? SelectionFilterKind;
    
    public IObservableRepo<IItems>? ItemsObservableValue;
    public ItemKind? ItemKind;
    
    // These play the role of delegates that have captured Items
    // These allow us to be able to do operations on Items without knowing the generic type of the items.
    // This helps us avoid casts and not have to pass around Items.
    public IItemRandomizer? ItemRandomizer;
    public ISortingService? SortingService;
    public IShuffle? Shuffle;
    
    public IDirectionalComparer? DirectionalComparerDecorator;
    public SortDirection SortDirection;
    
    public Task? SortingTask;

    public IObservableValue<int>? ItemCountObservableValue;
}

public class MainMenuModel : ObservableObject
{
    private MainMenuModelData _data;

    public MainMenuModel(
        IObservableRepo<ISortingAlgorithm> sortingAlgorithmProvider,
        IObservableRepo<ISortDisplay> sortDisplayProvider,
        IObservableRepo<ISelectionFilter> selectionFilterProvider,
        IObservableRepo<IItems> itemsObservableValue,
        IObservableValue<int> itemCountObservableValue)
    {
        _data = new()
        {
            SortingAlgorithmProvider = sortingAlgorithmProvider,
            SortDisplayProvider = sortDisplayProvider,
            SelectionFilterProvider = selectionFilterProvider,
            ItemsObservableValue = itemsObservableValue,
            ItemCountObservableValue = itemCountObservableValue,
        };
    }

    private IObservableRepo<ISortingAlgorithm> SortingAlgorithmObservableRepo => _data.SortingAlgorithmProvider!;
    private IObservableRepo<ISelectionFilter> SelectionFilterObservableRepo => _data.SelectionFilterProvider!;
    private IObservableRepo<ISortDisplay> SortDisplayObservableRepo => _data.SortDisplayProvider!;
    
    public IObservableValue<ISortingAlgorithm> SortingAlgorithmProvider => _data.SortingAlgorithmProvider!;
    public IObservableValue<ISelectionFilter> SelectionFilterProvider => _data.SelectionFilterProvider!;
    public IObservableValue<ISortDisplay> SortDisplayProvider => _data.SortDisplayProvider!;

    
    public ISortingAlgorithm? SortingAlgorithm
    {
        get => SortingAlgorithmProvider.Get();
        // set => SetProperty(ref _data.SortingAlgorithm, value);
    }

    public ISelectionFilter? SelectionFilter
    {
        get => SelectionFilterProvider.Get();
        // set => SetProperty(ref _data.SelectionFilter, value);
    }

    public void SetSortingAlgorithm((SortingAlgorithmKind kind, ISortingAlgorithm algorithm)? t)
    {
        if (t is not null)
        {
            var v = t.Value;
            _data.SortingAlgorithmKind = v.kind;
            SortingAlgorithmObservableRepo.Set(v.algorithm);
        }
        else
        {
            _data.SortingAlgorithmKind = null;
            SortingAlgorithmObservableRepo.Set(null);
        }
        OnPropertyChanged(nameof(SortingAlgorithmKind));
        OnPropertyChanged(nameof(SortingAlgorithm));
    }

    public ISortDisplay? SortDisplay
    {
        get => SortDisplayProvider.Get();
        set
        {
            if (SortDisplayProvider.Get() == value)
                return;
            SortDisplayObservableRepo.Set(value);
            OnPropertyChanged(nameof(SortDisplay));
        }
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
        SelectionFilterObservableRepo.Set(t?.filter ?? null);
        OnPropertyChanged(nameof(SelectionFilterKind));
        OnPropertyChanged(nameof(SelectionFilter));
    }

    public IItems? Items
    {
        get => _data.ItemsObservableValue!.Get();
        // set => SetProperty(ref _data.Items, value);
    }

    public IItemRandomizer? Randomizer
    {
        get => _data.ItemRandomizer;
        // set => SetProperty(ref _data.Randomizer, value);
    }
    
    public void SetItems((IItems items, IItemRandomizer randomizer, IDirectionalComparer comparer, IShuffle shuffle, ISortingService sortingService)? t)
    {
        if (t.HasValue)
        {
            var v = t.Value;
            _data.ItemsObservableValue!.Set(v.items);
            _data.ItemRandomizer = v.randomizer;
            _data.DirectionalComparerDecorator = v.comparer;
            _data.SortingService = v.sortingService;
            _data.Shuffle = v.shuffle;

            v.comparer.Direction = _data.SortDirection;
        }
        else
        {
            _data.ItemsObservableValue!.Set(null);
            _data.ItemRandomizer = null;
            _data.DirectionalComparerDecorator = null;
            _data.SortingService = null;
            _data.Shuffle = null;
        }

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

    public Task? SortingTask
    {
        get => _data.SortingTask;
        set => SetProperty(ref _data.SortingTask, value);
    }

    public ISortingService? SortingService
    {
        get => _data.SortingService;
    }

    public bool IsSorting
    {
        get => _data.SortingTask is null ? false : !_data.SortingTask.IsCompleted;
    }

    public void OnSortingStateChanged()
    {
        OnPropertyChanged(nameof(IsSorting));
    }

    public ItemKind? ItemKind
    {
        get => _data.ItemKind;
        set => SetProperty(ref _data.ItemKind, value);
    }

    public IShuffle? Shuffle
    {
        get => _data.Shuffle;
    }
    
    public IObservableValue<int> ItemCountObservableValue => _data.ItemCountObservableValue!;
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

    public void SelectItemKind(ItemKind? itemType)
    {
        if (itemType is null)
            ResetItemType();
        else
            SelectItemKind(itemType.Value);
    }

    public void SelectItemKind(ItemKind itemKind)
    {
        var currentType = Model.Items?.ElementType;
        const int defaultItemCount = 10;
        var itemCount = Model.Items?.List.Count ?? defaultItemCount;

        void SetItems<T>(Func<ItemsData<T>, IItemRandomizer<T>> createRandomizer)
        {
            var comparer = new DirectionalComparerDecorator<T>(Comparer<T>.Default);
            var items = new ItemsData<T>(new T[itemCount], comparer);
            var randomizer = createRandomizer(items);
            var shuffle = new Shuffler<T>(items);
            var service = new SortingService<T>(items, Model.SortingAlgorithmProvider, Model.SortDisplayProvider, Model.SelectionFilterProvider, comparer);
            Model.SetItems((items, randomizer, comparer, shuffle, service));
        }

        switch (itemKind)
        {
            case ItemKind.Int:
            {
                if (currentType == typeof(int))
                    return;
                SetItems<int>(items => new IntItemsRandomizer(items));
                break;
            }
            case ItemKind.String:
            {
                if (currentType == typeof(string))
                    return;
                SetItems<string>(items => new StringItemsRandomizer(items));
                break;
            }
            case ItemKind.Float:
            {
                if (currentType == typeof(float))
                    return;
                SetItems<float>(items => new FloatItemsRandomizer(items));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(itemKind), itemKind, null);
        }
    }

    public void ResetItemType()
    {
        Model.SetItems(null);
    }

    public bool IsSortingInProgress => Model.SortingTask is not null && !Model.SortingTask.IsCompleted;
    public bool IsSortingNotInProgress => Model.SortingTask is null || Model.SortingTask.IsCompleted;

    public async Task StartSorting()
    {
        Debug.Assert(IsSortingNotInProgress);

        var service = Model.SortingService;
        Debug.Assert(service is not null);

        var task = service.StartSorting();
        Model.SortingTask = task;
        Model.OnSortingStateChanged();

        try
        {
            await task;
        }
        finally
        {
            Model.OnSortingStateChanged();
        }
    }

    public void Randomize()
    {
        Debug.Assert(Model.Randomizer is not null);
        Model.Randomizer.Randomize();
    }

    public void Shuffle()
    {
        Debug.Assert(Model.Items is not null);
        Debug.Assert(Model.Shuffle is not null);
        Model.Shuffle.Shuffle();
    }
}

public enum ItemKind
{
    Int,
    String,
    Float,
}

public class MainMenuViewModel : ObservableObject
{
    private readonly MainMenuModel _model;
    private readonly MainMenuService _service;

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
                    OnPropertyChanged(nameof(CanStartSorting));
                    return;

                case nameof(MainMenuModel.SelectionFilterKind):
                    OnPropertyChanged(nameof(FilterName));
                    // How do I check when this actually changes?
                    OnPropertyChanged(nameof(IsFilterSelected));
                    OnPropertyChanged(nameof(SelectionFilterKind));
                    return;

                case nameof(MainMenuModel.IsSorting):
                    OnPropertyChanged(nameof(IsSortingInProgress));
                    OnPropertyChanged(nameof(IsSortingNotInProgress));
                    return;

                case nameof(MainMenuModel.SortingTask):
                    return;
            }
        };
        
        model.ItemCountObservableValue.ValueChanged += _ => OnPropertyChanged(nameof(ItemCount));
    }

    public Visibility StatusBarVisibility => Visibility.Visible;

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

    public bool IsSortingNotInProgress => _service.IsSortingNotInProgress;
    public bool IsSortingInProgress => _service.IsSortingInProgress;
    public bool CanStartSorting => IsAlgorithmSelected
        && AreItemsInitialized
        && _service.IsSortingNotInProgress;

    public bool AreItemsInitialized => _model.Items is not null; 
    public bool CanStopSorting => IsSortingInProgress;
    public bool CanShuffle => AreItemsInitialized && _service.IsSortingNotInProgress;
    public bool CanInitializeItems => !AreItemsInitialized;

    public IEnumerable<ItemKind> ItemKinds => _ItemKinds;
    private static readonly ItemKind[] _ItemKinds = (ItemKind[]) Enum.GetValues(typeof(ItemKind));
    public ItemKind? ItemKind
    {
        get => _model.ItemKind;
        set => _service.SelectItemKind(value);
    }
    
    public int ItemCount => _model.Items?.List.Count ?? 0;
}

public partial class MainMenu : Window
{
    private MainMenuViewModel _viewModel;
    private readonly MainMenuService _service;

    public MainMenu(MainMenuViewModel viewModel, MainMenuService service)
    {
        _service = service;
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        service.Model.SelectionFilterProvider.ValueChanged += selectionFilter =>
        {
            FilterPanel.Children.Clear();
            selectionFilter?.EnableUi(FilterPanel);
        };
        
        InitializeComponent();
    }

    public async void StartSorting(object sender, RoutedEventArgs e)
    {
        await _service.StartSorting();
    }

    public void Randomize(object sender, RoutedEventArgs e)
    {
        _service.Randomize();
    }

    public void Shuffle(object sender, RoutedEventArgs e)
    {
        _service.Shuffle();
    }
}