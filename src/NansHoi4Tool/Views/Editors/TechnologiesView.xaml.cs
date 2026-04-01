using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class TechnologiesView : UserControl
{
    private const double GridCellPx    = 100.0;
    private const double DragThreshold = 5.0;
    private const double ZoomMin       = 0.25;
    private const double ZoomMax       = 4.0;
    private const double ZoomStep      = 0.1;

    private double _zoom = 1.0;

    private Technology? _dragging;
    private Point       _dragStartMouse;
    private double      _dragStartNodeX;
    private double      _dragStartNodeY;
    private bool        _isDragging;

    public TechnologiesView(TechnologiesViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        TechCanvas.PreviewMouseLeftButtonDown += OnCanvasMouseDown;
        TechCanvas.PreviewMouseMove           += OnCanvasMouseMove;
        TechCanvas.PreviewMouseLeftButtonUp   += OnCanvasMouseUp;
        TechScrollViewer.PreviewMouseWheel    += OnScrollViewerWheel;
    }

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos  = e.GetPosition(TechCanvas);
        var node = HitTestNode(pos);
        if (node == null) return;

        _dragging       = node;
        _dragStartMouse = pos;
        _dragStartNodeX = node.X;
        _dragStartNodeY = node.Y;
        _isDragging     = false;
        TechCanvas.CaptureMouse();
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging == null || e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(TechCanvas);
        var dx  = pos.X - _dragStartMouse.X;
        var dy  = pos.Y - _dragStartMouse.Y;

        if (!_isDragging && (Math.Abs(dx) > DragThreshold || Math.Abs(dy) > DragThreshold))
            _isDragging = true;

        if (!_isDragging) return;

        _dragging.X = Math.Max(0, Math.Round((_dragStartNodeX * GridCellPx + dx) / GridCellPx));
        _dragging.Y = Math.Max(0, Math.Round((_dragStartNodeY * GridCellPx + dy) / GridCellPx));

        (DataContext as TechnologiesViewModel)?.RebuildConnections();
        e.Handled = true;
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragging == null) return;

        TechCanvas.ReleaseMouseCapture();
        if (_isDragging)
        {
            (DataContext as TechnologiesViewModel)?.NotifyNodeMoved();
            e.Handled = true;
        }
        _dragging   = null;
        _isDragging = false;
    }

    private void OnScrollViewerWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        e.Handled = true;
        _zoom = Math.Clamp(_zoom + (e.Delta > 0 ? ZoomStep : -ZoomStep), ZoomMin, ZoomMax);
        TechCanvas.LayoutTransform = new ScaleTransform(_zoom, _zoom);
    }

    private Technology? HitTestNode(Point pos)
    {
        var result = VisualTreeHelper.HitTest(TechCanvas, pos);
        if (result == null) return null;

        DependencyObject? element = result.VisualHit;
        while (element != null)
        {
            if (element is FrameworkElement { DataContext: Technology tech })
                return tech;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
