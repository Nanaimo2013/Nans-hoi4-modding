using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class IdeasView : UserControl
{
    public IdeasView(IdeasViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
