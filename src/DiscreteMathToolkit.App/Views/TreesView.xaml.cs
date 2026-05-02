using System.Windows.Controls;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App.Views;

public partial class TreesView : UserControl
{
    public TreesView()
    {
        InitializeComponent();
        Canvas.CanvasSizeChanged += OnCanvasSizeChanged;
    }

    private void OnCanvasSizeChanged(double width, double height)
    {
        if (DataContext is TreesViewModel vm)
            vm.OnCanvasSizeChanged(width, height);
    }
}
