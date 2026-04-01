namespace NansHoi4Tool.Services;

public enum NotificationType { Info, Success, Warning, Error }

public interface INotificationService
{
    void Show(string message, NotificationType type = NotificationType.Info, int durationMs = 3000);
    void ShowFull(string title, string body, NotificationType type = NotificationType.Info, int durationMs = 4000);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
}
