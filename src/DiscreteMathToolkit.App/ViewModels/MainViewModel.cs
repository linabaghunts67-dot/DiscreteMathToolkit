using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.App.Services;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App.ViewModels;

public sealed class SidebarItem
{
    public string Glyph { get; }
    public string Label { get; }
    public Type PageType { get; }

    public SidebarItem(string glyph, string label, Type pageType)
    {
        Glyph = glyph;
        Label = label;
        PageType = pageType;
    }
}

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService _nav;
    private readonly IThemeService _theme;

    [ObservableProperty] private IPageViewModel? _currentPage;
    [ObservableProperty] private SidebarItem? _selectedSidebarItem;
    [ObservableProperty] private AppTheme _currentTheme;

    public ObservableCollection<SidebarItem> SidebarItems { get; }

    public IRelayCommand ToggleThemeCommand { get; }

    public MainViewModel(INavigationService nav, IThemeService theme)
    {
        _nav = nav;
        _theme = theme;
        _currentTheme = theme.CurrentTheme;

        SidebarItems = new ObservableCollection<SidebarItem>
        {
            new("⌂",  "Dashboard",          typeof(DashboardViewModel)),
            new("◈",  "Graph Theory",       typeof(GraphTheoryViewModel)),
            new("Y",  "Trees",              typeof(TreesViewModel)),
            new("⊨",  "Logic",              typeof(LogicViewModel)),
            new("C",  "Combinatorics",      typeof(CombinatoricsViewModel)),
            new("01", "Number Systems",     typeof(NumberSystemsViewModel)),
            new("∪",  "Sets & Relations",   typeof(SetsRelationsViewModel)),
            new("⚙",  "Settings",           typeof(SettingsViewModel)),
        };

        _nav.PageChanged += page =>
        {
            CurrentPage = page;
            // Sync sidebar selection if navigation came from somewhere else (e.g. dashboard tile)
            var match = SidebarItems.FirstOrDefault(s => s.PageType == page.GetType());
            if (match != null && match != SelectedSidebarItem) SelectedSidebarItem = match;
        };
        _theme.ThemeChanged += t => CurrentTheme = t;

        ToggleThemeCommand = new RelayCommand(() => _theme.Toggle());

        // Open dashboard on launch
        _selectedSidebarItem = SidebarItems[0];
        _nav.NavigateTo<DashboardViewModel>();
    }

    partial void OnSelectedSidebarItemChanged(SidebarItem? value)
    {
        if (value == null) return;
        if (CurrentPage?.GetType() == value.PageType) return; // already on this page
        _nav.NavigateTo(value.PageType);
    }
}
