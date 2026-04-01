using System.Globalization;
using System.Windows.Data;

namespace NansHoi4Tool.Helpers;

public class FocusGridConverter : IValueConverter
{
    public static readonly FocusGridConverter Instance = new();
    public double GridSize { get; set; } = 100;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? d * GridSize + 20 : 20;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? (d - 20) / GridSize : 0;
}
