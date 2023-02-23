using System;
using System.ComponentModel;
using System.Windows.Controls;
using Laborator1;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace lab1.Forms;

public sealed class RangeSelectionFilterModel : ObservableObject
{
    private readonly IGetter<int> _countGetter;
    private int _from;
    private int _to;
    
    public RangeSelectionFilterModel(IObservableValue<int> countGetter)
    {
        countGetter.ValueChanged += _ => OnPropertyChanged(nameof(Count));
        _countGetter = countGetter;
        _from = 0;
        _to = countGetter.Get();
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
    
    public int Count => _countGetter.Get();
}

public sealed class RangeSelectionFilterViewModel : ObservableObject, IDisposable
{
    private readonly RangeSelectionFilterModel _model;
    
    public RangeSelectionFilterViewModel(RangeSelectionFilterModel model)
    {
        _model = model;
        model.PropertyChanged += OnModelPropertyChanged;
    }

    private void OnModelPropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        OnPropertyChanged(args.PropertyName);
    }

    public void Dispose()
    {
        _model.PropertyChanged -= OnModelPropertyChanged;
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

public sealed partial class RangeSelectionFilterUserControl : UserControl
{
    public RangeSelectionFilterUserControl(RangeSelectionFilterViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}