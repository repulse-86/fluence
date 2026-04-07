using System;
using Windows.UI.Xaml.Data;

namespace Fluence.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                double percentage = (double)value;
                double totalWidth = 0;
                if (parameter != null && double.TryParse(parameter.ToString(), out totalWidth))
                {
                    return (percentage / 100.0) * totalWidth;
                }
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
