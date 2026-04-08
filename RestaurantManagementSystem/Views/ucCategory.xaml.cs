using System.Windows;
using System.Windows.Controls;
using System.Data;
using RestaurantManagementSystem.BLL; // Nhớ thêm dòng này để gọi được BLL

namespace RestaurantManagementSystem.Views
{
    public partial class ucCategory : UserControl
    {
        public ucCategory()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        // Hàm làm mới danh sách DataGrid
        private void RefreshData()
        {
            // UI chỉ gọi BLL, không biết SQL là gì cả
            dgCategory.ItemsSource = CategoryBLL.Instance.GetCategories().DefaultView;

            // Xóa trắng các ô nhập liệu sau khi thao tác
            txtCategoryID.Text = "";
            txtCategoryName.Text = "";
            txtCategoryName.Focus();
        }

        // Khi click vào một dòng trên bảng, hiển thị dữ liệu sang bên trái
        private void dgCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategory.SelectedItem is DataRowView row)
            {
                txtCategoryID.Text = row["CategoryID"].ToString();
                txtCategoryName.Text = row["CategoryName"].ToString();
            }
        }

        // SỰ KIỆN NÚT THÊM
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string name = txtCategoryName.Text;
            string message = CategoryBLL.Instance.AddCategory(name);

            MessageBox.Show(message, "Thông báo");
            RefreshData();
        }

        // SỰ KIỆN NÚT SỬA
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string id = txtCategoryID.Text;
            string name = txtCategoryName.Text;
            string message = CategoryBLL.Instance.UpdateCategory(id, name);

            MessageBox.Show(message, "Thông báo");
            RefreshData();
        }

        // SỰ KIỆN NÚT XÓA
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa danh mục này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string id = txtCategoryID.Text;
                string message = CategoryBLL.Instance.DeleteCategory(id);

                MessageBox.Show(message, "Thông báo");
                RefreshData();
            }
        }

        // NÚT NHẬP LẠI (CLEAR)
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}