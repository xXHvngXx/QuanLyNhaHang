using System;
using System.Globalization;
using System.Windows.Data;

namespace RestaurantManagementSystem.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return status == 1 ? "Đã gửi" : "Chưa gửi";
            }
            return "Chưa xác định";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}