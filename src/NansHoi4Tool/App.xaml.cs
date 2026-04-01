using Microsoft.Extensions.DependencyInjection;
using NansHoi4Tool.Controls;
using NansHoi4Tool.Services;
using NansHoi4Tool.ViewModels;
using NansHoi4Tool.Views;
using NansHoi4Tool.Views.Editors;
using System.Windows;

namespace NansHoi4Tool;

public partial class App : Application
{
    private IServiceProvider _services = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        var settings = _services.GetRequiredService<IAppSettingsService>();
        var theme = _services.GetRequiredService<IThemeService>();
        (theme as ThemeService)?.Apply();

        var discord = _services.GetRequiredService<IDiscordService>();
        discord.Initialize();

        _services.GetRequiredService<AutoSaveService>();

        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services.GetService<IDiscordService>()?.Shutdown();
        base.OnExit(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core services (singletons)
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService>(p => p.GetRequiredService<NotificationService>());
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<ICollaborationService, CollaborationService>();
        services.AddSingleton<AutoSaveService>();
        services.AddSingleton<IHoi4ScriptExporter, Hoi4ScriptExporter>();
        services.AddSingleton<IAutoUpdateService, AutoUpdateService>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<FocusTreeViewModel>();
        services.AddSingleton<EventsViewModel>();
        services.AddSingleton<IdeasViewModel>();
        services.AddSingleton<TechnologiesViewModel>();
        services.AddSingleton<DecisionsViewModel>();
        services.AddSingleton<CountryViewModel>();
        services.AddSingleton<LocalisationViewModel>();
        services.AddSingleton<UnitsViewModel>();
        services.AddSingleton<VersionHistoryViewModel>();
        services.AddSingleton<CollaborationViewModel>();
        services.AddTransient<UpdateViewModel>();

        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<FocusTreeView>();
        services.AddTransient<EventsView>();
        services.AddTransient<IdeasView>();
        services.AddTransient<TechnologiesView>();
        services.AddTransient<DecisionsView>();
        services.AddTransient<CountryView>();
        services.AddTransient<LocalisationView>();
        services.AddTransient<UnitsView>();
        services.AddTransient<VersionHistoryView>();
        services.AddTransient<Views.CollaborationView>();
        services.AddTransient<Views.HelpView>();
        services.AddTransient<Views.UpdateView>();

        // Controls
        services.AddTransient<NotificationOverlay>();
    }
}

