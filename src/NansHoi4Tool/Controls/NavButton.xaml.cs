using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NansHoi4Tool.Controls;

public partial class NavButton : UserControl
{
    public static readonly DependencyProperty PageKeyProperty =
        DependencyProperty.Register(nameof(PageKey), typeof(string), typeof(NavButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(NavButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(NavButton), new PropertyMetadata("Circle"));

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(string), typeof(NavButton), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty NavCommandProperty =
        DependencyProperty.Register(nameof(NavCommand), typeof(ICommand), typeof(NavButton), new PropertyMetadata(null));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(NavButton), new PropertyMetadata(true));

    public string PageKey { get => (string)GetValue(PageKeyProperty); set => SetValue(PageKeyProperty, value); }
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public string CurrentPage { get => (string)GetValue(CurrentPageProperty); set => SetValue(CurrentPageProperty, value); }
    public ICommand NavCommand { get => (ICommand)GetValue(NavCommandProperty); set => SetValue(NavCommandProperty, value); }
    public bool IsExpanded { get => (bool)GetValue(IsExpandedProperty); set => SetValue(IsExpandedProperty, value); }

    public NavButton()
    {
        InitializeComponent();
    }
}
