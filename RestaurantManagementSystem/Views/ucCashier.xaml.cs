using RestaurantManagementSystem.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace RestaurantManagementSystem.Views
{
    public partial class ucCashier : UserControl
    {
        decimal currentTotal = 0;
        int currentTableId = 0;
        int currentBillId = 0;

        public ucCashier() { InitializeComponent(); }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try { LoadTable(); LoadCategory(); }
            catch (Exception ex) { ShowNotify(false, "LỖI HỆ THỐNG", "Không thể tải dữ liệu: " + ex.Message); }
        }

        // --- HÀM THÔNG BÁO "STYLE LOGIN" ---
        private async void ShowNotify(bool isSuccess, string title, string message)
        {
            Border target = isSuccess ? brdSuccess : brdError;
            if (isSuccess)
            {
                txtSuccessTitle.Text = title;
                txtSuccessMsg.Text = message;
            }
            else
            {
                txtErrorTitle.Text = title;
                txtErrorMsg.Text = message;
            }

            target.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            target.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(2000);

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3));
            fadeOut.Completed += (s, e) => target.Visibility = Visibility.Collapsed;
            target.BeginAnimation(OpacityProperty, fadeOut);
        }

        void LoadTable()
        {
            DataTable dataTable = DataProvider.Instance.ExecuteQuery("SELECT * FROM TableFood");
            lvTable.ItemsSource = dataTable.DefaultView;
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

        private void lvTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvTable.SelectedItem is DataRowView rowView)
            {
                currentTableId = Convert.ToInt32(rowView["TableID"]);
                LoadBill(currentTableId);
            }
        }

        void LoadBill(int tableID)
        {
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("SELECT * FROM Bill WHERE TableID = @tableId AND Status = 0", new object[] { tableID });
            if (dtBill.Rows.Count > 0)
            {
                currentBillId = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                DataTable dtBillInfo = DataProvider.Instance.ExecuteQuery(@"
                    SELECT f.FoodID, f.FoodName, bi.Quantity, f.Price, (bi.Quantity * f.Price) AS Total, bi.Note 
                    FROM BillInfo bi JOIN Food f ON bi.FoodID = f.FoodID 
                    WHERE bi.BillID = @billId", new object[] { currentBillId });
                dgBill.ItemsSource = dtBillInfo.DefaultView;
                currentTotal = 0;
                foreach (DataRow row in dtBillInfo.Rows) currentTotal += Convert.ToDecimal(row["Total"]);
                txtTotal.Text = currentTotal.ToString("N0");
            }
            else { dgBill.ItemsSource = null; txtTotal.Text = "0"; currentTotal = 0; currentBillId = 0; }
        }

        private void btnAddFood_Click(object sender, RoutedEventArgs e)
        {
            if (currentTableId == 0) { ShowNotify(false, "LỖI CHỌN BÀN", "Vui lòng chọn bàn trước khi thêm món!"); return; }
            if (cbFood.SelectedValue == null) return;

            int foodID = Convert.ToInt32(cbFood.SelectedValue);
            int quantity = int.TryParse(txtQuantity.Text, out int q) ? q : 1;
            string note = txtNote.Text;

            if (currentBillId == 0)
            {
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO Bill (DateCheckIn, TableID, Status) VALUES (GETDATE(), @tId , 0)", new object[] { currentTableId });
                DataProvider.Instance.ExecuteNonQuery("UPDATE TableFood SET Status = 1 WHERE TableID = @tId", new object[] { currentTableId });
            }
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("SELECT BillID FROM Bill WHERE TableID = @tId AND Status = 0", new object[] { currentTableId });
            currentBillId = Convert.ToInt32(dtBill.Rows[0][0]);

            DataTable exist = DataProvider.Instance.ExecuteQuery("SELECT * FROM BillInfo WHERE BillID = @bId AND FoodID = @fId", new object[] { currentBillId, foodID });
            if (exist.Rows.Count > 0)
                DataProvider.Instance.ExecuteNonQuery("UPDATE BillInfo SET Quantity += @qty, Note = @note WHERE BillID = @bId AND FoodID = @fId", new object[] { quantity, note, currentBillId, foodID });
            else
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO BillInfo (BillID, FoodID, Quantity, Note) VALUES (@bId , @fId , @qty , @note)", new object[] { currentBillId, foodID, quantity, note });

            txtNote.Text = ""; LoadBill(currentTableId); LoadTable();
            ShowNotify(true, "THÊM MÓN XONG", "Đã cập nhật danh sách gọi món.");
        }

        private void btnUpdateFood_Click(object sender, RoutedEventArgs e)
        {
            if (dgBill.SelectedItem is DataRowView row)
            {
                if (cbFood.SelectedValue == null) return;
                DataProvider.Instance.ExecuteNonQuery("DELETE FROM BillInfo WHERE BillID = @bId AND FoodID = @fId", new object[] { currentBillId, (int)row["FoodID"] });
                btnAddFood_Click(null, null);
                ShowNotify(true, "CẬP NHẬT XONG", "Đã sửa đổi thông tin món ăn.");
            }
        }

        private void btnDeleteFood_Click(object sender, RoutedEventArgs e)
        {
            if (dgBill.SelectedItem is DataRowView row)
            {
                string foodName = row["FoodName"].ToString();
                decimal itemTotal = Convert.ToDecimal(row["Total"]);

                // GHI LOG: "Thu ngân chuẩn bị xóa món X"
                RecordShadow("DELETE", $"Xóa món: {foodName}", currentTotal, currentTotal - itemTotal);




                // Confirmation vẫn nên dùng MessageBox vì ShowNotify không có nút chọn Yes/No
                if (MessageBox.Show("Xác nhận xóa món này khỏi hóa đơn?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DataProvider.Instance.ExecuteNonQuery("DELETE FROM BillInfo WHERE BillID = @bId AND FoodID = @fId", new object[] { currentBillId, (int)row["FoodID"] });
                    LoadBill(currentTableId);
                    ShowNotify(true, "ĐÃ XÓA MÓN", "Món ăn đã được gỡ khỏi hóa đơn.");
                }
            }
        }

        private void btnShowBill_Click(object sender, RoutedEventArgs e)
        {
            if (currentTableId == 0 || currentBillId == 0) { ShowNotify(false, "CHƯA CÓ ĐƠN", "Bàn này hiện chưa có món ăn nào!"); return; }
            if (lvTable.SelectedItem is DataRowView selectedTable)
            {
                txtPrintTable.Text = "Bàn: " + selectedTable["TableName"].ToString();
                txtPrintDate.Text = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy");
                txtPrintTime.Text = "Giờ: " + DateTime.Now.ToString("HH:mm");
                txtPrintTotal.Text = currentTotal.ToString("N0");
                itemsPrintBill.ItemsSource = dgBill.ItemsSource;
                receiptOverlay.Visibility = Visibility.Visible;
            }
        }

        private void btnThanhToan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra xem có đang chọn bàn/hóa đơn nào không
            if (currentTableId == 0 || currentBillId == 0)
            {
                ShowNotify(false, "CHƯA CHỌN BÀN", "Vui lòng chọn bàn có khách để thanh toán!");
                return;
            }

            // 2. Kiểm tra tính hợp lệ của tiền khách đưa
            if (!decimal.TryParse(txtCustomerMoney.Text, out decimal customerMoney))
            {
                ShowNotify(false, "LỖI NHẬP LIỆU", "Số tiền khách đưa không hợp lệ. Vui lòng nhập số!");
                txtCustomerMoney.Focus();
                return;
            }

            // 3. KIỂM TRA QUAN TRỌNG: Khách có đưa đủ tiền không?
            if (customerMoney < currentTotal)
            {
                decimal conThieu = currentTotal - customerMoney;
                ShowNotify(false, "THIẾU TIỀN", $"Khách đưa thiếu {conThieu:N0} VNĐ. Vui lòng thu đủ trước khi chốt!");
                txtCustomerMoney.Focus();
                return; // Dừng hàm tại đây, không cập nhật Database
            }



            RecordShadow("FINAL_PAY", "Chốt thanh toán, đóng bàn", currentTotal, currentTotal);



            // 4. NẾU ĐỦ TIỀN MỚI THỰC HIỆN CẬP NHẬT DATABASE
            try
            {
                // Chốt hóa đơn và giải phóng bàn
                DataProvider.Instance.ExecuteNonQuery("UPDATE Bill SET Status = 1, DateCheckOut = GETDATE() WHERE BillID = @bId", new object[] { currentBillId });
                DataProvider.Instance.ExecuteNonQuery("UPDATE TableFood SET Status = 0 WHERE TableID = @tId", new object[] { currentTableId });

                // Hiện thông báo thành công theo style Login 
                ShowNotify(true, "THANH TOÁN THÀNH CÔNG", $"Đã chốt bill bàn {currentTableId}!");

                // 5. LÀM SẠCH GIAO DIỆN SAU KHI XONG
                dgBill.ItemsSource = null;
                txtTotal.Text = "0";
                txtCustomerMoney.Text = "";
                txtChange.Text = "0";
                lvTable.SelectedItem = null;
                LoadTable(); // Tải lại danh sách bàn để cập nhật màu Xanh
            }
            catch (Exception ex)
            {
                ShowNotify(false, "LỖI DATABASE", "Không thể cập nhật thanh toán: " + ex.Message);
            }
        }

        private void btnExportReceipt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new PrintDialog();
                FrameworkElement container = printTicket.Parent as FrameworkElement;
                if (container == null) return;

                double originalMaxHeight = container.MaxHeight;
                container.MaxHeight = double.PositiveInfinity;
                double safetyMargin = 20;
                double printableWidth = pd.PrintableAreaWidth - (safetyMargin * 2);
                double scale = printableWidth / container.ActualWidth;
                if (scale > 1) scale = 1;

                container.LayoutTransform = new ScaleTransform(scale, scale);
                Size pageSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                container.Measure(pageSize);
                container.Arrange(new Rect(new Point(safetyMargin, safetyMargin), pageSize));
                container.UpdateLayout();

                pd.PrintVisual(container, "HoaDon_RestaurantManagement");

                container.LayoutTransform = null;
                container.MaxHeight = originalMaxHeight;
                receiptOverlay.Visibility = Visibility.Collapsed;

                ShowNotify(true, "XUẤT BILL XONG", "Hóa đơn PDF đã được lưu thành công.");
            }
            catch (Exception)
            {
                if (printTicket.Parent is FrameworkElement container) container.LayoutTransform = null;
                receiptOverlay.Visibility = Visibility.Collapsed;
                // Bấm Cancel ở Save As PDF sẽ nhảy vào đây - Silent Mode
            }

            // GHI LOG: "Đã in Bill tạm tính cho khách xem con số này"
            RecordShadow("PRINT_BILL", "Xuất hóa đơn tạm tính cho khách", currentTotal, currentTotal);
        }

        private void btnCloseReceipt_Click(object sender, RoutedEventArgs e)
        {
            receiptOverlay.Visibility = Visibility.Collapsed;
        }

        private void txtCustomerMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtCustomerMoney.Text, out decimal m))
            {
                decimal c = m - currentTotal;
                txtChange.Text = c >= 0 ? c.ToString("N0") : "Thiếu!";
            }
            else txtChange.Text = "0";
        }


        private void RecordShadow(string action, string desc, decimal oldTotal, decimal newTotal)
        {
            try
            {
                string tName = "N/A";
                if (lvTable.SelectedItem is DataRowView row)
                    tName = row["TableName"].ToString();

                // Thêm cột BillID vào câu lệnh INSERT
                string query = "INSERT INTO ShadowLog (TableName, ActionType, OldTotal, NewTotal, Description, BillID) " +
                               "VALUES ( @tName , @action , @old , @new , @desc , @bId )";

                // Truyền currentBillId vào mảng tham số
                DataProvider.Instance.ExecuteNonQuery(query, new object[] { tName, action, oldTotal, newTotal, desc, currentBillId });
            }
            catch
            {
                // Im lặng 
            }
        }


    }
}