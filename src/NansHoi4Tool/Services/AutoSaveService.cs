namespace NansHoi4Tool.Services;

public sealed class AutoSaveService : IDisposable
{
    private readonly IProjectService _projects;
    private readonly IAppSettingsService _settings;
    private readonly ILogService _log;
    private readonly INotificationService _notifications;
    private Timer? _timer;

    public AutoSaveService(
        IProjectService projects,
        IAppSettingsService settings,
        ILogService log,
        INotificationService notifications)
    {
        _projects = projects;
        _settings = settings;
        _log = log;
        _notifications = notifications;

        _projects.ProjectOpened += (_, _) => StartTimer();
        _projects.ProjectClosed += (_, _) => StopTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        var s = _settings.Current;
        if (!s.AutoSaveEnabled) return;
        var interval = TimeSpan.FromSeconds(Math.Max(15, s.AutoSaveIntervalSeconds));
        _timer = new Timer(OnTick, null, interval, interval);
        _log.Info($"Auto-save scheduled every {interval.TotalSeconds}s");
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private async void OnTick(object? _)
    {
        if (!_projects.IsProjectOpen) return;
        try
        {
            await _projects.SaveProjectAsync();
            _log.Info("Auto-saved project");
        }
        catch (Exception ex)
        {
            _log.Warning($"Auto-save failed: {ex.Message}");
        }
    }

    public void Dispose() => StopTimer();
}
