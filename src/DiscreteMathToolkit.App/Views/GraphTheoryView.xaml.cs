using System.Windows.Controls;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App.Views;

public partial class GraphTheoryView : UserControl
{
    public GraphTheoryView()
    {
        InitializeComponent();
        Canvas.CanvasSizeChanged += OnCanvasSizeChanged;
    }

    private void OnCanvasSizeChanged(double width, double height)
    {
        if (DataContext is GraphTheoryViewModel vm)
            vm.OnCanvasSizeChanged(width, height);
    }
}
