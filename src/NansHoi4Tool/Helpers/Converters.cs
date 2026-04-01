using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NansHoi4Tool.Services;

namespace NansHoi4Tool.Helpers;

public class BoolToWidthConverter : IValueConverter
{
    public double ExpandedWidth { get; set; } = 220;
    public double CollapsedWidth { get; set; } = 56;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? ExpandedWidth : CollapsedWidth;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is bool bv && bv;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NotificationTypeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is NotificationType t ? t switch
        {
            NotificationType.Success => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            NotificationType.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
            NotificationType.Warning => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
            _ => new SolidColorBrush(Color.FromRgb(33, 150, 243))
        } : Brushes.Gray;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NotificationTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is NotificationType t ? t switch
        {
            NotificationType.Success => "CheckCircle",
            NotificationType.Error => "AlertCircle",
            NotificationType.Warning => "Alert",
            _ => "InformationOutline"
        } : "InformationOutline";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Binds a RadioButton to an enum property. ConverterParameter = enum member name.</summary>
public class EnumToBoolConverter : IValueConverter
{
    public static readonly EnumToBoolConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b && b && parameter != null && targetType.IsEnum)
            return Enum.Parse(targetType, parameter.ToString()!);
        return Binding.DoNothing;
    }
}

public class PageKeyToSelectedConverter : IValueConverter, IMultiValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => values?.Length >= 2 && values[0]?.ToString() == values[1]?.ToString();
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Wraps an <see cref="Action"/> as an <see cref="ICommand"/> for XAML binding.</summary>
public class ActionToCommandConverter : IValueConverter
{
    public static readonly ActionToCommandConverter Instance = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Action action ? new ActionCommand(action) : null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

internal sealed class ActionCommand : ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action) => _action = action;
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _action();
}
