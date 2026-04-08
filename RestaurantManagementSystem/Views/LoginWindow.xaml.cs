using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            
            Application.Current.Shutdown();
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
{
    try
    {
        string userName = txtUsername.Text.Trim();
        string passWord = txtPassword.Password.Trim();

        //Tạm khóa nút Đăng nhập lại để tránh người dùng bấm liên tục nhiều lần lúc đang chờ
        btnLogin.IsEnabled = false;

        
        // Từ khóa 'await' sẽ đợi luồng ngầm làm xong mà không làm đơ giao diện chính
        bool isLoginSuccess = await Task.Run(() => Login(userName, passWord));

        //Sau khi luồng ngầm tính xong, trả kết quả về đây
        if (isLoginSuccess)
        {
            //Display
            brdSuccess.Visibility = Visibility.Visible;

            //Fade In 
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            brdSuccess.BeginAnimation(OpacityProperty, fadeIn);
            
            await Task.Delay(1000);

            MainWindow main = new MainWindow();
            main.Opacity = 0;
            main.Show();

            var fadeOutLogin = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(100) 
            };

            var fadeInMain = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) 
            };

            fadeOutLogin.Completed += (s, ev) =>
            {
                this.Close();
            };

            this.BeginAnimation(Window.OpacityProperty, fadeOutLogin);
            main.BeginAnimation(Window.OpacityProperty, fadeInMain);
        }
        else
        {
            // Mở khóa lại nút Đăng nhập nếu sai mật khẩu để người dùng thử lại
            btnLogin.IsEnabled = true; 

            //Display
            brdError.Visibility = Visibility.Visible;

            //Fade In
            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(400)
            };
            brdError.BeginAnimation(OpacityProperty, fadeIn);
            
            await Task.Delay(1000);

            //Fade Out
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(400)
            };
            fadeOut.Completed += (s, a) => brdError.Visibility = Visibility.Collapsed;
            brdError.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
    catch (Exception ex)
    {
        btnLogin.IsEnabled = true; 
        MessageBox.Show("Lỗi hệ thống: " + ex.Message);
    }
}

        // Hàm kiểm tra logic đăng nhập
        bool Login(string userName, string passWord)
        {
            string query = "SELECT * FROM Account WHERE UserName = @user";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result.Rows.Count > 0)
            {
                string hashFromDB = result.Rows[0]["Password"].ToString();

                if (SecurityHelper.VerifyPassword(passWord, hashFromDB))
                {
                    // Lưu lại toàn bộ thông tin người dùng vào bộ nhớ tạm
                    AccountDAL.LoginAccount = result.Rows[0];

                    return true; // Đăng nhập thành công
                }
            }
            return false;
        }
        private void btnShowRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow register = new RegisterWindow();
            this.Hide(); // Ẩn Login 

            register.ShowDialog(); 

            // Kiểm tra xem Application.Current có còn tồn tại không
            if (Application.Current != null)
            {
                this.Show();
            }
        }
    }
}
