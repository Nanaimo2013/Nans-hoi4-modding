using MahApps.Metro.Controls;
using NansHoi4Tool.Services;
using NansHoi4Tool.ViewModels;
using NansHoi4Tool.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NansHoi4Tool;

public partial class MainWindow : MetroWindow
{
    private readonly INavigationService _navigation;
    private readonly MainWindowViewModel _vm;
    private readonly IServiceProvider _services;

    public MainWindow(
        MainWindowViewModel vm,
        INavigationService navigation,
        IServiceProvider services)
    {
        InitializeComponent();
        _vm = vm;
        _navigation = navigation;
        _services = services;
        DataContext = vm;

        _navigation.Navigated += OnNavigated;
        Loaded += (_, _) =>
        {
            var ns = _services.GetService(typeof(NotificationService)) as NotificationService;
            if (ns != null) NotifOverlay.Bind(ns);
            _navigation.NavigateTo("Dashboard");
        };
    }

    private void OnNavigated(object? sender, string pageKey)
    {
        var view = ResolveView(pageKey);
        if (view == null) return;

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var dur  = new Duration(TimeSpan.FromMilliseconds(260));

        var translate = new TranslateTransform(0, 14);
        MainContent.RenderTransform = translate;
        MainContent.Opacity = 0;
        MainContent.Content = view;

        MainContent.BeginAnimation(OpacityProperty,
            new DoubleAnimation(0, 1, dur) { EasingFunction = ease });

        translate.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(14, 0, dur) { EasingFunction = ease });
    }

    private UserControl? ResolveView(string pageKey) => pageKey switch
    {
        "Dashboard" => _services.GetService(typeof(DashboardView)) as UserControl,
        "Settings" => _services.GetService(typeof(SettingsView)) as UserControl,
        "FocusTree" => _services.GetService(typeof(Views.Editors.FocusTreeView)) as UserControl,
        "Events" => _services.GetService(typeof(Views.Editors.EventsView)) as UserControl,
        "Ideas" => _services.GetService(typeof(Views.Editors.IdeasView)) as UserControl,
        "Technologies" => _services.GetService(typeof(Views.Editors.TechnologiesView)) as UserControl,
        "Decisions" => _services.GetService(typeof(Views.Editors.DecisionsView)) as UserControl,
        "Country" => _services.GetService(typeof(Views.Editors.CountryView)) as UserControl,
        "Localisation" => _services.GetService(typeof(Views.Editors.LocalisationView)) as UserControl,
        "Units" => _services.GetService(typeof(Views.Editors.UnitsView)) as UserControl,
        "VersionHistory" => _services.GetService(typeof(Views.VersionHistoryView)) as UserControl,
        "Collaboration" => _services.GetService(typeof(Views.CollaborationView)) as UserControl,
        "Help" => _services.GetService(typeof(Views.HelpView)) as UserControl,
        "Update" => _services.GetService(typeof(Views.UpdateView)) as UserControl,
        _ => null
    };
}