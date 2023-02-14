using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Laborator1;

public partial class App : Application
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

        var services = new ServiceCollection();
        services.AddSingleton<IKeyedProvider<SortingAlgorithmKind, ISortingAlgorithm>>(SortingAlgorithmFactory.Instance);
        services.AddSingleton<IKeyedProvider<SelectionFilterKind, ISelectionFilter>, SelectionFilterFactory>();
        services.AddSingleton<ISortDisplay, SortDisplay>();
        services.AddSingleton(new MainMenuModel(default));
        services.AddSingleton<MainMenuService>();

        services.AddSingleton<MainMenuViewModel>();
        services.AddTransient<MainMenu>();
        
        var serviceProvider = services.BuildServiceProvider();

        var mainMenu = serviceProvider.GetRequiredService<MainMenu>();
        mainMenu.Show();
    }
}

