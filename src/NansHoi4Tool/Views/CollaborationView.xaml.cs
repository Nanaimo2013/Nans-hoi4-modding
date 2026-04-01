using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views;

public partial class CollaborationView : UserControl
{
    public CollaborationView(CollaborationViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
