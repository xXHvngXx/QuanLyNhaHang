using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RestaurantManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Đóng form đăng ký để quay lại form đăng nhập
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string user = txtRegUser.Text.Trim();
                string pass = txtRegPass.Password;
                // Để mặc định
                string displayName = "Nhân Viên Phục Vụ";

                // --- BƯỚC 1: KIỂM TRA ĐỊNH DẠNG ---
                if (user.Length < 5 || !Regex.IsMatch(user, @"^[a-zA-Z0-9]+$"))
                {
                    await ShowErrorNotification("Tên tài khoản không hợp lệ!", "Phải từ 5 ký tự và không chứa ký tự đặc biệt.");
                    return;
                }

                if (pass.Length < 3) // Tối thiểu 3 ký tự
                {
                    await ShowErrorNotification("Mật khẩu quá ngắn!", "Vui lòng nhập mật khẩu dài hơn.");
                    return;
                }

                // --- BƯỚC 2: KIỂM TRA TRÙNG LẶP (Dùng hàm trong DAL) ---
                if (AccountDAL.Instance.CheckAccountExist(user))
                {
                    await ShowErrorNotification("Tài khoản đã tồn tại!", "Vui lòng chọn một tên đăng nhập khác.");
                    return;
                }

                // --- BƯỚC 3: ĐĂNG KÝ (Role 1 - Staff) ---
                string hashedPass = SecurityHelper.HashPassword(pass);

                // Gọi hàm Insert từ DAL (0-Admin, 1-Staff, 2-Cashier)
                // Ở đây mình đăng ký mặc định là Staff (1)
                bool isSuccess = AccountDAL.Instance.InsertAccount(user, displayName, hashedPass, 1);

                if (isSuccess)
                {
                    // Hiện thông báo thành công
                    brdSuccess.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(500) };
                    brdSuccess.BeginAnimation(OpacityProperty, fadeIn);

                    await Task.Delay(1500);
                    this.Close(); // Đóng để quay về Login
                }
                else
                {
                    MessageBox.Show("Đăng ký thất bại, vui lòng thử lại!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }


        private async Task ShowErrorNotification(string header, string message)
        {
            txtErrorHeader.Text = header;
            txtErrorMessage.Text = message;
            brdError.Visibility = Visibility.Visible;

            // Fade In
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400) };
            brdError.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(2500); // Hiện trong 2.5 giây

            // Fade Out
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(400) };
            fadeOut.Completed += (s, a) => brdError.Visibility = Visibility.Collapsed;
            brdError.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
