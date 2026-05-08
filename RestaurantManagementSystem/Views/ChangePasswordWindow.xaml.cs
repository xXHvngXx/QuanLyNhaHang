using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();

            IMessageService messageService = new MessageService();

            this.DataContext = new ChangePasswordViewModel(messageService);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Logic Hiệu ứng Overlay & Thông báo

        private void btnShowConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra sơ bộ trước khi hiện bảng xác nhận
            if (string.IsNullOrWhiteSpace(txtNewPass.Password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
                // Kiểm tra command trước khi thực thi
                if (vm.ChangePasswordCommand != null && vm.ChangePasswordCommand.CanExecute(this))
                {
                    this.Tag = null;

                    // Thực thi logic đổi mật khẩu ở ViewModel
                    vm.ChangePasswordCommand.Execute(this);

                    // ViewModel sẽ set Tag="SUCCESS" nếu đổi thành công trong DB
                    if (this.Tag != null && this.Tag.ToString() == "SUCCESS")
                    {
                        await ShowSuccessAndClose();
                    }
                }
            }
        }

        private async Task ShowSuccessAndClose()
        {
            // Hiện thông báo xanh lá
            brdNotify.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
            brdNotify.BeginAnimation(OpacityProperty, fadeIn);

            // Chờ 1 giây để người dùng kịp đọc
            await Task.Delay(1000);

            // Hiệu ứng mờ dần toàn bộ cửa sổ trước khi đóng (Tùy chọn thêm)
            DoubleAnimation windowFade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            this.BeginAnimation(OpacityProperty, windowFade);

            await Task.Delay(200);
            this.Close();
        }

        #endregion

        #region Logic Ẩn/Hiện mật khẩu

        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn) HandlePassVisibility(btn, true);
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn) HandlePassVisibility(btn, false);
        }

        // Fix lỗi UX: Thả chuột ngoài phạm vi nút vẫn tự ẩn mật khẩu
        private void Eye_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button btn) HandlePassVisibility(btn, false);
        }

        private void HandlePassVisibility(Button btn, bool isShow)
        {
            if (btn?.Tag == null) return;
            string tag = btn.Tag.ToString();

            // Xác định cặp Control cần xử lý dựa trên Tag
            PasswordBox pBox = null;
            TextBox tBox = null;

            switch (tag)
            {
                case "Old":
                    pBox = txtOldPass; tBox = txtOldPassVisible;
                    break;
                case "New":
                    pBox = txtNewPass; tBox = txtNewPassVisible;
                    break;
                case "Confirm":
                    pBox = txtConfirmPass; tBox = txtConfirmPassVisible;
                    break;
            }

            if (pBox == null || tBox == null) return;

            if (isShow)
            {
                tBox.Text = pBox.Password;
                pBox.Visibility = Visibility.Collapsed;
                tBox.Visibility = Visibility.Visible;
            }
            else
            {
                pBox.Visibility = Visibility.Visible;
                tBox.Visibility = Visibility.Collapsed;
                // Focus lại vào PasswordBox 
                pBox.Focus();
            }
        }

        #endregion
    }
}