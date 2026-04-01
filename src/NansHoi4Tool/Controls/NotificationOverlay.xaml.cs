using System.Windows.Controls;
using NansHoi4Tool.Services;

namespace NansHoi4Tool.Controls;

public partial class NotificationOverlay : UserControl
{
    public NotificationOverlay()
    {
        InitializeComponent();
    }

    public void Bind(NotificationService service)
    {
        DataContext = service;
    }
}
