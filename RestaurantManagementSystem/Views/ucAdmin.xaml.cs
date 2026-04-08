using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for ucAdmin.xaml
    /// </summary>
    public partial class ucAdmin : UserControl
    {
        public ucAdmin()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Mặc định khi vào Admin là mở trang Quản lý Món ăn (Trang số 0)
            tabMain.SelectedIndex = 0;
            HighlightButton(btnMenuFood);
        }

        // Hàm xử lý chung khi click vào các nút ở Sidebar
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBtn = sender as Button;

            // Chuyển trang tương ứng
            if (clickedBtn == btnMenuFood) tabMain.SelectedIndex = 0;
            else if (clickedBtn == btnMenuCategory) tabMain.SelectedIndex = 1;
            else if (clickedBtn == btnMenuAccount) tabMain.SelectedIndex = 2;
            else if (clickedBtn == btnMenuReport) tabMain.SelectedIndex = 3;
            else if (clickedBtn == btnMenuAudit) tabMain.SelectedIndex = 4;

            // Đổi màu nút đang chọn để Giám đốc biết đang ở trang nào
            HighlightButton(clickedBtn);
        }

        // Hàm tạo hiệu ứng làm nổi bật nút đang được chọn
        private void HighlightButton(Button activeBtn)
        {
            // 1. Reset màu toàn bộ nút về trong suốt
            btnMenuFood.Background = Brushes.Transparent;
            btnMenuCategory.Background = Brushes.Transparent;
            btnMenuAccount.Background = Brushes.Transparent;
            btnMenuReport.Background = Brushes.Transparent;
            btnMenuAudit.Background = Brushes.Transparent;

            // 2. Tô màu xanh dương đậm cho nút đang được click
            var bc = new BrushConverter();
            activeBtn.Background = (Brush)bc.ConvertFrom("#34495E");
        }
    }
}
