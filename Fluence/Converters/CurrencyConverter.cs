using Fluence.Services;
using System;
using Windows.UI.Xaml.Data;

namespace Fluence.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return ((double)value).ToString(AppConstants.CurrencyFormat);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
