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
        services.AddScoped<IKeyedProvider<SelectionFilterKind, ISelectionFilter>, SelectionFilterFactory>();
        services.AddScoped<ISortDisplay, SortDisplay>();
        services.AddScoped(sp => new MainMenuModel(default));
        services.AddScoped<MainMenuService>();

        services.AddScoped<MainMenuViewModel>();
        services.AddScoped<MainMenu>();

        var serviceProvider = services.BuildServiceProvider();

        var scope = serviceProvider.CreateScope();
        var mainMenu = scope.ServiceProvider.GetRequiredService<MainMenu>();
        mainMenu.Show();
    }
}

