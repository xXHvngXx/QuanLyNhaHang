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

            _viewModel = new ReportViewModel();
            this.DataContext = _viewModel;

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

            ChartSeries.Stroke = new SolidColorBrush(mainColor);

            var gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0.5, 0);
            gradient.EndPoint = new Point(0.5, 1);
            gradient.GradientStops.Add(new GradientStop(mainColor, 0));
            gradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, mainColor.R, mainColor.G, mainColor.B), 1.2));

            ChartSeries.Fill = gradient;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.LoadReport();
        }

        private void dtp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                _viewModel.LoadReportCommand.Execute(null);
            }
        }
    }
}