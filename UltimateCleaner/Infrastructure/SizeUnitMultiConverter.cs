using System.Globalization;
using System.Windows.Data;
using MemoryCleaner.Models;

namespace MemoryCleaner.Infrastructure;

public class SizeUnitMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return "";

        var bytes = values[0] is long b ? b : 0;
        var unit = values[1] is SizeUnit u ? u : SizeUnit.GB;

        if (values.Length >= 3 && values[2] is bool isDir && isDir)
            return "<DIR>";

        return unit switch
        {
            SizeUnit.Bytes => $"{bytes:N0} B",
            SizeUnit.MB => $"{bytes / 1024d / 1024d:N2} MB",
            SizeUnit.GB => $"{bytes / 1024d / 1024d / 1024d:N2} GB",
            _ => $"{bytes:N0} B"
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
