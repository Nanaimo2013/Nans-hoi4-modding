using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views;

public partial class VersionHistoryView : UserControl
{
    public VersionHistoryView(VersionHistoryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
