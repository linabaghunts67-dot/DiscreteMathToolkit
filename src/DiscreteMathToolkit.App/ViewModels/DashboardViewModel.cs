using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DiscreteMathToolkit.App.Mvvm;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App.ViewModels;

public sealed class DashboardTile
{
    public string Title { get; }
    public string Description { get; }
    public string Glyph { get; }
    public Type PageType { get; }

    public DashboardTile(string title, string description, string glyph, Type pageType)
    {
        Title = title;
        Description = description;
        Glyph = glyph;
        PageType = pageType;
    }
}

public sealed class DashboardViewModel : ViewModelBase, IPageViewModel
{
    private readonly INavigationService _nav;

    public string Title => "Dashboard";

    public ObservableCollection<DashboardTile> Tiles { get; }
    public IRelayCommand<DashboardTile> OpenTileCommand { get; }

    public DashboardViewModel(INavigationService nav)
    {
        _nav = nav;
        Tiles = new ObservableCollection<DashboardTile>
        {
            new("Graph Theory",         "BFS, DFS, Dijkstra, Prim, Kruskal — with animated step playback.",  "◈", typeof(GraphTheoryViewModel)),
            new("Trees",                "Build BSTs, animate traversals, reconstruct from sequences.",       "Y", typeof(TreesViewModel)),
            new("Logic & Truth Tables", "Parse and evaluate propositional logic; build truth tables.",       "⊨", typeof(LogicViewModel)),
            new("Combinatorics",        "Permutations, combinations, Pascal's triangle.",                    "C", typeof(CombinatoricsViewModel)),
            new("Number Systems",       "Base conversion, two's complement, Hamming(7,4).",                  "01", typeof(NumberSystemsViewModel)),
            new("Sets & Relations",     "Set operations, relation property analysis.",                       "∪", typeof(SetsRelationsViewModel)),
        };
        OpenTileCommand = new RelayCommand<DashboardTile>(tile =>
        {
            if (tile == null) return;
            _nav.NavigateTo(tile.PageType);
        });
    }
}
