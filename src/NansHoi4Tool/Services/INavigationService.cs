using System.Windows.Controls;

namespace NansHoi4Tool.Services;

public interface INavigationService
{
    string? CurrentPage { get; }
    void NavigateTo(string pageKey);
    void GoBack();
    bool CanGoBack { get; }
    event EventHandler<string>? Navigated;
}
