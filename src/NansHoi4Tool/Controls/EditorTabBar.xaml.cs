using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NansHoi4Tool.Controls;

/// <summary>
/// A horizontal scrollable tab strip with a + button.
/// Bind Tabs, AddCommand, SelectTabCommand, CloseTabCommand from the parent DataContext.
/// Each tab item must expose: Label (string), IsActive (bool), CanClose (bool).
/// </summary>
public partial class EditorTabBar : UserControl
{
    public static readonly DependencyProperty TabsProperty =
        DependencyProperty.Register(nameof(Tabs), typeof(IEnumerable), typeof(EditorTabBar));

    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(EditorTabBar));

    public static readonly DependencyProperty SelectTabCommandProperty =
        DependencyProperty.Register(nameof(SelectTabCommand), typeof(ICommand), typeof(EditorTabBar));

    public static readonly DependencyProperty CloseTabCommandProperty =
        DependencyProperty.Register(nameof(CloseTabCommand), typeof(ICommand), typeof(EditorTabBar));

    public IEnumerable? Tabs
    {
        get => (IEnumerable?)GetValue(TabsProperty);
        set => SetValue(TabsProperty, value);
    }

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public ICommand? SelectTabCommand
    {
        get => (ICommand?)GetValue(SelectTabCommandProperty);
        set => SetValue(SelectTabCommandProperty, value);
    }

    public ICommand? CloseTabCommand
    {
        get => (ICommand?)GetValue(CloseTabCommandProperty);
        set => SetValue(CloseTabCommandProperty, value);
    }

    public EditorTabBar()
    {
        InitializeComponent();
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        TabScroll.ScrollToHorizontalOffset(TabScroll.HorizontalOffset - e.Delta * 0.5);
        e.Handled = true;
    }
}
