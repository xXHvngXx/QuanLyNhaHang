using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            IMessageService messageService = new MessageService();

            var vm = new RegisterViewModel(messageService);
            this.DataContext = vm;

            vm.OnRegisterSuccess = async () => await RunSuccessAnimation();
            vm.OnRegisterFail = async (header, msg) => await ShowErrorNotification(header, msg);
        }

        #region Animations

        private async Task RunSuccessAnimation()
        {
            brdSuccess.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) };
            brdSuccess.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1500);
            this.Close(); 
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

        #endregion

        #region Event Handlers

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtRegPassVisible.Text = txtRegPass.Password;
            txtRegPass.Visibility = Visibility.Collapsed;
            txtRegPassVisible.Visibility = Visibility.Visible;
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            txtRegPass.Visibility = Visibility.Visible;
            txtRegPassVisible.Visibility = Visibility.Collapsed;
            txtRegPass.Focus(); 
        }

        private void Eye_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtRegPassVisible.Visibility == Visibility.Visible)
            {
                txtRegPass.Visibility = Visibility.Visible;
                txtRegPassVisible.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}