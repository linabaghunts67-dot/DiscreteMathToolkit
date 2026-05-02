using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.App.Services;
using DiscreteMathToolkit.Infrastructure.Logging;

namespace DiscreteMathToolkit.App.ViewModels.Pages;

public sealed partial class SettingsViewModel : ViewModelBase, IPageViewModel
{
    private readonly IThemeService _theme;

    public string Title => "Settings";

    public string AppVersion => typeof(SettingsViewModel).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    public string LogDirectory => SerilogAppLogger.DefaultLogDirectory;

    [ObservableProperty] private AppTheme _selectedTheme;

    public IRelayCommand OpenLogFolderCommand { get; }
    public IRelayCommand SetDarkCommand { get; }
    public IRelayCommand SetLightCommand { get; }

    public SettingsViewModel(IThemeService theme)
    {
        _theme = theme;
        _selectedTheme = theme.CurrentTheme;
        _theme.ThemeChanged += t => SelectedTheme = t;

        OpenLogFolderCommand = new RelayCommand(OpenLogFolder);
        SetDarkCommand = new RelayCommand(() => _theme.Apply(AppTheme.Dark));
        SetLightCommand = new RelayCommand(() => _theme.Apply(AppTheme.Light));
    }

    private void OpenLogFolder()
    {
        try
        {
            System.IO.Directory.CreateDirectory(LogDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = LogDirectory,
                UseShellExecute = true
            });
        }
        catch
        {
            // best effort: nothing to do if Explorer can't open it
        }
    }
}
