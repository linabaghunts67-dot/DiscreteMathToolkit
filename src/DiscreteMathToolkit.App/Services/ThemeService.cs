using System.Windows;

namespace DiscreteMathToolkit.App.Services;

public enum AppTheme { Dark, Light }

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    event Action<AppTheme>? ThemeChanged;
    void Apply(AppTheme theme);
    void Toggle();
}

public sealed class ThemeService : IThemeService
{
    public AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;
    public event Action<AppTheme>? ThemeChanged;

    public void Apply(AppTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var existingTheme = dictionaries.FirstOrDefault(d =>
            d.Source != null &&
            (d.Source.OriginalString.EndsWith("ColorsDark.xaml", StringComparison.Ordinal) ||
             d.Source.OriginalString.EndsWith("ColorsLight.xaml", StringComparison.Ordinal)));
        if (existingTheme != null) dictionaries.Remove(existingTheme);

        var uri = theme == AppTheme.Dark
            ? new Uri("pack://application:,,,/Themes/ColorsDark.xaml", UriKind.Absolute)
            : new Uri("pack://application:,,,/Themes/ColorsLight.xaml", UriKind.Absolute);

        // Insert at the front so any other dictionaries (control styles) override correctly
        dictionaries.Insert(0, new ResourceDictionary { Source = uri });

        CurrentTheme = theme;
        ThemeChanged?.Invoke(theme);
    }

    public void Toggle() =>
        Apply(CurrentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark);
}
