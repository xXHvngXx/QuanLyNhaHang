using QRCoder;
using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Views;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RestaurantManagementSystem.ViewModels
{
    public class CashierViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;
        public Action OnPrintSuccess { get; set; }

        #region Properties
        private DataView _tables;
        public DataView Tables { get => _tables; set => SetProperty(ref _tables, value); }

        // Bộ lọc tìm kiếm danh sách bàn thời gian thực
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyTableFilter();
                }
            }
        }

        private string _searchFoodText = "";
        public string SearchFoodText
        {
            get => _searchFoodText;
            set
            {
                if (SetProperty(ref _searchFoodText, value))
                {
                    ApplyFoodFilter();
                }
            }
        }

        // Nguồn dữ liệu đã lọc liên kết trực tiếp với ListBox Bàn trên giao diện
        private DataView _filteredTables;
        public DataView FilteredTables { get => _filteredTables; set => SetProperty(ref _filteredTables, value); }

        private DataView _categories;
        public DataView Categories { get => _categories; set => SetProperty(ref _categories, value); }

        private DataView _foods;
        public DataView Foods { get => _foods; set => SetProperty(ref _foods, value); }

        private DataView _billDetails;
        public DataView BillDetails { get => _billDetails; set => SetProperty(ref _billDetails, value); }

        private decimal _subTotal;
        public decimal SubTotal { get => _subTotal; set => SetProperty(ref _subTotal, value); }

        private decimal _totalAmount;
        public decimal TotalAmount { get => _totalAmount; set => SetProperty(ref _totalAmount, value); }

        private DataRowView _selectedTable;
        public DataRowView SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value) && value != null)
                    OnTableSelected();
            }
        }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                    OnCategorySelected();
            }
        }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood { get => _selectedFood; set => SetProperty(ref _selectedFood, value); }

        private DataRowView _selectedBillDetail;
        public DataRowView SelectedBillDetail
        {
            get => _selectedBillDetail;
            set
            {
                if (_selectedBillDetail != null)
                {
                    SaveCurrentNote();
                }

                if (SetProperty(ref _selectedBillDetail, value))
                {
                    if (value != null)
                    {
                        Quantity = Convert.ToInt32(value["Quantity"]);

                        Note = value.Row.Table.Columns.Contains("Note") ? value["Note"].ToString() : "";

                        if (Categories != null && value.Row.Table.Columns.Contains("CategoryID"))
                        {
                            int targetCategoryId = Convert.ToInt32(value["CategoryID"]);
                            foreach (DataRowView categoryRow in Categories)
                            {
                                if (Convert.ToInt32(categoryRow["CategoryID"]) == targetCategoryId)
                                {
                                    SelectedCategory = categoryRow;
                                    break;
                                }
                            }
                        }

                        if (Foods != null && value.Row.Table.Columns.Contains("FoodID"))
                        {
                            int targetFoodId = Convert.ToInt32(value["FoodID"]);
                            foreach (DataRowView foodRow in Foods)
                            {
                                if (Convert.ToInt32(foodRow["FoodID"]) == targetFoodId)
                                {
                                    SelectedFood = foodRow;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Note = "";
                    }
                }
            }
        }

        public int CurrentBillId { get; set; }

        private string _note = "";
        public string Note { get => _note; set => SetProperty(ref _note, value); }

        private int _quantity = 1;
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; set; }
        public ICommand AddFoodCommand { get; set; }
        public ICommand UpdateFoodCommand { get; set; }
        public ICommand DeleteFoodCommand { get; set; }
        public ICommand PayCommand { get; set; }
        public ICommand PrintBillCommand { get; set; }
        #endregion

        public CashierViewModel(IMessageService messageService)
        {
            _messageService = messageService;
            InitCommands();
        }

        private void InitCommands()
        {
            LoadDataCommand = new RelayCommand<object>(p => { LoadTables(); LoadCategories(); });

            AddFoodCommand = new RelayCommand<object>(
                p => ExecuteAddFood(),
                p => SelectedTable != null && SelectedFood != null);

            UpdateFoodCommand = new RelayCommand<object>(
                p => ExecuteUpdateFood(),
                p => SelectedBillDetail != null);

            DeleteFoodCommand = new RelayCommand<object>(
                p => ExecuteDeleteFood(),
                p => SelectedBillDetail != null);

            PayCommand = new RelayCommand<object>(
                p => ExecutePay(),
                p => CurrentBillId != 0);

            PrintBillCommand = new RelayCommand<object>(
                p => ExecutePrintBill(),
                p => CurrentBillId != 0 && BillDetails != null);
        }

        #region Data Loading Logic
        void LoadTables()
        {
            // Bảo lưu ID bàn đang chọn trước khi làm mới dữ liệu từ Database
            int savedTableId = SelectedTable != null ? Convert.ToInt32(SelectedTable["TableID"]) : -1;

            DataTable dt = DataProvider.Instance.ExecuteQuery("EXEC USP_GetTableList");
            Tables = dt.DefaultView;

            // Đồng bộ dữ liệu hiển thị cho FilteredTables của thanh tìm kiếm
            FilteredTables = new DataView(dt);

            ApplyTableFilter();

            // Khôi phục lại trạng thái chọn bàn mà không làm mất liên kết giao diện
            if (savedTableId != -1)
            {
                foreach (DataRowView row in FilteredTables)
                {
                    if (Convert.ToInt32(row["TableID"]) == savedTableId)
                    {
                        _selectedTable = row;
                        OnPropertyChanged(nameof(SelectedTable));
                        LoadBill(savedTableId); // Tải lại chi tiết hóa đơn
                        return;
                    }
                }
            }

            ResetBillStateOnly();
        }

        private void ApplyTableFilter()
        {
            if (FilteredTables == null) return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredTables.RowFilter = string.Empty;
            }
            else
            {
                FilteredTables.RowFilter = string.Format("TableName LIKE '%{0}%'", SearchText.Replace("'", "''"));
            }
        }

        private DataTable _allFoodsCache;

        void LoadCategories()
        {
            // Tải danh mục
            Categories = DataProvider.Instance.ExecuteQuery("EXEC USP_GetCategoryList").DefaultView;

            // Tải trước TOÀN BỘ món ăn vào bộ nhớ đệm một lần duy nhất 
            _allFoodsCache = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.Food");
        }
        void OnCategorySelected()
        {
            
            _searchFoodText = string.Empty;
            OnPropertyChanged(nameof(SearchFoodText));

            if (SelectedCategory != null && _allFoodsCache != null)
            {
                int id = Convert.ToInt32(SelectedCategory["CategoryID"]);

                // Lọc offline từ bộ nhớ đệm
                DataView dv = new DataView(_allFoodsCache);
                dv.RowFilter = $"CategoryID = {id}";
                Foods = dv;
            }
            SelectedFood = null;
        }
        private void ApplyFoodFilter()
        {
            if (_allFoodsCache == null)
            {
                _allFoodsCache = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.Food");
            }

            string textSearch = string.IsNullOrWhiteSpace(SearchFoodText) ? "" : SearchFoodText.Trim();

            // Ô tìm kiếm món có chữ -> Lọc offline toàn chuỗi trên RAM
            if (!string.IsNullOrEmpty(textSearch))
            {
                DataView dvAll = new DataView(_allFoodsCache);
                dvAll.RowFilter = string.Format("FoodName LIKE '%{0}%'", textSearch.Replace("'", "''"));
                Foods = dvAll;

                if (Foods.Count == 1)
                {
                    SelectedFood = Foods[0];
                }
            }
            // Ô tìm kiếm trống -> Trả lại hiển thị danh sách món theo Danh mục đang chọn
            else if (SelectedCategory != null)
            {
                DataView dvCat = new DataView(_allFoodsCache);
                int id = Convert.ToInt32(SelectedCategory["CategoryID"]);
                dvCat.RowFilter = $"CategoryID = {id}";
                Foods = dvCat;
                SelectedFood = null;
            }
            // Không có danh mục lẫn từ khóa tìm kiếm
            else
            {
                Foods = null;
                SelectedFood = null;
            }
        }

        void OnTableSelected() => LoadBill(Convert.ToInt32(SelectedTable["TableID"]));

        void LoadBill(int tableId)
        {
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillByTableID @idTable", new object[] { tableId });

            if (dtBill.Rows.Count > 0)
            {
                CurrentBillId = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                BillDetails = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillDetailsByBillID @idBill", new object[] { CurrentBillId }).DefaultView;

                // Tính toán số tiền món gốc
                decimal sum = 0;
                foreach (DataRowView row in BillDetails) sum += Convert.ToDecimal(row["Total"]);
                SubTotal = sum;

                // Áp dụng công thức tính toán lũy tiến bảo toàn tài chính: +5% phí DV, +8% Thuế VAT
                decimal withServiceCharge = SubTotal * 1.05m;
                TotalAmount = Math.Round(withServiceCharge * 1.08m, 0);
            }
            else
            {
                BillDetails = null;
                SubTotal = 0;
                TotalAmount = 0;
                CurrentBillId = 0;
            }
        }
        #endregion

        #region Execution Logic
        private void ExecuteAddFood()
        {
            if (SelectedTable == null || SelectedFood == null) return;

            int tableId = Convert.ToInt32(SelectedTable["TableID"]);
            int foodId = Convert.ToInt32(SelectedFood["FoodID"]);

            string currentUserName = (AccountDAL.LoginAccount != null) ? AccountDAL.LoginAccount["UserName"].ToString() : "Admin";

            DataProvider.Instance.ExecuteNonQuery(
                "EXEC USP_InsertBillInfo @idTable , @idFood , @count , @userName",
                new object[] { tableId, foodId, Quantity, currentUserName }
            );

            LoadTables();
        }

        private void ExecuteUpdateFood()
        {
            if (Quantity <= 0)
            {
                _messageService.ShowError("Lỗi", "Số lượng phải lớn hơn 0. Nếu muốn bỏ món hãy chọn Xóa món.");
                return;
            }

            int billId = Convert.ToInt32(SelectedBillDetail["BillID"]);
            int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);
            DataProvider.Instance.ExecuteNonQuery("EXEC USP_UpdateBillInfoQuantity @quantity , @idBill , @idFood", new object[] { Quantity, billId, foodId });

            LoadTables();
            _messageService.ShowInfo("Thông báo", "Đã cập nhật số lượng.");
        }

        private void SaveCurrentNote()
        {
            if (SelectedBillDetail != null)
            {
                int billId = Convert.ToInt32(SelectedBillDetail["BillID"]);
                int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);

                DataProvider.Instance.ExecuteNonQuery(
                    "UPDATE dbo.BillInfo SET Note = @note WHERE BillID = @billId AND FoodID = @foodId",
                    new object[] { Note, billId, foodId }
                );
            }
        }

        private void ExecuteDeleteFood()
        {
            if (SelectedTable == null || SelectedBillDetail == null) return;

            if (!_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn muốn xóa món này khỏi hóa đơn?")) return;

            try
            {
                int tableId = Convert.ToInt32(SelectedTable["TableID"]);
                int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);
                string currentUser = AccountDAL.LoginAccount["UserName"].ToString();

                string query = "EXEC USP_DeleteBillInfoByTable @idTable , @idFood , @userName";
                DataProvider.Instance.ExecuteNonQuery(query, new object[] { tableId, foodId, currentUser });

                LoadTables();
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", "Không thể xóa món: " + ex.Message);
            }
        }

        private void ExecutePay()
        {
            if (SelectedTable == null || CurrentBillId == 0) return;

            try
            {
                int tableId = Convert.ToInt32(SelectedTable["TableID"]);
                string tableName = SelectedTable["TableName"].ToString();
                string currentUserName = (AccountDAL.LoginAccount != null) ? AccountDAL.LoginAccount["UserName"].ToString() : "Admin";

                DataProvider.Instance.ExecuteNonQuery(
                    "EXEC USP_UpdateBillInfoByTable @idBill , @idTable , @totalPrice , @userName",
                    new object[] { CurrentBillId, tableId, (double)TotalAmount, currentUserName }
                );

                _messageService.ShowInfo("Thành công", "Đã thanh toán cho " + tableName);

                ResetAllStateAfterPay();
                LoadTables();
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", "Thanh toán thất bại: " + ex.Message);
            }
        }

        private void ResetBillStateOnly()
        {
            _selectedTable = null;
            OnPropertyChanged(nameof(SelectedTable));
            CurrentBillId = 0;
            BillDetails = null;
            SubTotal = 0;
            TotalAmount = 0;
        }

        private void ResetAllStateAfterPay()
        {
            ResetBillStateOnly();
            SearchText = string.Empty;
            SelectedCategory = null;
            Foods = null;
            SelectedFood = null;
        }

        private void ExecutePrintBill()
        {
            try
            {
                string cashierName = AccountDAL.LoginAccount != null ? AccountDAL.LoginAccount["UserName"].ToString() : "Chưa xác định";

                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.DocumentPaginator.PageSize = new Size(400, 920);

                FixedPage page = new FixedPage() { Width = 400, Height = 920, Background = Brushes.White };
                StackPanel container = new StackPanel() { Width = 360, Margin = new Thickness(20) };

                // Header
                container.Children.Add(new TextBlock { Text = "NHÀ HÀNG VIỆT", FontSize = 22, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });
                container.Children.Add(new TextBlock { Text = "HÓA ĐƠN THANH TOÁN", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 5, 0, 10) });
                container.Children.Add(new TextBlock { Text = $"Thu ngân: {cashierName}", FontSize = 12, FontStyle = FontStyles.Italic });
                container.Children.Add(new TextBlock { Text = $"Bàn: {SelectedTable["TableName"]}", FontSize = 12 });
                container.Children.Add(new TextBlock { Text = $"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}", FontSize = 12 });
                container.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });

                // Bill Items
                foreach (DataRowView item in BillDetails)
                {
                    Grid row = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                    row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    row.Children.Add(new TextBlock { Text = item["FoodName"].ToString(), TextWrapping = TextWrapping.Wrap });
                    var txtPrice = new TextBlock { Text = $"{item["Total"]:N0}", HorizontalAlignment = HorizontalAlignment.Right };
                    Grid.SetColumn(txtPrice, 1);
                    row.Children.Add(txtPrice);

                    container.Children.Add(row);
                }

                container.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });

                // Tính toán thuế phí rõ ràng để in hóa đơn
                decimal serviceCharge = Math.Round(SubTotal * 0.05m, 0);
                decimal vatAmount = Math.Round((SubTotal + serviceCharge) * 0.08m, 0);

                Grid rowSubTotal = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                rowSubTotal.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                rowSubTotal.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                rowSubTotal.Children.Add(new TextBlock { Text = "Cộng tiền món:", Foreground = Brushes.DimGray });
                var txtSub = new TextBlock { Text = $"{SubTotal:N0}", HorizontalAlignment = HorizontalAlignment.Right, Foreground = Brushes.DimGray };
                Grid.SetColumn(txtSub, 1); rowSubTotal.Children.Add(txtSub);
                container.Children.Add(rowSubTotal);

                Grid rowService = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                rowService.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                rowService.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                rowService.Children.Add(new TextBlock { Text = "Phí dịch vụ (5%):", Foreground = Brushes.DimGray });
                var txtSvc = new TextBlock { Text = $"{serviceCharge:N0}", HorizontalAlignment = HorizontalAlignment.Right, Foreground = Brushes.DimGray };
                Grid.SetColumn(txtSvc, 1); rowService.Children.Add(txtSvc);
                container.Children.Add(rowService);

                Grid rowVAT = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                rowVAT.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                rowVAT.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                rowVAT.Children.Add(new TextBlock { Text = "Thuế VAT (8%):", Foreground = Brushes.DimGray });
                var txtVat = new TextBlock { Text = $"{vatAmount:N0}", HorizontalAlignment = HorizontalAlignment.Right, Foreground = Brushes.DimGray };
                Grid.SetColumn(txtVat, 1); rowVAT.Children.Add(txtVat);
                container.Children.Add(rowVAT);

                container.Children.Add(new Separator { Margin = new Thickness(0, 5, 0, 10) });

                container.Children.Add(new TextBlock { Text = $"TỔNG CỘNG: {TotalAmount:N0} VNĐ", FontSize = 18, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right });
                container.Children.Add(new TextBlock { Text = "Pass WiFi: NhaHangViet123", Margin = new Thickness(0, 15, 0, 5), HorizontalAlignment = HorizontalAlignment.Center, FontStyle = FontStyles.Italic });

                // QR Code mã hóa chuỗi theo tổng giá trị cuối cùng
                QRCodeGenerator qrGen = new QRCodeGenerator();
                QRCodeData qrData = qrGen.CreateQrCode($"Thanh toan {SelectedTable["TableName"]} - {TotalAmount} VND", QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                using (MemoryStream ms = new MemoryStream(qrBytes))
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    container.Children.Add(new Image { Source = bi, Width = 150, Height = 150, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(10) });
                }

                container.Children.Add(new TextBlock { Text = "QUÉT MÃ ĐỂ THANH TOÁN (MOMO/VNPAY)", FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gray });
                container.Children.Add(new TextBlock { Text = "Hân hạnh được phục vụ quý khách!", Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Medium });

                page.Children.Add(container);
                PageContent content = new PageContent();
                ((System.Windows.Markup.IAddChild)content).AddChild(page);
                fixedDoc.Pages.Add(content);

                BillPreviewWindow preview = new BillPreviewWindow(fixedDoc);
                if (preview.ShowDialog() == true)
                {
                    try
                    {
                        // KHÔNG CẦN KHAI BÁO LẠI 'cashierName' nữa, dùng luôn cái đã có ở đầu hàm

                        string queryLog = @"INSERT INTO ShadowLog (BillID, TableName, ActionType, OldTotal, NewTotal, StaffName, LogTime) 
                            VALUES (@BillID, @TableName, @ActionType, @OldTotal, @NewTotal, @StaffName, GETDATE())";

                        DataProvider.Instance.ExecuteNonQuery(queryLog, new object[] {
                                CurrentBillId,
                                SelectedTable["TableName"],
                                "PRINT_BILL",
                                TotalAmount,                 
                                0,                         
                                cashierName
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi ghi log in bill: " + ex.Message);
                    }

                    OnPrintSuccess?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi", "Không thể xuất hóa đơn: " + ex.Message);
            }
        }
        #endregion
    }
}