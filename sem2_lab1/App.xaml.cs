using System;
using System.Windows;
using lab1.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Laborator1;

public sealed partial class App : Application
{
    private void Main(object sender, StartupEventArgs e)
    {
        this.DispatcherUnhandledException += (_, e) =>
        {
            MessageBox.Show(
                "An unhandled exception just occurred: " + e.Exception.Message,
                e.Exception.GetType().FullName,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        };
        
        // IG this DI framework can't do everything I want, I think it doesn't have custom scopes
        var services = new ServiceCollection();
        services.AddSingleton<IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm>>(SortingAlgorithmFactory.Instance);
        services.AddScoped<IKeyedProvider<SelectionFilterKind, ISelectionFilter>, SelectionFilterFactory>();
        services.AddScoped<ISortDisplay, SortDisplay>();

        void AddObservable<T>(ServiceCollection s)
        {
            s.AddScoped<IObservableValue<T>>(sp => sp.GetRequiredService<ObservableValue<T>>());
            s.AddScoped<IObservableRepo<T>>(sp => sp.GetRequiredService<ObservableValue<T>>());
            s.AddScoped<IGetter<T>>(sp => sp.GetRequiredService<ObservableValue<T>>());
            s.AddScoped<ISetter<T>>(sp => sp.GetRequiredService<ObservableValue<T>>());
            s.AddScoped<ObservableValue<T>, ObservableValue<T>>();
        }
        
        AddObservable<IItems>(services);
        AddObservable<ISortingAlgorithm>(services);
        AddObservable<ISortDisplay>(services);
        AddObservable<ISelectionFilter>(services);
        
        services.AddScoped(sp => new MainMenuModel(
            // Autofac would've been able to fill these in, in the default framework I have to do this manually.
            sp.GetRequiredService<ObservableValue<ISortingAlgorithm>>(),
            sp.GetRequiredService<ObservableValue<ISortDisplay>>(),
            sp.GetRequiredService<ObservableValue<ISelectionFilter>>(),
            sp.GetRequiredService<ObservableValue<IItems>>(),
            
            sp.GetRequiredService<ItemCountObservableValue>()));
        services.AddScoped<MainMenuService>();

        services.AddScoped<MainMenuViewModel>();
        services.AddScoped<MainMenu>();
        
        services.AddScoped<ItemCountObservableValue>();
        services.AddScoped<RangeSelectionFilterModel>(
            sp => new RangeSelectionFilterModel(
                sp.GetRequiredService<ItemCountObservableValue>()));

        services.AddScoped<IGetter<RangeSelectionFilterModel>, ValueGetter<RangeSelectionFilterModel>>();

        services.AddScoped<IRandomGetter<int>, RandomIntGetter>();
        services.AddScoped<IRandomGetter<float>, RandomFloatGetter>();
        services.AddScoped<IRandomGetter<string>, RandomStringGetter>();
        
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var mainMenu = scope.ServiceProvider.GetRequiredService<MainMenu>();
        mainMenu.Show();
    }
}

