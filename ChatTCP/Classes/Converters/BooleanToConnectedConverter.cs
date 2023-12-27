using System.Globalization;
using System.Windows.Data;

namespace ChatTCP.Classes.Converters
{
    internal class BooleanToConnectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Disconnect";
            }
            else
            {
                return "Connect";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
