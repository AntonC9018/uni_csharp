using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace sem2_lab3;

public partial class StudentModel : ObservableObject
{
    [ObservableProperty] private string? _firstName;
    [ObservableProperty] private string? _lastName;
    [ObservableProperty] private float _averageGrade;
    [ObservableProperty] private string? _strongestSubject;
}

public class ClassroomDataModel
{
    public ObservableCollection<StudentModel> Students { get; } = new();
    public event Action? StudentsChangedInAnyWay;

    public ClassroomDataModel()
    {
        Students.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null)
            {
                foreach (var item in args.NewItems)
                    ((StudentModel) item).PropertyChanged += (_, _) => StudentsChangedInAnyWay?.Invoke();
            }
            StudentsChangedInAnyWay?.Invoke();
        };
    }
}

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    public ClassroomDataModel Model { get; }
    private Random _random = new();
    
    [NotifyCanExecuteChangedFor(nameof(RerunCurrentQueryCommand))]
    [ObservableProperty]
    private int? _selectedQueryIndex;

    public record struct QueryResultRecord(QueryResultKind Kind, object Value);
    [ObservableProperty] private QueryResultRecord _queryResult;
    [ObservableProperty] private bool _autoRerun = true;

    public MainWindowViewModel(ClassroomDataModel model)
    {
        Model = model;
        model.StudentsChangedInAnyWay += RerunCurrentQuery;
    }

    public void Dispose()
    {
        Model.StudentsChangedInAnyWay -= RerunCurrentQuery;
    }

    partial void OnSelectedQueryIndexChanged(int? value)
    {
        RerunCurrentQuery();
    }
    
    // Might need to wrap this in a view model for rows
    public ObservableCollection<StudentModel> StudentList => Model.Students;
    
    private static readonly string[] _Names = {"John", "Jane", "Bob", "Alice", "Joe", "Mary", "Jack", "Jill"};
    private static readonly string[] _LastNames = 
        { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore" };
    private static readonly string[] _Subjects = {"Math", "English", "Science", "History", "Art", "Music", "PE"};
    
    [RelayCommand]
    private void SpawnRandomStudent()
    {
        var student = new StudentModel
        {
            FirstName = _Names[_random.Next(_Names.Length)],
            LastName = _LastNames[_random.Next(_LastNames.Length)],
            AverageGrade = _random.Next(3, 10),
            StrongestSubject = _Subjects[_random.Next(_Subjects.Length)]
        };
        Model.Students.Add(student);
    }
    
    public IEnumerable<string> QueryNames => Queries.QueryInfos.Select(q => q.Name);

    [RelayCommand(CanExecute = nameof(CanRerunCurrentQuery))]
    private void RerunCurrentQuery()
    {
        if (SelectedQueryIndex is null)
            return;
        if (!AutoRerun)
            return;
        var qinfo = Queries.QueryInfos[SelectedQueryIndex.Value];
        var query = qinfo.Method;
        QueryResult = new(qinfo.Kind, query(Model.Students));
    }
    
    public bool CanRerunCurrentQuery => SelectedQueryIndex is not null;
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IDisposable
{
    public MainWindowViewModel ViewModel => (MainWindowViewModel) DataContext;
    
    public MainWindow()
    {
        var model = new ClassroomDataModel();
        model.Students.Add(new StudentModel
        {
            FirstName = "John",
            LastName = "Smith",
            AverageGrade = 5.5f,
            StrongestSubject = "Math"
        });
        DataContext = new MainWindowViewModel(model);
        InitializeComponent();
        
        ViewModel.PropertyChanged += (object? sender, PropertyChangedEventArgs args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.QueryResult):
                {
                    RefreshQueryResultView();
                    break;
                }
            }
        };
    }

    public void Dispose() => ViewModel.Dispose();

    private void RefreshQueryResultView()
    {
        var r = ViewModel.QueryResult;
        
        void Refresh<T>(Func<T> creator, Action<T> modify) where T : UIElement
        {
            if (QueryResultView.Children is [T widget])
            {
                modify(widget);
                return;
            }
            QueryResultView.Children.Clear();
            widget = creator();
            modify(widget);
            QueryResultView.Children.Add(widget);
        }

        void RefreshText(string? text)
        {
            Refresh(() => new TextBlock
                {
                    FontSize = 24,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                },
                tb => tb.Text = text ?? "No results.");
        }

        bool MaybeNoResults(IEnumerable enumerable)
        {
            var e = enumerable.GetEnumerator();
            using var _ = e as IDisposable;
            if (!e.MoveNext())
            {
                RefreshText(null);
                return true;
            }
            return false;
        }
        
        switch (r.Kind)
        {
            case QueryResultKind.SingleValue:
            {
                RefreshText(r.Value!.ToString());
                break;
            }
            case QueryResultKind.ComplexValue:
            {
                throw new NotImplementedException();
            }
            case QueryResultKind.MultipleSimpleValues:
            {
                if (MaybeNoResults((IEnumerable) r.Value!))
                    break;
                Refresh(() => new ListBox
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    lb => lb.ItemsSource = (IEnumerable) r.Value!);
                break;
            }
            case QueryResultKind.MultipleComplexValues:
            {
                if (MaybeNoResults((IEnumerable) r.Value!))
                    break;
                Refresh(() => new DataGrid
                    {
                        IsReadOnly = true,
                        AutoGenerateColumns = true,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    lb => lb.ItemsSource = (IEnumerable) r.Value!);
                break;
            }
        }
    }
}
