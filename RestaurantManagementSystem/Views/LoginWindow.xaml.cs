using RestaurantManagementSystem.ViewModels;
using RestaurantManagementSystem.Models; 
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Khởi tạo Service và gán DataContext
            IMessageService messageService = new MessageService();
            var vm = new LoginViewModel(messageService);
            this.DataContext = vm;

            vm.OnLoginSuccess = async () => await RunSuccessAnimation();
            vm.OnLoginFail = async () => await RunErrorAnimation();
        }

        #region Hiệu ứng Thông báo (Success/Error)

        private async Task RunSuccessAnimation()
        {
            // Hiện thông báo thành công
            brdSuccess.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) };
            brdSuccess.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1000); // Đợi 1 giây để người dùng thấy thông báo

            // Chuẩn bị MainWindow
            MainWindow main = new MainWindow();
            main.Opacity = 0; // Để tàng hình trước khi fade in
            main.Show();

            // Hiệu ứng chuyển cảnh (Login mờ dần - Main hiện dần)
            var fadeOutLogin = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(200) };
            var fadeInMain = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) };

            fadeOutLogin.Completed += (s, ev) => this.Close();

            this.BeginAnimation(Window.OpacityProperty, fadeOutLogin);
            main.BeginAnimation(Window.OpacityProperty, fadeInMain);
        }

        private async Task RunErrorAnimation()
        {
            brdError.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400) };
            brdError.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1500); 

            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(400) };
            fadeOut.Completed += (s, a) => brdError.Visibility = Visibility.Collapsed;
            brdError.BeginAnimation(OpacityProperty, fadeOut);
        }

        #endregion

        #region Điều hướng & Hệ thống

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnShowRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            this.Hide();
            register.ShowDialog();

            // Sau khi đóng RegisterWindow, hiện lại màn hình Login
            if (Application.Current != null) this.Show();
        }

        #endregion

        #region Logic Ẩn/Hiện mật khẩu

        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            HidePassword();
        }

        private void Eye_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Visible)
            {
                HidePassword();
            }
        }

        private void HidePassword()
        {
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Focus();
        }

        #endregion
    }
}