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

        #region Properties
        private ObservableCollection<DataRowView> _tables;
        public ObservableCollection<DataRowView> Tables { get => _tables; set => SetProperty(ref _tables, value); }

        private ObservableCollection<DataRowView> _categories;
        public ObservableCollection<DataRowView> Categories { get => _categories; set => SetProperty(ref _categories, value); }

        private ObservableCollection<DataRowView> _foods;
        public ObservableCollection<DataRowView> Foods { get => _foods; set => SetProperty(ref _foods, value); }

        private ObservableCollection<DataRowView> _billDetails;
        public ObservableCollection<DataRowView> BillDetails { get => _billDetails; set => SetProperty(ref _billDetails, value); }

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

        private string _quantity = "1";
        public string Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
        #endregion

        #region Commands
        public ICommand AddFoodCommand { get; set; }
        public ICommand UpdateFoodCommand { get; set; }
        public ICommand DeleteFoodCommand { get; set; }
        public ICommand LoadDataCommand { get; set; }
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

            InitData();
        }

        #region Logic Methods
        public void InitData()
        {
            try
            {
                // Lưu lại ID bàn đang chọn để sau khi reload không bị mất focus
                int savedTableId = SelectedTable != null ? Convert.ToInt32(SelectedTable["TableID"]) : -1;

                // Load danh sách bàn
                DataTable dtTable = DataProvider.Instance.ExecuteQuery("EXEC USP_GetTableList");
                Tables = ConvertDataTableToCollection(dtTable);

                // Load danh mục món ăn (Cái này quan trọng để ComboBox có dữ liệu ngay)
                DataTable dtCategory = DataProvider.Instance.ExecuteQuery("SELECT * FROM dbo.Category");
                Categories = ConvertDataTableToCollection(dtCategory);

                // Khôi phục lại bàn đang chọn
                if (savedTableId != -1)
                {
                    SelectedTable = Tables.FirstOrDefault(t => Convert.ToInt32(t["TableID"]) == savedTableId);
                }

                // Cập nhật UI
                OnPropertyChanged(nameof(Tables));
                OnPropertyChanged(nameof(Categories));
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", ex.Message);
            }
        }

        void LoadFoodByCategory()
        {
            if (SelectedCategory == null) return;
            int id = Convert.ToInt32(SelectedCategory["CategoryID"]);
            DataTable dt = DataProvider.Instance.ExecuteQuery("EXEC USP_GetFoodByCategoryID @idCategory", new object[] { id });
            Foods = ConvertDataTableToCollection(dt);
            SelectedFood = null;
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
                InitData(); // Cập nhật lại màu sắc bàn
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

        private ObservableCollection<DataRowView> ConvertDataTableToCollection(DataTable dt)
        {
            if (dt == null) return new ObservableCollection<DataRowView>();
            return new ObservableCollection<DataRowView>(dt.DefaultView.Cast<DataRowView>());
        }
        #endregion
    }
}