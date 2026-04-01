using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class FocusTreeView : UserControl
{
    private const double GridCellPx   = 100.0;
    private const double DragThreshold = 5.0;
    private const double ZoomMin       = 0.25;
    private const double ZoomMax       = 4.0;
    private const double ZoomStep      = 0.1;

    private double    _zoom = 1.0;
    private FocusNode? _draggingNode;
    private Point      _dragStartMouse;
    private double     _dragStartNodeX;
    private double     _dragStartNodeY;
    private bool       _isDragging;

    public FocusTreeView(FocusTreeViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        FocusCanvas.PreviewMouseLeftButtonDown += OnCanvasMouseDown;
        FocusCanvas.PreviewMouseMove           += OnCanvasMouseMove;
        FocusCanvas.PreviewMouseLeftButtonUp   += OnCanvasMouseUp;
        FocusScrollViewer.PreviewMouseWheel    += OnScrollViewerWheel;
    }

    // ── drag helpers ────────────────────────────────────────────────────────

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos  = e.GetPosition(FocusCanvas);
        var node = HitTestNode(pos);
        if (node == null) return;

        _draggingNode   = node;
        _dragStartMouse = pos;
        _dragStartNodeX = node.X;
        _dragStartNodeY = node.Y;
        _isDragging     = false;
        // Don't capture yet — wait for threshold so button clicks still fire
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggingNode == null || e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(FocusCanvas);
        var dx  = pos.X - _dragStartMouse.X;
        var dy  = pos.Y - _dragStartMouse.Y;

        if (!_isDragging && (Math.Abs(dx) > DragThreshold || Math.Abs(dy) > DragThreshold))
        {
            _isDragging = true;
            FocusCanvas.CaptureMouse(); // capture only once we're sure this is a drag
        }

        if (!_isDragging) return;

        _draggingNode.X = Math.Max(0, Math.Round((_dragStartNodeX * GridCellPx + dx) / GridCellPx));
        _draggingNode.Y = Math.Max(0, Math.Round((_dragStartNodeY * GridCellPx + dy) / GridCellPx));

        (DataContext as FocusTreeViewModel)?.RebuildConnections();
        e.Handled = true;
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggingNode == null) return;

        FocusCanvas.ReleaseMouseCapture();
        if (_isDragging)
        {
            (DataContext as FocusTreeViewModel)?.NotifyNodeMoved();
            e.Handled = true;
        }
        _draggingNode = null;
        _isDragging   = false;
    }

    // ── connect-mode cursor line ─────────────────────────────────────────────

    private void OnCanvasMouseMoveForCursor(object sender, MouseEventArgs e)
    {
        var vm = DataContext as FocusTreeViewModel;
        if (vm == null || !vm.ConnectModeActive || vm.ConnectSource == null)
        {
            CursorLine.Visibility = Visibility.Collapsed;
            return;
        }

        const double half = FocusConnection.NodeHalf;
        const double cell = FocusConnection.GridCellPx;

        var pos = e.GetPosition(FocusCanvas);
        CursorLine.Visibility = Visibility.Visible;
        CursorLine.X1 = vm.ConnectSource.X * cell + half;
        CursorLine.Y1 = vm.ConnectSource.Y * cell + half;
        CursorLine.X2 = pos.X;
        CursorLine.Y2 = pos.Y;
    }

    // ── zoom ─────────────────────────────────────────────────────────────────

    private void OnScrollViewerWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        e.Handled = true;
        _zoom = Math.Clamp(_zoom + (e.Delta > 0 ? ZoomStep : -ZoomStep), ZoomMin, ZoomMax);
        FocusCanvas.LayoutTransform = new ScaleTransform(_zoom, _zoom);
    }

    // ── hit test ─────────────────────────────────────────────────────────────

    private FocusNode? HitTestNode(Point canvasPos)
    {
        var result = VisualTreeHelper.HitTest(FocusCanvas, canvasPos);
        if (result == null) return null;

        DependencyObject? element = result.VisualHit;
        while (element != null)
        {
            if (element is FrameworkElement { DataContext: FocusNode node })
                return node;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
