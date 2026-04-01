using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NansHoi4Tool.Services;

public class NotificationItem : ObservableObject
{
    public string Title   { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int DurationMs { get; set; }
    public Action? OnDismiss { get; set; }

    private double _progress = -1;
    /// <summary>0–100 to show a progress bar; -1 = hidden.</summary>
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
    public bool HasProgress => Progress >= 0;
}

public class NotificationService : INotificationService
{
    public ObservableCollection<NotificationItem> ActiveNotifications { get; } = new();

    public void Show(string message, NotificationType type = NotificationType.Info, int durationMs = 3000)
        => ShowFull(string.Empty, message, type, durationMs);

    public void ShowFull(string title, string body, NotificationType type = NotificationType.Info, int durationMs = 4000)
    {
        NotificationItem? item = null;
        item = new NotificationItem
        {
            Title = title,
            Message = body,
            Type = type,
            DurationMs = durationMs,
            OnDismiss = () => Dismiss(item!)
        };

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            ActiveNotifications.Add(item);
            Task.Delay(durationMs).ContinueWith(_ =>
                System.Windows.Application.Current?.Dispatcher.Invoke(() => ActiveNotifications.Remove(item)));
        });
    }

    private void Dismiss(NotificationItem item)
        => System.Windows.Application.Current?.Dispatcher.Invoke(() => ActiveNotifications.Remove(item));

    public void ShowSuccess(string message) => Show(message, NotificationType.Success);
    public void ShowError(string message)   => Show(message, NotificationType.Error,   5000);
    public void ShowWarning(string message) => Show(message, NotificationType.Warning, 4000);

    /// <summary>Show a notification with an indeterminate or determinate progress bar.</summary>
    public NotificationItem ShowProgress(string title, string body, NotificationType type = NotificationType.Info)
    {
        NotificationItem? item = null;
        item = new NotificationItem
        {
            Title     = title,
            Message   = body,
            Type      = type,
            DurationMs = int.MaxValue,
            Progress  = 0,
            OnDismiss = () => Dismiss(item!)
        };
        System.Windows.Application.Current?.Dispatcher.Invoke(() => ActiveNotifications.Add(item));
        return item;
    }
}
