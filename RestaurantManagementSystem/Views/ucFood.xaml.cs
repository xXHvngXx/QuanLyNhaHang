using RestaurantManagementSystem.BLL;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RestaurantManagementSystem.Views
{
    public partial class ucFood : UserControl
    {
        public ucFood()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryIntoComboBox(); // Load danh mục vào ComboBox thả xuống
            RefreshData();              // Load danh sách món ăn vào bảng
        }

        private void LoadCategoryIntoComboBox()
        {
            
            cbCategory.ItemsSource = CategoryBLL.Instance.GetCategories().DefaultView;
        }

        private void RefreshData()
        {
            dgFood.ItemsSource = FoodBLL.Instance.GetFoods().DefaultView;

            // Xóa trắng form nhập liệu
            txtFoodID.Text = "";
            txtFoodName.Text = "";
            cbCategory.SelectedItem = null;
            txtPrice.Text = "";
            txtFoodName.Focus();
        }

        private void dgFood_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFood.SelectedItem is DataRowView row)
            {
                txtFoodID.Text = row["FoodID"].ToString();
                txtFoodName.Text = row["FoodName"].ToString();
                cbCategory.SelectedValue = row["CategoryID"]; // Tự động chọn đúng danh mục trong ComboBox
                txtPrice.Text = row["Price"].ToString();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            string msg = FoodBLL.Instance.AddFood(txtFoodName.Text, cbCategory.SelectedValue, txtPrice.Text);
            MessageBox.Show(msg, "Thông báo");
            RefreshData();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string msg = FoodBLL.Instance.UpdateFood(txtFoodID.Text, txtFoodName.Text, cbCategory.SelectedValue, txtPrice.Text);
            MessageBox.Show(msg, "Thông báo");
            RefreshData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa món ăn này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string msg = FoodBLL.Instance.DeleteFood(txtFoodID.Text);
                MessageBox.Show(msg, "Thông báo");
                RefreshData();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }


        public bool CheckDuplicateFood(string name, int categoryId, int excludeFoodId = -1)
        {
            // excludeFoodId dùng cho trường hợp Update (bỏ qua chính nó)
            string query = "SELECT COUNT(*) FROM Food WHERE FoodName = @name AND CategoryID = @catId AND FoodID != @id";
            System.Data.DataTable data = DataProvider.Instance.ExecuteQuery(query, new object[] { name, categoryId, excludeFoodId });

            return System.Convert.ToInt32(data.Rows[0][0]) > 0;
        }
    }
}
