using RestaurantManagementSystem.Models;
using System;
using System.Data;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class CashierViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

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
        public DataRowView SelectedTable { get => _selectedTable; set { _selectedTable = value; OnPropertyChanged(); OnTableSelected(); } }

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

        // Mới: Dòng đang được chọn trên DataGrid hóa đơn
        private DataRowView _selectedBillDetail;
        public DataRowView SelectedBillDetail
        {
            get => _selectedBillDetail;
            set
            {
                _selectedBillDetail = value;
                OnPropertyChanged();
                // Khi click vào dòng trên Grid, tự động lấy số lượng món đó hiện lên ô nhập
                if (value != null) Quantity = Convert.ToInt32(value["Quantity"]);
            }
        }

        public int CurrentBillId { get; set; }
        private string _note = "";
        public string Note { get => _note; set { _note = value; OnPropertyChanged(); } }

        private int _quantity = 1;
        public int Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }
        #endregion

        public ICommand LoadDataCommand { get; set; }
        public ICommand AddFoodCommand { get; set; }
        public ICommand UpdateFoodCommand { get; set; } // Mới
        public ICommand DeleteFoodCommand { get; set; } // Mới
        public ICommand PayCommand { get; set; }

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

            // Command cập nhật: Chỉ cho phép bấm khi đã chọn 1 dòng trong hóa đơn
            UpdateFoodCommand = new RelayCommand<object>(
                execute: (p) => ExecuteUpdateFood(),
                canExecute: (p) => SelectedBillDetail != null
            );

            // Command xóa: Chỉ cho phép bấm khi đã chọn 1 dòng trong hóa đơn
            DeleteFoodCommand = new RelayCommand<object>(
                execute: (p) => ExecuteDeleteFood(),
                canExecute: (p) => SelectedBillDetail != null
            );

            PayCommand = new RelayCommand<object>(
                execute: (p) => ExecutePay(),
                canExecute: (p) => CurrentBillId != 0
            );
        }

        #region Data Loading
        void LoadTables() => Tables = DataProvider.Instance.ExecuteQuery("SELECT * FROM TableFood").DefaultView;
        void LoadCategories() => Categories = DataProvider.Instance.ExecuteQuery("SELECT * FROM Category").DefaultView;

        void OnCategorySelected()
        {
            if (SelectedCategory == null) return;
            int id = Convert.ToInt32(SelectedCategory["CategoryID"]);
            Foods = DataProvider.Instance.ExecuteQuery("SELECT * FROM Food WHERE CategoryID = @id", new object[] { id }).DefaultView;
            SelectedFood = null;
        }

        void OnTableSelected()
        {
            if (SelectedTable == null) return;
            LoadBill(Convert.ToInt32(SelectedTable["TableID"]));
        }

        void LoadBill(int tableId)
        {
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("SELECT * FROM Bill WHERE TableID = @id AND Status = 0", new object[] { tableId });
            if (dtBill.Rows.Count > 0)
            {
                CurrentBillId = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                // Lấy thêm bi.FoodID và bi.BillID để phục vụ xóa/sửa
                BillDetails = DataProvider.Instance.ExecuteQuery(@"
                    SELECT f.FoodName, bi.Quantity, f.Price, (bi.Quantity * f.Price) AS Total, bi.FoodID, bi.BillID
                    FROM BillInfo bi 
                    JOIN Food f ON bi.FoodID = f.FoodID 
                    WHERE bi.BillID = @id", new object[] { CurrentBillId }).DefaultView;

                decimal sum = 0;
                foreach (DataRowView row in BillDetails) sum += Convert.ToDecimal(row["Total"]);
                TotalAmount = sum;
            }
            else { BillDetails = null; TotalAmount = 0; CurrentBillId = 0; }
        }
        #endregion

        #region Execution Logic
        private void ExecuteAddFood()
        {
            int tableId = Convert.ToInt32(SelectedTable["TableID"]);
            int foodId = Convert.ToInt32(SelectedFood["FoodID"]);

            // 1. Kiểm tra/Tạo Bill (Nếu cần - tùy vào logic DB của bạn)
            // 2. Thêm vào BillInfo (Gợi ý: Dùng Store Procedure hoặc Check EXISTS)
            string query = "EXEC USP_InsertBillInfo @idTable , @idFood , @count";
            DataProvider.Instance.ExecuteNonQuery(query, new object[] { tableId, foodId, Quantity });

            LoadBill(tableId);
            LoadTables(); // Refresh màu bàn (đỏ/xanh)
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

            DataProvider.Instance.ExecuteNonQuery("UPDATE BillInfo SET Quantity = @qty WHERE BillID = @bid AND FoodID = @fid",
                new object[] { Quantity, billId, foodId });

            LoadBill(Convert.ToInt32(SelectedTable["TableID"]));
            _messageService.ShowInfo("Thông báo", "Đã cập nhật số lượng.");
        }

        private void ExecuteDeleteFood()
        {
            // 1. Kiểm tra an toàn: Nếu chưa chọn bàn hoặc chưa chọn món để xóa thì thoát
            if (SelectedTable == null || SelectedBillDetail == null) return;

            if (!_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn muốn xóa món này khỏi hóa đơn?")) return;

            // 2. LƯU LẠI ID BÀN VÀO BIẾN TẠM (Quan trọng nhất)
            int tableId = Convert.ToInt32(SelectedTable["TableID"]);

            int billId = Convert.ToInt32(SelectedBillDetail["BillID"]);
            int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);

            // 3. Thực hiện xóa trong Database
            DataProvider.Instance.ExecuteNonQuery("DELETE FROM BillInfo WHERE BillID = @bid AND FoodID = @fid",
                new object[] { billId, foodId });

            
            LoadBill(tableId);

            
            LoadTables();
        }

        private void ExecutePay()
        {
            // 1. Kiểm tra an toàn 
            if (SelectedTable == null || CurrentBillId == 0) return;

            if (!_messageService.ShowConfirm("Thanh toán", "Bạn có muốn thanh toán cho " + SelectedTable["TableName"] + "?")) return;

            // Lưu lại ID bàn đang chọn trước khi refresh danh sách
            int tableId = Convert.ToInt32(SelectedTable["TableID"]);

            // 2. Thực hiện cập nhật DB
            DataProvider.Instance.ExecuteNonQuery("UPDATE Bill SET Status = 1, DateCheckOut = GETDATE() WHERE BillID = @id", new object[] { CurrentBillId });
            DataProvider.Instance.ExecuteNonQuery("UPDATE TableFood SET Status = 0 WHERE TableID = @id", new object[] { tableId });

            _messageService.ShowInfo("Thành công", "Thanh toán hoàn tất!");

            // 3. Reset các thông tin liên quan đến hóa đơn vừa thanh toán
            CurrentBillId = 0;
            BillDetails = null;
            TotalAmount = 0;

            // 4. Refresh lại danh sách bàn
            LoadTables();

        }
        #endregion
    }
}