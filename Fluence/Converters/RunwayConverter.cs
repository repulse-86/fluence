using System;
using Windows.UI.Xaml.Data;

namespace Fluence.Converters
{
    public class RunwayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                int days = (int)value;
                if (days == -1)
                {
                    return "∞";
                }
            }
            return value != null ? value.ToString() : "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
