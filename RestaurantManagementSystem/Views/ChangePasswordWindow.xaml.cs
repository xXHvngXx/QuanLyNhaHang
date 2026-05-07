using RestaurantManagementSystem.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
            this.DataContext = new ChangePasswordViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Logic Hiệu ứng Overlay & Thông báo

        private void btnShowConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewPass.Password)) return;
            confirmOverlay.Visibility = Visibility.Visible;
        }

        private void btnCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            confirmOverlay.Visibility = Visibility.Collapsed;
        }

        private async void btnFinalConfirm_Click(object sender, RoutedEventArgs e)
        {
            confirmOverlay.Visibility = Visibility.Collapsed;

            if (this.DataContext is ChangePasswordViewModel vm)
            {
                if (vm.ChangePasswordCommand != null && vm.ChangePasswordCommand.CanExecute(this))
                {
                    this.Tag = null;

                    // Chạy lệnh đổi mật khẩu
                    vm.ChangePasswordCommand.Execute(this);

                    if (this.Tag != null && this.Tag.ToString() == "SUCCESS")
                    {
                        await ShowSuccessAndClose();
                    }
                }
            }
        }

        private async Task ShowSuccessAndClose()
        {
            brdNotify.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
            brdNotify.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(1000);

            this.Hide();

            await Task.Delay(200);
            this.Close();
        }

        #endregion

        #region Logic Ẩn/Hiện mật khẩu

        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn?.Tag == null) return;

            string tag = btn.Tag.ToString();
            if (tag == "Old") ShowPass(txtOldPass, txtOldPassVisible);
            else if (tag == "New") ShowPass(txtNewPass, txtNewPassVisible);
            else if (tag == "Confirm") ShowPass(txtConfirmPass, txtConfirmPassVisible);
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn?.Tag == null) return;

            string tag = btn.Tag.ToString();
            if (tag == "Old") HidePass(txtOldPass, txtOldPassVisible);
            else if (tag == "New") HidePass(txtNewPass, txtNewPassVisible);
            else if (tag == "Confirm") HidePass(txtConfirmPass, txtConfirmPassVisible);
        }

        private void ShowPass(System.Windows.Controls.PasswordBox p, System.Windows.Controls.TextBox t)
        {
            t.Text = p.Password;
            p.Visibility = Visibility.Collapsed;
            t.Visibility = Visibility.Visible;
        }

        private void HidePass(System.Windows.Controls.PasswordBox p, System.Windows.Controls.TextBox t)
        {
            p.Visibility = Visibility.Visible;
            t.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}