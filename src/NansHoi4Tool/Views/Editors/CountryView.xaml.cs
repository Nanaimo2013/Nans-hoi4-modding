using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views.Editors;

public partial class CountryView : UserControl
{
    public CountryView(CountryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
