using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class EventsView : UserControl
{
    public EventsView(EventsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
