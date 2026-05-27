using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class WaiterViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        private ObservableCollection<DataRowView> _allTablesBackup;

        private string _searchTableText;
        public string SearchTableText
        {
            get => _searchTableText;
            set
            {
                if (SetProperty(ref _searchTableText, value))
                {
                    ApplyTableFilter(); 
                }
            }
        }

        private bool _isAutomatedSelection = false;

        #region Properties
        private ObservableCollection<DataRowView> _tables;
        public ObservableCollection<DataRowView> Tables { get => _tables; set => SetProperty(ref _tables, value); }

        private ObservableCollection<DataRowView> _categories;
        public ObservableCollection<DataRowView> Categories { get => _categories; set => SetProperty(ref _categories, value); }

        private ObservableCollection<DataRowView> _foods;
        public ObservableCollection<DataRowView> Foods { get => _foods; set => SetProperty(ref _foods, value); }

        private ObservableCollection<DataRowView> _billDetails;
        public ObservableCollection<DataRowView> BillDetails { get => _billDetails; set => SetProperty(ref _billDetails, value); }

        private ObservableCollection<DataRowView> _filteredFoods;
        public ObservableCollection<DataRowView> FilteredFoods { get => _filteredFoods; set => SetProperty(ref _filteredFoods, value); }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    if (!_isAutomatedSelection)
                    {
                        ApplyFoodFilter();
                    }
                }
            }
        }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood { get => _selectedFood; set => SetProperty(ref _selectedFood, value); }

        private DataRowView _selectedTable;

        private decimal _totalAmount;
        public decimal TotalAmount { get => _totalAmount; set => SetProperty(ref _totalAmount, value); }

        public DataRowView SelectedTable
        {
            get => _selectedTable;
            set { if (SetProperty(ref _selectedTable, value)) LoadBill(); }
        }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set { if (SetProperty(ref _selectedCategory, value)) LoadFoodByCategory(); }
        }

        private int? _selectedCategoryId;
        public int? SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (SetProperty(ref _selectedCategoryId, value))
                {
                    if (value != null && Categories != null)
                    {
                        SelectedCategory = Categories.FirstOrDefault(c => Convert.ToInt32(c["CategoryID"]) == value);
                    }
                    else
                    {
                        SelectedCategory = null;
                    }
                }
            }
        }

        private string _quantity = "1";
        public string Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private DataRowView _selectedBillDetail;
        public DataRowView SelectedBillDetail
        {
            get => _selectedBillDetail;
            set
            {
                if (SetProperty(ref _selectedBillDetail, value))
                {
                    ExecuteSelectBillDetail();
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddFoodCommand { get; set; }
        public ICommand UpdateFoodCommand { get; set; }
        public ICommand DeleteFoodCommand { get; set; }
        public ICommand LoadDataCommand { get; set; }
        public ICommand ConfirmOrderCommand { get; set; }
        #endregion

        public WaiterViewModel(IMessageService messageService)
        {
            _messageService = messageService;

            LoadDataCommand = new RelayCommand<object>(p => InitData());

            AddFoodCommand = new RelayCommand<object>(
                p => ExecuteAddFood(),
                p => SelectedTable != null && SelectedFood != null
            );

            UpdateFoodCommand = new RelayCommand<DataRowView>(
                p => ExecuteUpdateFood(p),
                p => p != null && !string.IsNullOrEmpty(Quantity)
            );

            DeleteFoodCommand = new RelayCommand<DataRowView>(
                p => ExecuteDeleteFood(p),
                p => p != null
            );
            ConfirmOrderCommand = new RelayCommand<object>(
                p => ExecuteConfirmOrder(),
                p => SelectedTable != null && BillDetails != null && BillDetails.Count > 0
            );
            InitData();
        }

        #region Logic Methods
        public void InitData()
        {
            try
            {
                // Reset các ô nhập liệu và tìm kiếm về rỗng/mặc định
                _searchTableText = string.Empty;
                OnPropertyChanged(nameof(SearchTableText));

                _searchText = string.Empty;
                OnPropertyChanged(nameof(SearchText));

                Quantity = "1"; // Trả số lượng order về 1
                OnPropertyChanged(nameof(Quantity));

                // Reset các vùng chọn món ăn (Combobox danh mục và món ăn)
                SelectedCategoryId = null;
                SelectedCategory = null;
                SelectedFood = null;
                FilteredFoods = new ObservableCollection<DataRowView>(); // Xóa sạch danh sách món ăn đang lọc

                // Tải lại dữ liệu từ cơ sở dữ liệu
                int savedTableId = SelectedTable != null ? Convert.ToInt32(SelectedTable["TableID"]) : -1;

                DataTable dtTable = DataProvider.Instance.ExecuteQuery("EXEC USP_GetTableList");

                // Lưu vào bộ nhớ tạm để không bị mất danh sách gốc khi tìm kiếm
                _allTablesBackup = ConvertDataTableToCollection(dtTable);

                // Chạy hàm lọc để đổ dữ liệu bàn ra màn hình
                ApplyTableFilter();

                DataTable dtCategory = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.Category");
                Categories = ConvertDataTableToCollection(dtCategory);

                if (savedTableId != -1 && _allTablesBackup != null)
                {
                    // Giữ lại trạng thái bàn đang chọn trước khi bấm refresh
                    SelectedTable = _allTablesBackup.FirstOrDefault(t => Convert.ToInt32(t["TableID"]) == savedTableId);
                }

                // Báo giao diện cập nhật lại toàn bộ thông tin
                OnPropertyChanged(nameof(Tables));
                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(FilteredFoods));
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", ex.Message);
            }
        }

        private void ApplyTableFilter()
        {
            if (_allTablesBackup == null) return;

            // Chuyển chữ tìm kiếm về dạng chữ thường để không phân biệt Hoa/Thường
            string textSearch = string.IsNullOrWhiteSpace(SearchTableText) ? "" : SearchTableText.ToLower().Trim();

            if (string.IsNullOrEmpty(textSearch))
            {
                // Nếu ô tìm kiếm trống, hiển thị lại toàn bộ bàn
                Tables = new ObservableCollection<DataRowView>(_allTablesBackup);
            }
            else
            {
                // Lọc các bàn có tên chứa ký tự nhập vào
                var filtered = _allTablesBackup.Where(t =>
                    t["TableName"] != null &&
                    t["TableName"].ToString().ToLower().Contains(textSearch)
                ).ToList();

                Tables = new ObservableCollection<DataRowView>(filtered);
            }

            OnPropertyChanged(nameof(Tables));
        }

        void LoadFoodByCategory()
        {
            if (SelectedCategory == null) return;
            int id = Convert.ToInt32(SelectedCategory["CategoryID"]);
            DataTable dt = DataProvider.Instance.ExecuteQuery("EXEC USP_GetFoodByCategoryID @idCategory", new object[] { id });
            Foods = ConvertDataTableToCollection(dt);

            FilteredFoods = new ObservableCollection<DataRowView>(Foods);

            if (!_isAutomatedSelection)
            {
                _searchText = string.Empty;
                OnPropertyChanged(nameof(SearchText));
                SelectedFood = null;
            }
        }

        private void ApplyFoodFilter()
        {
            string textSearch = string.IsNullOrWhiteSpace(SearchText) ? "" : SearchText.ToLower().Trim();

            if (SelectedCategory == null)
            {
                if (string.IsNullOrEmpty(textSearch))
                {
                    FilteredFoods = new ObservableCollection<DataRowView>();
                    return;
                }
                else
                {
                    DataTable dtAllFoods = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.Food");
                    Foods = ConvertDataTableToCollection(dtAllFoods);
                }
            }

            if (Foods == null) return;

            var result = Foods.Where(f => {
                bool matchesText = string.IsNullOrEmpty(textSearch) ||
                                   f["FoodName"].ToString().ToLower().Contains(textSearch);
                return matchesText;
            }).ToList();

            FilteredFoods = new ObservableCollection<DataRowView>(result);

            if (FilteredFoods.Count == 1 && !string.IsNullOrEmpty(textSearch))
            {
                SelectedFood = FilteredFoods[0];
            }
        }

        private void ExecuteSelectBillDetail()
        {
            if (SelectedBillDetail == null) return;

            try
            {
                // 1. Bật cờ hiệu kiểm soát tự động
                _isAutomatedSelection = true;

                int foodId = Convert.ToInt32(SelectedBillDetail["FoodID"]);
                Quantity = SelectedBillDetail["Quantity"].ToString();

                // Gán gián tiếp vào ô tìm kiếm hiển thị
                _searchText = SelectedBillDetail["FoodName"].ToString();
                OnPropertyChanged(nameof(SearchText));

                DataTable dtFoodInfo = DataProvider.Instance.ExecuteQuery($"SELECT CategoryID FROM dbo.Food WHERE FoodID = {foodId}");
                if (dtFoodInfo != null && dtFoodInfo.Rows.Count > 0)
                {
                    // Sửa tên cột nhận về thành CategoryID
                    int categoryId = Convert.ToInt32(dtFoodInfo.Rows[0]["CategoryID"]);

                    // Kích hoạt đổi danh mục trước
                    SelectedCategoryId = categoryId;

                    // Đẩy việc gán SelectedFood vào Dispatcher để đợi giao diện nạp xong danh sách món
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (FilteredFoods != null && FilteredFoods.Count > 0)
                        {
                            var targetFood = FilteredFoods.FirstOrDefault(f =>
                                f.Row.Table.Columns.Contains("FoodID") && Convert.ToInt32(f["FoodID"]) == foodId);

                            if (targetFood == null)
                            {
                                targetFood = FilteredFoods.FirstOrDefault(f =>
                                    f["FoodName"].ToString().Trim().Equals(_searchText.Trim(), StringComparison.OrdinalIgnoreCase));
                            }

                            // Gán món ăn tìm được cho thuộc tính liên kết ComboBox
                            SelectedFood = targetFood;
                        }

                        // Giải phóng cờ hiệu bên trong luồng xử lý
                        _isAutomatedSelection = false;
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    _isAutomatedSelection = false;
                }

                // Làm mới giao diện số lượng
                OnPropertyChanged(nameof(Quantity));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi bind ngược: " + ex.Message);
                _isAutomatedSelection = false;
            }
        }

        public void LoadBill()
        {
            if (SelectedTable == null)
            {
                BillDetails = null;
                TotalAmount = 0;
                return;
            }

            try
            {
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);
                DataTable dt = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillDetailByTableID @idTable", new object[] { tableID });

                BillDetails = ConvertDataTableToCollection(dt);

                OnPropertyChanged(nameof(BillDetails));

                TotalAmount = BillDetails.Sum(row => Convert.ToDecimal(row["TotalPrice"]));
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi tải hóa đơn", ex.Message);
            }

        }

        void ExecuteAddFood()
        {
            if (!int.TryParse(Quantity, out int qty)) qty = 1;
            try
            {
                if (SelectedFood == null || SelectedTable == null) return;

                int foodID = Convert.ToInt32(SelectedFood["FoodID"]);
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                string currentUser = AccountDAL.LoginAccount["UserName"].ToString();

                DataProvider.Instance.ExecuteNonQuery(
                    "EXEC USP_InsertBillInfo @idTable , @idFood , @count , @userName",
                    new object[] { tableID, foodID, qty, currentUser }
                );

                LoadBill();
                InitData();
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", "Không lấy được thông tin tài khoản hoặc lỗi SQL: " + ex.Message);
            }
        }

        void ExecuteUpdateFood(DataRowView row)
        {
            if (SelectedTable == null || row == null) return;
            if (!int.TryParse(Quantity, out int newQty)) return;

            try
            {
                int foodID = Convert.ToInt32(row["FoodID"]);
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                DataProvider.Instance.ExecuteNonQuery("EXEC USP_UpdateBillInfoQuantityByTable @idTable , @idFood , @quantity", new object[] { tableID, foodID, newQty });

                LoadBill();
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi cập nhật", ex.Message);
            }
        }

        void ExecuteDeleteFood(DataRowView row)
        {
            if (SelectedTable == null || row == null) return;

            if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc muốn xóa món này khỏi hóa đơn?"))
            {
                try
                {
                    int foodID = Convert.ToInt32(row["FoodID"]);
                    int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                    string currentUser = AccountDAL.LoginAccount["UserName"].ToString();

                    string query = "EXEC USP_DeleteBillInfoByTable @idTable , @idFood , @userName";

                    DataProvider.Instance.ExecuteNonQuery(query, new object[] { tableID, foodID, currentUser });

                    LoadBill();
                    InitData();
                }
                catch (Exception ex)
                {
                    _messageService.ShowError("Lỗi xóa món", ex.Message);
                }
            }
        }

        private void ExecuteConfirmOrder()
        {
            if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn gửi các món này đến thu ngân không?"))
            {
                try
                {
                    int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                    DataProvider.Instance.ExecuteNonQuery("EXEC USP_ConfirmBillInfo @idTable", new object[] { tableID });

                    _messageService.ShowInfo("Thông báo", "Đã gửi đơn hàng cho thu ngân!");

                    LoadBill();
                }
                catch (Exception ex)
                {
                    _messageService.ShowError("Lỗi hệ thống", ex.Message);
                }
            }
        }

        private ObservableCollection<DataRowView> ConvertDataTableToCollection(DataTable dt)
        {
            if (dt == null) return new ObservableCollection<DataRowView>();
            return new ObservableCollection<DataRowView>(dt.DefaultView.Cast<DataRowView>());
        }
        #endregion
    }
}