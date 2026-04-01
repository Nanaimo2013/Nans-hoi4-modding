using System.Windows.Controls;
using NansHoi4Tool.ViewModels;

namespace NansHoi4Tool.Views;

public partial class UpdateView : UserControl
{
    public UpdateView(UpdateViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, _) => await vm.InitAsync();
    }
}
