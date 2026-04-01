using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class UnitsView : UserControl
{
    public UnitsView(UnitsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
