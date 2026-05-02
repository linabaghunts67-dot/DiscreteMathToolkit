using System.Windows;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.App.Services;
using DiscreteMathToolkit.App.ViewModels;
using DiscreteMathToolkit.App.ViewModels.Pages;
using DiscreteMathToolkit.Infrastructure.Export;
using DiscreteMathToolkit.Infrastructure.Logging;
using DiscreteMathToolkit.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DiscreteMathToolkit.App;

public partial class App : Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var collection = new ServiceCollection();

        // Singletons
        collection.AddSingleton<IAppLogger>(_ => new SerilogAppLogger(SerilogAppLogger.DefaultLogDirectory));
        collection.AddSingleton<IGraphRepository, JsonGraphRepository>();
        collection.AddSingleton<IExportService, FileExportService>();
        collection.AddSingleton<IThemeService, ThemeService>();
        collection.AddSingleton<INavigationService, NavigationService>();
        collection.AddSingleton<MainViewModel>();

        // Page view models — singletons so editing state (graph, expression) is preserved across navigations
        collection.AddSingleton<DashboardViewModel>();
        collection.AddSingleton<GraphTheoryViewModel>();
        collection.AddSingleton<TreesViewModel>();
        collection.AddSingleton<LogicViewModel>();
        collection.AddSingleton<CombinatoricsViewModel>();
        collection.AddSingleton<NumberSystemsViewModel>();
        collection.AddSingleton<SetsRelationsViewModel>();
        collection.AddSingleton<SettingsViewModel>();
        collection.AddTransient<PlaceholderPageViewModel>();

        _services = collection.BuildServiceProvider();

        // Apply dark theme on startup
        var theme = _services.GetRequiredService<IThemeService>();
        theme.Apply(AppTheme.Dark);

        var logger = _services.GetRequiredService<IAppLogger>();
        logger.Info("Application starting.");

        var window = new MainWindow
        {
            DataContext = _services.GetRequiredService<MainViewModel>()
        };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_services is IDisposable d) d.Dispose();
        base.OnExit(e);
    }
}
