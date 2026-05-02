using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DiscreteMathToolkit.App.ViewModels.Pages;
using DiscreteMathToolkit.Visualization.Layout;

namespace DiscreteMathToolkit.App.Views.Controls;

/// <summary>
/// Lightweight code-behind canvas that renders a <see cref="GraphRenderState"/>.
/// Uses theme brushes via DynamicResource so dark/light theming works automatically.
/// </summary>
public sealed class GraphCanvas : Canvas
{
    public static readonly DependencyProperty RenderStateProperty = DependencyProperty.Register(
        nameof(RenderState), typeof(GraphRenderState), typeof(GraphCanvas),
        new PropertyMetadata(GraphRenderState.Empty, OnRenderStateChanged));

    public GraphRenderState RenderState
    {
        get => (GraphRenderState)GetValue(RenderStateProperty);
        set => SetValue(RenderStateProperty, value);
    }

    public event Action<double, double>? CanvasSizeChanged;

    public GraphCanvas()
    {
        ClipToBounds = true;
        SizeChanged += (_, e) =>
        {
            CanvasSizeChanged?.Invoke(e.NewSize.Width, e.NewSize.Height);
            Redraw();
        };
        Background = Brushes.Transparent;
    }

    private static void OnRenderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GraphCanvas c) c.Redraw();
    }

    private void Redraw()
    {
        Children.Clear();
        var state = RenderState;
        if (state == null) return;

        // Edges first so nodes paint on top
        foreach (var edge in state.Edges)
        {
            var fromNode = FindNode(state, edge.From);
            var toNode = FindNode(state, edge.To);
            if (fromNode is null || toNode is null) continue;
            DrawEdge(fromNode.Position, toNode.Position, edge);
        }
        foreach (var node in state.Nodes)
        {
            DrawNode(node);
        }
    }

    private static RenderableNode? FindNode(GraphRenderState state, int id)
    {
        for (int i = 0; i < state.Nodes.Count; i++)
            if (state.Nodes[i].Id == id) return state.Nodes[i];
        return null;
    }

    private void DrawEdge(Point2D from, Point2D to, RenderableEdge edge)
    {
        const double nodeRadius = 22;
        // shorten so the line touches the boundary, not the centre
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist < 0.001) return;
        double ux = dx / dist;
        double uy = dy / dist;

        double sx = from.X + ux * nodeRadius;
        double sy = from.Y + uy * nodeRadius;
        double ex = to.X - ux * nodeRadius;
        double ey = to.Y - uy * nodeRadius;

        var (strokeKey, thickness, dashed) = edge.Badge switch
        {
            EdgeBadge.Active => ("AccentPrimaryBrush", 3.0, false),
            EdgeBadge.Tree => ("AccentSuccessBrush", 3.0, false),
            EdgeBadge.Accepted => ("AccentSuccessBrush", 2.5, false),
            EdgeBadge.Rejected => ("AccentDangerBrush", 1.5, true),
            _ => ("StrokeStrongBrush", 1.5, false),
        };

        var line = new Line
        {
            X1 = sx, Y1 = sy, X2 = ex, Y2 = ey,
            StrokeThickness = thickness,
            StrokeLineJoin = PenLineJoin.Round
        };
        line.SetResourceReference(Shape.StrokeProperty, strokeKey);
        if (dashed) line.StrokeDashArray = new DoubleCollection { 4, 4 };
        Children.Add(line);

        // arrowhead for directed edges
        if (edge.IsDirected)
        {
            const double arrowSize = 9;
            double angle = Math.Atan2(uy, ux);
            double a1 = angle + Math.PI - 0.4;
            double a2 = angle + Math.PI + 0.4;
            var arrow = new Polygon
            {
                Points = new PointCollection
                {
                    new System.Windows.Point(ex, ey),
                    new System.Windows.Point(ex + arrowSize * Math.Cos(a1), ey + arrowSize * Math.Sin(a1)),
                    new System.Windows.Point(ex + arrowSize * Math.Cos(a2), ey + arrowSize * Math.Sin(a2))
                }
            };
            arrow.SetResourceReference(Shape.FillProperty, strokeKey);
            Children.Add(arrow);
        }

        // edge label (weight)
        if (!string.IsNullOrEmpty(edge.Label))
        {
            var label = new TextBlock
            {
                Text = edge.Label,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(4, 1, 4, 1)
            };
            label.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            label.SetResourceReference(TextBlock.BackgroundProperty, "SurfaceRaisedBrush");

            // position at midpoint, slightly perpendicular to line
            double mx = (sx + ex) / 2.0;
            double my = (sy + ey) / 2.0;
            // perpendicular offset so it doesn't sit on the line
            double nx = -uy * 10;
            double ny = ux * 10;
            label.Loaded += (_, _) =>
            {
                Canvas.SetLeft(label, mx + nx - label.ActualWidth / 2);
                Canvas.SetTop(label, my + ny - label.ActualHeight / 2);
            };
            // initial estimate before Loaded fires
            Canvas.SetLeft(label, mx + nx - 8);
            Canvas.SetTop(label, my + ny - 8);
            Children.Add(label);
        }
    }

    private void DrawNode(RenderableNode node)
    {
        const double radius = 22;
        var (fillKey, strokeKey, foregroundKey) = node.Badge switch
        {
            NodeBadge.Current => ("AccentPrimaryBrush", "AccentPrimaryBrush", "SurfaceBaseBrush"),
            NodeBadge.Visited => ("SurfaceElevatedBrush", "AccentSecondaryBrush", "ForegroundPrimaryBrush"),
            NodeBadge.Settled => ("SurfaceElevatedBrush", "AccentSuccessBrush", "ForegroundPrimaryBrush"),
            NodeBadge.Frontier => ("SurfaceElevatedBrush", "AccentWarningBrush", "ForegroundPrimaryBrush"),
            NodeBadge.InTree => ("AccentSuccessBrush", "AccentSuccessBrush", "SurfaceBaseBrush"),
            _ => ("SurfaceRaisedBrush", "StrokeStrongBrush", "ForegroundPrimaryBrush"),
        };

        var circle = new Ellipse
        {
            Width = radius * 2, Height = radius * 2,
            StrokeThickness = 2.0
        };
        circle.SetResourceReference(Shape.FillProperty, fillKey);
        circle.SetResourceReference(Shape.StrokeProperty, strokeKey);
        Canvas.SetLeft(circle, node.Position.X - radius);
        Canvas.SetTop(circle, node.Position.Y - radius);
        Children.Add(circle);

        var label = new TextBlock
        {
            Text = node.Label,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            Width = radius * 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, foregroundKey);
        Canvas.SetLeft(label, node.Position.X - radius);
        Canvas.SetTop(label, node.Position.Y - 9);
        Children.Add(label);

        if (!string.IsNullOrEmpty(node.Annotation))
        {
            var ann = new TextBlock
            {
                Text = node.Annotation,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(4, 1, 4, 1),
            };
            ann.SetResourceReference(TextBlock.ForegroundProperty, "AccentPrimaryBrush");
            ann.SetResourceReference(TextBlock.BackgroundProperty, "SurfaceOverlayBrush");
            ann.Loaded += (_, _) =>
            {
                Canvas.SetLeft(ann, node.Position.X - ann.ActualWidth / 2);
                Canvas.SetTop(ann, node.Position.Y + radius + 4);
            };
            Canvas.SetLeft(ann, node.Position.X - 12);
            Canvas.SetTop(ann, node.Position.Y + radius + 4);
            Children.Add(ann);
        }
    }
}
