using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Controls.Presenters;

namespace SatelliteDemo.Converters
{
    public class LabelTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ContentPresenter presenter)
            {
                return new TranslateTransform(-presenter.Bounds.Width / 2.0, 0.0);
            }

            return new TranslateTransform();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
