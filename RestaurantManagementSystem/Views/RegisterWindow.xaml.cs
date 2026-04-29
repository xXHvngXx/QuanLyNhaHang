using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            var vm = new RegisterViewModel();
            this.DataContext = vm;

            // Đăng ký nhận sự kiện từ ViewModel
            vm.OnRegisterSuccess = async () => await RunSuccessAnimation();
            vm.OnRegisterFail = async (header, msg) => await ShowErrorNotification(header, msg);
        }

        private async Task RunSuccessAnimation()
        {
            brdSuccess.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) };
            brdSuccess.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1500);
            this.Close(); // Đóng form để về Login
        }

        private async Task ShowErrorNotification(string header, string message)
        {
            txtErrorHeader.Text = header;
            txtErrorMessage.Text = message;
            brdError.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400) };
            brdError.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(2500);

            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(400) };
            fadeOut.Completed += (s, a) => brdError.Visibility = Visibility.Collapsed;
            brdError.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}