using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class DecisionsView : UserControl
{
    public DecisionsView(DecisionsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
