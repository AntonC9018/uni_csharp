using System.Windows.Controls;
using Laborator1;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace lab1.Forms;

public class RangeSelectionFilterModel : ObservableObject
{
    private readonly IProvider<int> _countProvider;
    private int _from;
    private int _to;
    
    public RangeSelectionFilterModel(IObservableValue<int> countProvider)
    {
        countProvider.ValueChanged += _ => OnPropertyChanged(nameof(Count));
        _countProvider = countProvider;
        _from = 0;
        _to = countProvider.Get();
    }
    
    public int From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }
    
    public int To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }
    
    public int Count => _countProvider.Get();
}

public class RangeSelectionFilterViewModel : ObservableObject
{
    private readonly RangeSelectionFilterModel _model;
    public RangeSelectionFilterViewModel(RangeSelectionFilterModel model)
    {
        _model = model;
        model.PropertyChanged += (_, args) => OnPropertyChanged(args.PropertyName);
    }
    
    public int From
    {
        get => _model.From;
        set => _model.From = value;
    }
    
    public int To
    {
        get => _model.To;
        set => _model.To = value;
    }
    
    public int Count => _model.Count;
}

public partial class RangeSelectionFilterUserControl : UserControl
{
    private RangeSelectionFilterViewModel _viewModel;
    public RangeSelectionFilterUserControl(RangeSelectionFilterViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
    }
}