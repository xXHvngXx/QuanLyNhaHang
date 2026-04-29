using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Kết nối View với bộ não ViewModel
            var vm = new LoginViewModel();
            this.DataContext = vm;

            // Hứng tín hiệu thành công/thất bại để chạy Animation
            vm.OnLoginSuccess = async () => await RunSuccessAnimation();
            vm.OnLoginFail = async () => await RunErrorAnimation();
        }

        private async Task RunSuccessAnimation()
        {
            brdSuccess.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) };
            brdSuccess.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1000);

            MainWindow main = new MainWindow();
            main.Opacity = 0;
            main.Show();

            var fadeOutLogin = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(100) };
            var fadeInMain = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };

            fadeOutLogin.Completed += (s, ev) => this.Close();

            this.BeginAnimation(Window.OpacityProperty, fadeOutLogin);
            main.BeginAnimation(Window.OpacityProperty, fadeInMain);
        }

        private async Task RunErrorAnimation()
        {
            brdError.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400) };
            brdError.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1000);

            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(400) };
            fadeOut.Completed += (s, a) => brdError.Visibility = Visibility.Collapsed;
            brdError.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnShowRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            this.Hide();
            register.ShowDialog();
            if (Application.Current != null) this.Show();
        }
    }
}