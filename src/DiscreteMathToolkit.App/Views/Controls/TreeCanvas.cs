using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DiscreteMathToolkit.App.ViewModels.Pages;

namespace DiscreteMathToolkit.App.Views.Controls;

public sealed class TreeCanvas : Canvas
{
    public static readonly DependencyProperty RenderStateProperty = DependencyProperty.Register(
        nameof(RenderState), typeof(TreeRenderState), typeof(TreeCanvas),
        new PropertyMetadata(TreeRenderState.Empty, OnChanged));

    public TreeRenderState RenderState
    {
        get => (TreeRenderState)GetValue(RenderStateProperty);
        set => SetValue(RenderStateProperty, value);
    }

    public event Action<double, double>? CanvasSizeChanged;

    public TreeCanvas()
    {
        ClipToBounds = true;
        Background = Brushes.Transparent;
        SizeChanged += (_, e) =>
        {
            CanvasSizeChanged?.Invoke(e.NewSize.Width, e.NewSize.Height);
            Redraw();
        };
    }

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeCanvas c) c.Redraw();
    }

    private void Redraw()
    {
        Children.Clear();
        var st = RenderState;
        if (st == null) return;

        foreach (var edge in st.Edges)
        {
            var line = new Line
            {
                X1 = edge.From.X, Y1 = edge.From.Y,
                X2 = edge.To.X, Y2 = edge.To.Y,
                StrokeThickness = 1.5
            };
            line.SetResourceReference(Shape.StrokeProperty, "StrokeStrongBrush");
            Children.Add(line);
        }

        const double r = 20;
        foreach (var node in st.Nodes)
        {
            var (fillKey, strokeKey, fgKey) = node.Badge switch
            {
                TreeNodeBadge.Current => ("AccentPrimaryBrush", "AccentPrimaryBrush", "SurfaceBaseBrush"),
                TreeNodeBadge.Visited => ("SurfaceElevatedBrush", "AccentSecondaryBrush", "ForegroundPrimaryBrush"),
                _ => ("SurfaceRaisedBrush", "StrokeStrongBrush", "ForegroundPrimaryBrush"),
            };

            var circle = new Ellipse { Width = r * 2, Height = r * 2, StrokeThickness = 2.0 };
            circle.SetResourceReference(Shape.FillProperty, fillKey);
            circle.SetResourceReference(Shape.StrokeProperty, strokeKey);
            Canvas.SetLeft(circle, node.Position.X - r);
            Canvas.SetTop(circle, node.Position.Y - r);
            Children.Add(circle);

            var label = new TextBlock
            {
                Text = node.Value.ToString(),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                Width = r * 2
            };
            label.SetResourceReference(TextBlock.ForegroundProperty, fgKey);
            Canvas.SetLeft(label, node.Position.X - r);
            Canvas.SetTop(label, node.Position.Y - 9);
            Children.Add(label);

            if (!string.IsNullOrEmpty(node.Annotation))
            {
                var ann = new TextBlock
                {
                    Text = node.Annotation,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Padding = new Thickness(4, 1, 4, 1)
                };
                ann.SetResourceReference(TextBlock.ForegroundProperty, "AccentPrimaryBrush");
                ann.SetResourceReference(TextBlock.BackgroundProperty, "SurfaceOverlayBrush");
                Canvas.SetLeft(ann, node.Position.X - 12);
                Canvas.SetTop(ann, node.Position.Y + r + 4);
                Children.Add(ann);
            }
        }
    }
}
