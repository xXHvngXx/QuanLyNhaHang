using RestaurantManagementSystem.DAL;
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
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();

            var vm = new ForgotPasswordViewModel();
            this.DataContext = vm;

            // KÍCH HOẠT HÀM HIỆU ỨNG MƯỢT MÀ BẠN ĐÃ VIẾT KHI VIEWMODEL PHẢN HỒI
            vm.OnResetSuccess = () => Dispatcher.Invoke(async () => await RunSuccessAnimation());
            vm.OnResetFail = (msg) => Dispatcher.Invoke(async () => await RunErrorAnimation(msg));
        }

        #region Hiệu ứng Thông báo (Giữ nguyên các hàm mượt mà của bạn)

        private async Task RunSuccessAnimation()
        {
            // Hiện thô lập tức, bỏ qua hoàn toàn Animation
            brdSuccess.Visibility = Visibility.Visible;
            brdSuccess.Opacity = 1;

            MessageBox.Show("DEBUG: Logic THÀNH CÔNG đã chạy!");

            await Task.Delay(2000);
            this.Close();
        }

        private async Task RunErrorAnimation(string message)
        {
            if (txtErrorDetail != null) txtErrorDetail.Text = message;

            // Hiện thô lập tức, ép Opacity = 1 để kiểm tra lỗi render trong suốt
            brdError.Visibility = Visibility.Visible;
            brdError.Opacity = 1;

            MessageBox.Show($"DEBUG LỖI: {message}");

            await Task.Delay(2500);
            brdError.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Xử lý Ẩn/Hiện Mật khẩu
        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtNewPasswordVisible.Text = pwbNewPassword.Password;
            pwbNewPassword.Visibility = Visibility.Collapsed;
            txtNewPasswordVisible.Visibility = Visibility.Visible;
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            pwbNewPassword.Visibility = Visibility.Visible;
            txtNewPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void Eye_MouseLeave(object sender, MouseEventArgs e) => Eye_PreviewMouseUp(null, null);

        private void Eye_ConfirmMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtConfirmPasswordVisible.Text = pwbConfirmPassword.Password;
            pwbConfirmPassword.Visibility = Visibility.Collapsed;
            txtConfirmPasswordVisible.Visibility = Visibility.Visible;
        }

        private void Eye_ConfirmMouseUp(object sender, MouseButtonEventArgs e)
        {
            pwbConfirmPassword.Visibility = Visibility.Visible;
            txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
        }

        private void Eye_ConfirmMouseLeave(object sender, MouseEventArgs e) => Eye_ConfirmMouseUp(null, null);
        #endregion

        #region Sự kiện khác
        private void btnClose_Click(object sender, RoutedEventArgs e) => this.Close();

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string uName = txtUserName.Text;
            string dName = txtDisplayName.Text;
            string newPass = pwbNewPassword.Password;
            string confPass = pwbConfirmPassword.Password;

            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(uName) || string.IsNullOrWhiteSpace(dName) ||
                string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confPass))
            {
                SmoothNotification(brdError, "Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // 2. Kiểm tra khớp mật khẩu
            if (newPass != confPass)
            {
                SmoothNotification(brdError, "Mật khẩu xác nhận không khớp!");
                return;
            }

            // 3. Kiểm tra DB
            if (!AccountDAL.Instance.CheckAccountExist(uName))
            {
                SmoothNotification(brdError, "Tài khoản không tồn tại.");
                return;
            }

            string realDisplayName = AccountDAL.Instance.GetDisplayNameByUserName(uName);
            if (dName != realDisplayName)
            {
                SmoothNotification(brdError, "Tên hiển thị không khớp.");
                return;
            }

            // 4. Tiến hành đổi mật khẩu
            string hashedPass = SecurityHelper.HashPassword(newPass);
            if (AccountDAL.Instance.ResetPassword(uName, hashedPass))
            {
                SmoothNotification(brdSuccess); // Hiện thông báo thành công mượt mà

                // Đợi 2 giây trải nghiệm thành công rồi đóng form
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                timer.Tick += (s, args) => { timer.Stop(); this.Close(); };
                timer.Start();
            }
            else
            {
                SmoothNotification(brdError, "Có lỗi xảy ra khi cập nhật!");
            }
        }
        private void SmoothNotification(Border targetBorder, string message = null)
        {
            if (targetBorder == brdError && message != null)
            {
                txtErrorDetail.Text = message;
            }

            targetBorder.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(300) };
            targetBorder.BeginAnimation(OpacityProperty, fadeIn);

            if (targetBorder == brdError)
            {
                var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(300) };

                    fadeOut.Completed += (sender, e) => targetBorder.Visibility = Visibility.Collapsed;

                    targetBorder.BeginAnimation(OpacityProperty, fadeOut);
                };
                timer.Start();
            }
        }
        #endregion
    }
}