using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NansHoi4Tool.Services;

namespace NansHoi4Tool.ViewModels;

public partial class UpdateViewModel : ObservableObject
{
    private readonly IAutoUpdateService _updater;
    private readonly ILogService        _log;

    [ObservableProperty] private string  _latestVersion  = string.Empty;
    [ObservableProperty] private string  _releaseNotes   = string.Empty;
    [ObservableProperty] private string  _publishedAt    = string.Empty;
    [ObservableProperty] private double  _downloadProgress;
    [ObservableProperty] private bool    _isDownloading;
    [ObservableProperty] private bool    _isCheckingUpdate = true;
    [ObservableProperty] private string  _statusText     = "Checking for updates…";
    [ObservableProperty] private bool    _updateAvailable;

    private ReleaseInfo? _pendingRelease;
    private CancellationTokenSource? _cts;

    public UpdateViewModel(IAutoUpdateService updater, ILogService log)
    {
        _updater = updater;
        _log     = log;
    }

    public async Task InitAsync()
    {
        IsCheckingUpdate = true;
        StatusText = "Checking for updates…";
        try
        {
            var info = await _updater.CheckForUpdateAsync();
            if (info is null)
            {
                StatusText      = "You're on the latest version.";
                UpdateAvailable = false;
            }
            else
            {
                _pendingRelease = info;
                LatestVersion   = info.Version;
                ReleaseNotes    = info.ReleaseNotes;
                PublishedAt     = info.PublishedAt.ToLocalTime().ToString("MMM dd, yyyy");
                UpdateAvailable = true;
                StatusText      = $"Version {info.Version} is available!";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Update check failed: {ex.Message}";
            _log.Warning($"[UpdateVM] {ex.Message}");
        }
        finally
        {
            IsCheckingUpdate = false;
        }
    }

    [RelayCommand]
    private async Task DownloadAndInstall()
    {
        if (_pendingRelease is null) return;
        _cts = new CancellationTokenSource();
        IsDownloading   = true;
        DownloadProgress = 0;
        StatusText      = "Downloading update…";

        try
        {
            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p;
                StatusText = $"Downloading… {p:F0}%";
            });

            var path = await _updater.DownloadInstallerAsync(_pendingRelease, progress, _cts.Token);
            StatusText = "Launching installer…";
            _updater.ApplyUpdate(path);
        }
        catch (OperationCanceledException)
        {
            StatusText = "Download cancelled.";
        }
        catch (Exception ex)
        {
            StatusText = $"Download failed: {ex.Message}";
            _log.Warning($"[UpdateVM] Download failed: {ex.Message}");
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private void CancelDownload() => _cts?.Cancel();

    [RelayCommand]
    private async Task Recheck() => await InitAsync();
}
