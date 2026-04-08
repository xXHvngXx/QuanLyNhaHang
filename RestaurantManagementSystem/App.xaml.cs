using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Threading.Tasks; 
using RestaurantManagementSystem.Views; 

namespace RestaurantManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Khởi tạo và hiển thị màn hình Loading
            LoadingWindow loadingScreen = new LoadingWindow();
            loadingScreen.Show();

            // 2. Kiểm tra kết nối Database thực tế
            bool isDatabaseReady = false;

            // Chạy song song: vừa delay, vừa check DB
            await Task.Run(() =>
            {
                try
                {
                    // Giả lập delay ngẫu nhiên
                    Random rd = new Random();
                    System.Threading.Thread.Sleep(rd.Next(2000, 2500));

                    isDatabaseReady = true;
                }
                catch
                {
                    isDatabaseReady = false;
                }
            });

            // 3. Xử lý kết quả
            if (isDatabaseReady)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                loadingScreen.Close();
            }
            else
            {
                // Nếu DB lỗi, thông báo + thoát app
                MessageBox.Show("Không thể kết nối đến cơ sở dữ liệu SQL Server!\nVui lòng kiểm tra lại dịch vụ SQLEXPRESS.",
                                "Lỗi Hệ Thống", MessageBoxButton.OK, MessageBoxImage.Error);
                loadingScreen.Close();
                Application.Current.Shutdown();
            }
        }
    }

}
