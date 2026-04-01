using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class LocalisationView : UserControl
{
    public LocalisationView(LocalisationViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
