using System.Windows.Controls;
using System.Windows.Media;
using RestaurantManagementSystem.ViewModels;
using System.Windows;
namespace RestaurantManagementSystem.Views
{
    public partial class ucReport : UserControl
    {
        private ReportViewModel _viewModel;

        public ucReport()
        {
            InitializeComponent();

            // Khởi tạo và gán DataContext
            _viewModel = new ReportViewModel();
            this.DataContext = _viewModel;

            // Đăng ký nhận sự kiện load xong để đổi màu biểu đồ
            _viewModel.OnReportLoaded = (average) =>
            {
                UpdateChartColor(average);
            };
        }

        private void UpdateChartColor(double average)
        {
            if (ChartSeries == null) return;

            Color mainColor;
            if (average >= 400000) mainColor = (Color)ColorConverter.ConvertFromString("#2ECC71"); // Xanh
            else if (average >= 300000) mainColor = (Color)ColorConverter.ConvertFromString("#F1C40F"); // Vàng
            else mainColor = (Color)ColorConverter.ConvertFromString("#E74C3C"); // Đỏ

            // Cập nhật đường kẻ (Stroke)
            ChartSeries.Stroke = new SolidColorBrush(mainColor);

            // Cập nhật vùng phủ (Fill) bằng Gradient mới
            var gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0.5, 0);
            gradient.EndPoint = new Point(0.5, 1);
            gradient.GradientStops.Add(new GradientStop(mainColor, 0));
            gradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, mainColor.R, mainColor.G, mainColor.B), 1.2));

            ChartSeries.Fill = gradient;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Tự động load dữ liệu lần đầu khi mở màn hình
            _viewModel.LoadReport();
        }

        private void dtp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Khi chọn ngày khác thì tự load lại
            if (this.IsLoaded)
            {
                _viewModel.LoadReportCommand.Execute(null);
            }
        }
    }
}