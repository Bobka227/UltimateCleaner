using System;
using System.Globalization;
using System.Windows.Data;

namespace MemoryCleaner.Infrastructure;

public class BoolToTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDir)
            return isDir ? "Папка" : "Файл";

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
