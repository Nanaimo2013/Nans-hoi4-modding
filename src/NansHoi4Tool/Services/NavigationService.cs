namespace NansHoi4Tool.Services;

public class NavigationService : INavigationService
{
    private readonly Stack<string> _history = new();
    private string? _current;

    public string? CurrentPage => _current;
    public bool CanGoBack => _history.Count > 0;

    public event EventHandler<string>? Navigated;

    public void NavigateTo(string pageKey)
    {
        if (_current != null)
            _history.Push(_current);
        _current = pageKey;
        Navigated?.Invoke(this, pageKey);
    }

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            _current = _history.Pop();
            Navigated?.Invoke(this, _current);
        }
    }
}
