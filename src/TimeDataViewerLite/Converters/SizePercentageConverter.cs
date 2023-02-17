using System.Globalization;
using Avalonia.Data.Converters;

namespace TimeDataViewerLite.Converters;

public class SizePercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null)
        {
            return 0.5 * System.Convert.ToDouble(value);
        }

        try
        {
            var split = System.Convert.ToString(parameter)?.Split('.')!;
            double parameterDouble = System.Convert.ToDouble(split[0]) + System.Convert.ToDouble(split[1]) / (Math.Pow(10, split[1].Length));
            return System.Convert.ToDouble(value) * parameterDouble;
        }
        catch (Exception)
        {
            throw new Exception($"Not correct format parameter: {parameter}");
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
