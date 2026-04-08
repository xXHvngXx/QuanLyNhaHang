using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PhanQuyen();
        }
        private void PhanQuyen()
        {
            if (AccountDAL.LoginAccount == null) return;

            // Lấy Role chuẩn từ SQL: 0-Admin, 1-Staff, 2-Cashier
            int role = (int)AccountDAL.LoginAccount["Role"];

            // 1. Phân quyền cho phần QUẢN TRỊ (Cụm nút đỏ)
            if (role == 0)
            {
                spAdminSection.Visibility = Visibility.Visible;
            }
            else
            {
                // Staff (1) và Cashier (2) đều không được đụng vào Quản trị
                spAdminSection.Visibility = Visibility.Collapsed;
            }

            // 2. Phân quyền chi tiết cho phần NGHIỆP VỤ
            switch (role)
            {
                case 0: // Admin: Thấy hết để đi kiểm tra lính
                    btnPhucVu.Visibility = Visibility.Visible;
                    btnThuNgan.Visibility = Visibility.Visible;
                    break;

                case 1: // Staff: Chỉ làm Phục vụ
                    btnPhucVu.Visibility = Visibility.Visible;
                    btnThuNgan.Visibility = Visibility.Collapsed;
                    break;

                case 2: // Cashier: Chỉ làm Thu ngân
                    btnPhucVu.Visibility = Visibility.Collapsed;
                    btnThuNgan.Visibility = Visibility.Visible;
                    break;
            }
        }
        // Khi bấm nút Phục Vụ, lôi ucWaiter ra nhét vào khung
        private void btnPhucVu_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ucWaiter();
        }

        // Khi bấm nút Thu Ngân, lôi ucCashier ra nhét vào khung
        private void btnThuNgan_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ucCashier();
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            // Gọi UserControl ucAdmin mà cậu vừa tạo nhét vào MainContent
            MainContent.Content = new ucAdmin();
        }
    }
}