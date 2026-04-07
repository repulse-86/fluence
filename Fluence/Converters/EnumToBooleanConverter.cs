using System;
using Windows.UI.Xaml.Data;

namespace Fluence.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null) return false;

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();

            return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null) return null;

            bool useValue = (bool)value;
            if (useValue) return parameter.ToString();

            return null;
        }
    }
}
