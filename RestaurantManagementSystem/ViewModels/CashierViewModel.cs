using QRCoder;
using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.Views;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace RestaurantManagementSystem.ViewModels
{
    public class CashierViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        public Action OnPrintSuccess { get; set; }

        #region Properties
        private DataView _tables;
        public DataView Tables { get => _tables; set { _tables = value; OnPropertyChanged(); } }

        private DataView _categories;
        public DataView Categories { get => _categories; set { _categories = value; OnPropertyChanged(); } }

        private DataView _foods;
        public DataView Foods { get => _foods; set { _foods = value; OnPropertyChanged(); } }

        private DataView _billDetails;
        public DataView BillDetails { get => _billDetails; set { _billDetails = value; OnPropertyChanged(); } }

        private decimal _totalAmount;
        public decimal TotalAmount { get => _totalAmount; set { _totalAmount = value; OnPropertyChanged(); } }

        private DataRowView _selectedTable;
        public DataRowView SelectedTable
        {
            get => _selectedTable;
            set
            {
                _selectedTable = value;
                OnPropertyChanged();
                OnTableSelected();
            }
        }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnCategorySelected();
            }
        }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood { get => _selectedFood; set { _selectedFood = value; OnPropertyChanged(); } }

        private DataRowView _selectedBillDetail;
        public DataRowView SelectedBillDetail
        {
            get => _selectedBillDetail;
            set
            {
                _selectedBillDetail = value;
                OnPropertyChanged();
                if (value != null) Quantity = Convert.ToInt32(value["Quantity"]);
            }
        }

        public int CurrentBillId { get; set; }
        private string _note = "";
        public string Note { get => _note; set { _note = value; OnPropertyChanged(); } }

        private int _quantity = 1;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }
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

            LoadDataCommand = new RelayCommand<object>(
                execute: (p) => { LoadTables(); LoadCategories(); },
                canExecute: (p) => true
            );

            AddFoodCommand = new RelayCommand<object>(
                execute: (p) => ExecuteAddFood(),
                canExecute: (p) => SelectedTable != null && SelectedFood != null
            );

            UpdateFoodCommand = new RelayCommand<object>(
                execute: (p) => ExecuteUpdateFood(),
                canExecute: (p) => SelectedBillDetail != null
            );

            DeleteFoodCommand = new RelayCommand<object>(
                execute: (p) => ExecuteDeleteFood(),
                canExecute: (p) => SelectedBillDetail != null
            );

            PayCommand = new RelayCommand<object>(
                execute: (p) => ExecutePay(),
                canExecute: (p) => CurrentBillId != 0
            );

            PrintBillCommand = new RelayCommand<object>(
                execute: (p) => ExecutePrintBill(),
                canExecute: (p) => CurrentBillId != 0 && BillDetails != null
            );
        }

        #region Data Loading
        void LoadTables()
        {
            int savedTableId = -1;
            if (SelectedTable != null)
            {
                savedTableId = Convert.ToInt32(SelectedTable["TableID"]);
            }

            Tables = DataProvider.Instance.ExecuteQuery("EXEC USP_GetTableList").DefaultView;

            if (savedTableId != -1)
            {
                foreach (DataRowView row in Tables)
                {
                    if (Convert.ToInt32(row["TableID"]) == savedTableId)
                    {
                        SelectedTable = row;
                        break;
                    }
                }
            }
        }

        void LoadCategories() => Categories = DataProvider.Instance.ExecuteQuery("EXEC USP_GetCategoryList").DefaultView;

        void OnCategorySelected()
        {
            if (SelectedCategory == null) return;
            int id = Convert.ToInt32(SelectedCategory["CategoryID"]);
            Foods = DataProvider.Instance.ExecuteQuery("EXEC USP_GetFoodByCategoryID @idCategory", new object[] { id }).DefaultView;
            SelectedFood = null;
        }

        void OnTableSelected()
        {
            if (SelectedTable == null) return;
            LoadBill(Convert.ToInt32(SelectedTable["TableID"]));
        }

        void LoadBill(int tableId)
        {
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillByTableID @idTable", new object[] { tableId });

            if (dtBill.Rows.Count > 0)
            {
                CurrentBillId = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                BillDetails = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillDetailsByBillID @idBill", new object[] { CurrentBillId }).DefaultView;

                decimal sum = 0;
                foreach (DataRowView row in BillDetails) sum += Convert.ToDecimal(row["Total"]);
                TotalAmount = sum;
            }
            else
            {
                BillDetails = null;
                TotalAmount = 0;
                CurrentBillId = 0;
            }
        }
        #endregion

        #region Execution Logic
        private void ExecuteAddFood()
        {
            int tableId = Convert.ToInt32(SelectedTable["TableID"]);
            int foodId = Convert.ToInt32(SelectedFood["FoodID"]);

            DataProvider.Instance.ExecuteNonQuery("EXEC USP_InsertBillInfo @idTable , @idFood , @count", new object[] { tableId, foodId, Quantity });

            LoadTables();
        }

        private void ExecuteUpdateFood()
        {
            int billId = Convert.ToInt32(SelectedBillDetail["BillID"]);
            int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);

            if (Quantity <= 0)
            {
                _messageService.ShowError("Lỗi", "Số lượng phải lớn hơn 0. Nếu muốn bỏ món hãy chọn Xóa món.");
                return;
            }

            DataProvider.Instance.ExecuteNonQuery("EXEC USP_UpdateBillInfoQuantity @quantity , @idBill , @idFood",
                new object[] { Quantity, billId, foodId });

            LoadTables();
            _messageService.ShowInfo("Thông báo", "Đã cập nhật số lượng.");
        }

        private void ExecuteDeleteFood()
        {
            if (SelectedTable == null || SelectedBillDetail == null) return;

            if (!_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn muốn xóa món này khỏi hóa đơn?")) return;

            int billId = Convert.ToInt32(SelectedBillDetail["BillID"]);
            int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);

            DataProvider.Instance.ExecuteNonQuery("EXEC USP_DeleteBillInfo @idBill , @idFood", new object[] { billId, foodId });

            LoadTables();
        }

        private void ExecutePay()
        {
            if (SelectedTable == null || CurrentBillId == 0) return;

            try
            {
                int tableId = Convert.ToInt32(SelectedTable["TableID"]);

                string currentUserName = "admin"; 
                if (AccountDAL.LoginAccount != null)
                {
                    currentUserName = AccountDAL.LoginAccount["UserName"].ToString();
                }

                DataProvider.Instance.ExecuteNonQuery("EXEC USP_CheckOut @idBill , @idTable , @userName",
                    new object[] { CurrentBillId, tableId, currentUserName });

                _messageService.ShowInfo("Thành công", "Đã thanh toán cho " + SelectedTable["TableName"]);

                SelectedTable = null;
                CurrentBillId = 0;
                BillDetails = null;
                TotalAmount = 0;
                LoadTables();
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", "Thanh toán thất bại: " + ex.Message);
            }
        }

        private void ExecutePrintBill()
        {
            try
            {
                string cashierName = "Chưa xác định";
                if (AccountDAL.LoginAccount != null)
                {
                    cashierName = AccountDAL.LoginAccount["UserName"].ToString();
                }

                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.DocumentPaginator.PageSize = new Size(400, 850);

                FixedPage page = new FixedPage() { Width = 400, Height = 850, Background = System.Windows.Media.Brushes.White };
                StackPanel container = new StackPanel() { Width = 360, Margin = new Thickness(20) };

                container.Children.Add(new TextBlock { Text = "NHÀ HÀNG VIỆT", FontSize = 22, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });
                container.Children.Add(new TextBlock { Text = "HÓA ĐƠN THANH TOÁN", FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 5, 0, 10) });

                container.Children.Add(new TextBlock { Text = "Thu ngân: " + cashierName, FontSize = 12, FontStyle = FontStyles.Italic });
                container.Children.Add(new TextBlock { Text = "Bàn: " + SelectedTable["TableName"], FontSize = 12 });
                container.Children.Add(new TextBlock { Text = "Ngày: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), FontSize = 12 });
                container.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });

                foreach (DataRowView item in BillDetails)
                {
                    Grid row = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                    row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                    row.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    row.Children.Add(new TextBlock { Text = item["FoodName"].ToString(), TextWrapping = TextWrapping.Wrap });

                    var txtPrice = new TextBlock { Text = string.Format("{0:N0}", item["Total"]), HorizontalAlignment = HorizontalAlignment.Right };
                    Grid.SetColumn(txtPrice, 1);
                    row.Children.Add(txtPrice);

                    container.Children.Add(row);
                }

                container.Children.Add(new Separator { Margin = new Thickness(0, 10, 0, 10) });
                container.Children.Add(new TextBlock { Text = "TỔNG CỘNG: " + string.Format("{0:N0} VNĐ", TotalAmount), FontSize = 18, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right });

                container.Children.Add(new TextBlock { Text = "Pass WiFi: NhaHangViet123", Margin = new Thickness(0, 20, 0, 5), HorizontalAlignment = HorizontalAlignment.Center, FontStyle = FontStyles.Italic });

                QRCodeGenerator qrGen = new QRCodeGenerator();
                QRCodeData qrData = qrGen.CreateQrCode("Thanh toán " + SelectedTable["TableName"] + " - " + TotalAmount + " VNĐ", QRCodeGenerator.ECCLevel.Q);
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

                container.Children.Add(new TextBlock { Text = "QUÉT MÃ ĐỂ THANH TOÁN (MOMO/VNPAY)", FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center, Foreground = System.Windows.Media.Brushes.Gray });
                container.Children.Add(new TextBlock { Text = "Hân hạnh được phục vụ quý khách!", Margin = new Thickness(0, 10, 0, 0), HorizontalAlignment = HorizontalAlignment.Center, FontWeight = FontWeights.Medium });

                page.Children.Add(container);
                PageContent content = new PageContent();
                ((System.Windows.Markup.IAddChild)content).AddChild(page);
                fixedDoc.Pages.Add(content);

                BillPreviewWindow preview = new BillPreviewWindow(fixedDoc);

                if (preview.ShowDialog() == true)
                {
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