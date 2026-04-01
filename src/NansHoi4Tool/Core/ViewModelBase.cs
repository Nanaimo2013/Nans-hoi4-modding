using CommunityToolkit.Mvvm.ComponentModel;

namespace NansHoi4Tool.Core;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _busyMessage = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    protected void SetBusy(string message = "Loading...")
    {
        IsBusy = true;
        BusyMessage = message;
    }

    protected void ClearBusy()
    {
        IsBusy = false;
        BusyMessage = string.Empty;
    }
}
