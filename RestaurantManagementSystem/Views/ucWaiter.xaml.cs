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
    /// <summary>
    /// Interaction logic for ucWaiter.xaml
    /// </summary>
    public partial class ucWaiter : UserControl
    {
        int currentTableId = 0;

        public ucWaiter()
        {
            InitializeComponent();
            
        }

        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadTable();
                LoadCategory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        void LoadTable()
        {
            DataTable dt = DataProvider.Instance.ExecuteQuery("SELECT * FROM TableFood");
            icTables.ItemsSource = dt.DefaultView;
        }

        void LoadCategory()
        {
            DataTable dt = DataProvider.Instance.ExecuteQuery("SELECT * FROM Category");
            cbCategory.ItemsSource = dt.DefaultView;
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCategory.SelectedValue != null)
            {
                int id = Convert.ToInt32(cbCategory.SelectedValue);
                DataTable dt = DataProvider.Instance.ExecuteQuery("SELECT * FROM Food WHERE CategoryID = @id", new object[] { id });
                cbFood.ItemsSource = dt.DefaultView;
                if (dt.Rows.Count > 0) cbFood.SelectedIndex = 0;
            }
        }

        private void btnTable_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            //hover viền bàn
            icTables.SelectedItem = btn.DataContext;

            currentTableId = Convert.ToInt32(btn.Tag);
            txtSelectedTable.Text = "🛎️ Đang order cho: " + btn.Content.ToString();
            txtSelectedTable.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B894"));

            LoadBill(currentTableId);
        }

        void LoadBill(int tableID)
        {
            string query = @"
                SELECT f.FoodID, f.FoodName, bi.Quantity, f.Price, (bi.Quantity * f.Price) AS Total
                FROM Bill b
                JOIN BillInfo bi ON b.BillID = bi.BillID
                JOIN Food f ON bi.FoodID = f.FoodID
                WHERE b.TableID = " + tableID + " AND b.Status = 0";

            DataTable dt = DataProvider.Instance.ExecuteQuery(query, new object[] { tableID });
            dgOrderBill.ItemsSource = dt.DefaultView;
        }

        private void btnAddFood_Click(object sender, RoutedEventArgs e)
        {
            if (currentTableId == 0)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi order!");
                return;
            }
            if (cbFood.SelectedValue == null) return;

            int foodID = Convert.ToInt32(cbFood.SelectedValue);
            int quantity = 1;
            int.TryParse(txtQuantity.Text, out quantity);

            // Kiểm tra bàn đã có Bill chưa
            DataTable bill = DataProvider.Instance.ExecuteQuery("SELECT * FROM Bill WHERE TableID = @tableId AND Status = 0", new object[] { currentTableId });
            int billID;

            if (bill.Rows.Count == 0)
            {
                // Mở bàn mới
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO Bill (DateCheckIn, TableID, Status, UserName) VALUES (GETDATE(), @tableId , 0, N'admin')", new object[] { currentTableId });
                DataProvider.Instance.ExecuteNonQuery("UPDATE TableFood SET Status = 1 WHERE TableID = @tableId", new object[] { currentTableId });

                billID = Convert.ToInt32(DataProvider.Instance.ExecuteQuery("SELECT MAX(BillID) FROM Bill").Rows[0][0]);
            }
            else
            {
                billID = Convert.ToInt32(bill.Rows[0]["BillID"]);
            }

            // Gộp món
            DataTable exist = DataProvider.Instance.ExecuteQuery("SELECT * FROM BillInfo WHERE BillID = @billId AND FoodID = @foodId", new object[] { billID, foodID });

            if (exist.Rows.Count > 0)
            {
                DataProvider.Instance.ExecuteNonQuery("UPDATE BillInfo SET Quantity += @qty WHERE BillID = @billId AND FoodID = @foodId", new object[] { quantity, billID, foodID });
            }
            else
            {
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO BillInfo (BillID, FoodID, Quantity) VALUES ( @billId , @foodId , @qty )", new object[] { billID, foodID, quantity });
            }

            LoadBill(currentTableId);
            LoadTable(); // Cập nhật màu bàn thành Đỏ
        }
        // 1. SỬA MÓN (Cập nhật món mới và số lượng mới)
        private void btnUpdateFood_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra điều kiện chọn bàn và chọn món trong bảng
            if (currentTableId == 0)
            {
                MessageBox.Show("Vui lòng chọn một bàn trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgOrderBill.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng click chọn dòng món ăn trong bảng cần sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Lấy thông tin món CŨ từ dòng đang chọn trong DataGrid
                var selectedRow = dgOrderBill.SelectedItem as System.Data.DataRowView;
                int oldFoodID = (int)selectedRow["FoodID"];

                // 3. Lấy thông tin món MỚI và Số lượng từ ComboBox/TextBox
                if (cbFood.SelectedItem == null) return;
                var newFoodRow = cbFood.SelectedItem as System.Data.DataRowView;
                int newFoodID = (int)newFoodRow["FoodID"];
                int newQuantity = int.Parse(txtQuantity.Text);

                // 4. Tìm BillID hiện tại của bàn
                DataTable dataBill = DataProvider.Instance.ExecuteQuery(
                    "SELECT BillID FROM Bill WHERE TableID = " + currentTableId + " AND Status = 0");

                if (dataBill.Rows.Count > 0)
                {
                    int billID = (int)dataBill.Rows[0]["BillID"];

                    // A. Xóa món cũ (để tránh lỗi trùng khóa chính nếu cậu đổi sang món đã có trong bill)
                    DataProvider.Instance.ExecuteNonQuery(
                        "DELETE FROM BillInfo WHERE BillID = " + billID + " AND FoodID = " + oldFoodID);

                    // B. Thêm/Cập nhật món mới 
                    DataTable exist = DataProvider.Instance.ExecuteQuery(
                        "SELECT * FROM BillInfo WHERE BillID = " + billID + " AND FoodID = " + newFoodID);

                    if (exist.Rows.Count > 0)
                    {
                        DataProvider.Instance.ExecuteNonQuery(
                            "UPDATE BillInfo SET Quantity += " + newQuantity +
                            " WHERE BillID = " + billID + " AND FoodID = " + newFoodID);
                    }
                    else
                    {
                        DataProvider.Instance.ExecuteNonQuery(
                            "INSERT INTO BillInfo (BillID, FoodID, Quantity) VALUES (" + billID + "," + newFoodID + "," + newQuantity + ")");
                    }

                    // 5. Cập nhật lại giao diện
                    LoadBill(currentTableId);
                    MessageBox.Show("Đã cập nhật món thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. XÓA MÓN
        private void btnDeleteFood_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem đã chọn dòng nào chưa
            if (dgOrderBill.SelectedItem == null) return;

            // Lấy dòng đang chọn và ép kiểu về DataRowView
            var selectedRow = dgOrderBill.SelectedItem as System.Data.DataRowView;
            if (selectedRow != null)
            {
                // Lấy FoodID từ cột dữ liệu 
                int foodID = (int)selectedRow["FoodID"];

                if (MessageBox.Show("Xác nhận xóa món này?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DataTable dataBill = DataProvider.Instance.ExecuteQuery("SELECT BillID FROM Bill WHERE TableID = " + currentTableId + " AND Status = 0");

                    if (dataBill.Rows.Count > 0)
                    {
                        int billID = (int)dataBill.Rows[0]["BillID"];
                        DataProvider.Instance.ExecuteNonQuery("DELETE FROM BillInfo WHERE BillID = " + billID + " AND FoodID = " + foodID);
                        LoadBill(currentTableId);
                    }
                }
            }
        }
    }
}
