using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace sem2_lab2;

public partial class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<NamedObject<CollectionValues>> _collectionCollection = Collections.All;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCollection))]
    [NotifyPropertyChangedFor(nameof(CurrentCollectionName))]
    [NotifyPropertyChangedFor(nameof(CollectionNames))]
    [NotifyPropertyChangedFor(nameof(SelectedCollectionNamedValues))]
    public int _selectedCollectionIndex;
    
    public NamedObject<CollectionValues> SelectedCollection => _collectionCollection[SelectedCollectionIndex];
    public IEnumerable<NamedObject<object>> SelectedCollectionNamedValues => SelectedCollection.Value.Values;
    // public IEnumerable<object> CollectionAsObject => SelectedCollection.Value.Values;
    // public IEnumerable<object> CurrentItems => SelectedCollection.Value.Values.Select(x => x.Value);
    // public IEnumerable<string> CurrentNames => SelectedCollection.Value.Values.Select(x => x.DisplayName);
    public IEnumerable<string> CollectionNames => _collectionCollection.Select(x => x.DisplayName);
    public string CurrentCollectionName => SelectedCollection.DisplayName;
}

public partial class MainView : Window
{
    public MainView()
    {
        var viewModel = new MainViewModel();
        DataContext = viewModel;
        InitializeComponent();
    }
}
