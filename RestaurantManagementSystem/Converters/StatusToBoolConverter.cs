using System;
using System.Globalization;
using System.Windows.Data;

namespace RestaurantManagementSystem.Converters
{
    public class StatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu chưa chọn dòng nào (value là null), nên trả về false để vô hiệu hóa nút
            if (value == null) return false;

            if (int.TryParse(value.ToString(), out int status))
            {
                return status == 0; // Trả về true nếu là 0 (Được phép Sửa/Xóa)
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}