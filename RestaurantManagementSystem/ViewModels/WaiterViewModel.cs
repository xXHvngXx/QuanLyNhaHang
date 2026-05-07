using RestaurantManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class WaiterViewModel : BaseViewModel
    {
        #region Properties
        private ObservableCollection<DataRowView> _tables;
        public ObservableCollection<DataRowView> Tables { get => _tables; set { _tables = value; OnPropertyChanged(); } }

        private ObservableCollection<DataRowView> _categories;
        public ObservableCollection<DataRowView> Categories { get => _categories; set { _categories = value; OnPropertyChanged(); } }

        private ObservableCollection<DataRowView> _foods;
        public ObservableCollection<DataRowView> Foods { get => _foods; set { _foods = value; OnPropertyChanged(); } }

        private ObservableCollection<DataRowView> _billDetails;
        public ObservableCollection<DataRowView> BillDetails { get => _billDetails; set { _billDetails = value; OnPropertyChanged(); } }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood { get => _selectedFood; set { _selectedFood = value; OnPropertyChanged(); } }

        private DataRowView _selectedTable;
        public DataRowView SelectedTable
        {
            get => _selectedTable;
            set { _selectedTable = value; OnPropertyChanged(); LoadBill(); }
        }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); LoadFoodByCategory(); }
        }

        private string _quantity = "1";
        public string Quantity { get => _quantity; set { _quantity = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand AddFoodCommand { get; set; }
        public ICommand UpdateFoodCommand { get; set; }
        public ICommand DeleteFoodCommand { get; set; }
        public ICommand LoadDataCommand { get; set; }
        #endregion

        public WaiterViewModel()
        {
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
                int? selectedTableId = null;
                if (SelectedTable != null)
                {
                    selectedTableId = Convert.ToInt32(SelectedTable["TableID"]);
                }

                DataTable dtTable = DataProvider.Instance.ExecuteQuery("EXEC USP_GetTableList");
                Tables = ConvertDataTableToCollection(dtTable);

                DataTable dtCategory = DataProvider.Instance.ExecuteQuery("EXEC USP_GetCategoryList");
                Categories = ConvertDataTableToCollection(dtCategory);

                if (selectedTableId != null)
                {
                    foreach (var table in Tables)
                    {
                        if (Convert.ToInt32(table["TableID"]) == selectedTableId)
                        {
                            SelectedTable = table;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
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
                return;
            }
            int tableID = Convert.ToInt32(SelectedTable["TableID"]);
            DataTable dtBill = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillByTableID @idTable", new object[] { tableID });

            if (dtBill.Rows.Count > 0)
            {
                int billID = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                DataTable dt = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillDetailsByBillID @idBill", new object[] { billID });
                BillDetails = ConvertDataTableToCollection(dt);
            }
            else
            {
                BillDetails = null;
            }
        }

        void ExecuteAddFood()
        {
            if (SelectedTable == null || SelectedFood == null) return;

            int foodID = Convert.ToInt32(SelectedFood["FoodID"]);
            int tableID = Convert.ToInt32(SelectedTable["TableID"]);
            if (!int.TryParse(Quantity, out int quantity)) quantity = 1;

            try
            {
                DataProvider.Instance.ExecuteNonQuery("EXEC USP_InsertBillInfo @idTable , @idFood , @count",
                    new object[] { tableID, foodID, quantity });

                LoadBill();
                InitData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm món: " + ex.Message);
            }
        }

        void ExecuteUpdateFood(DataRowView row)
        {
            if (SelectedTable == null || row == null) return;

            if (!int.TryParse(Quantity, out int newQuantity))
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ!");
                return;
            }

            try
            {
                int foodID = Convert.ToInt32(row["FoodID"]);
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                DataTable dtBill = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillByTableID @idTable", new object[] { tableID });

                if (dtBill.Rows.Count > 0)
                {
                    int billID = Convert.ToInt32(dtBill.Rows[0]["BillID"]);
                    DataProvider.Instance.ExecuteNonQuery("EXEC USP_UpdateBillInfoQuantity @quantity , @idBill , @idFood",
                        new object[] { newQuantity, billID, foodID });

                    LoadBill();
                    MessageBox.Show("Đã cập nhật số lượng!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa: " + ex.Message);
            }
        }

        void ExecuteDeleteFood(DataRowView row)
        {
            if (SelectedTable == null || row == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int foodID = Convert.ToInt32(row["FoodID"]);
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                DataTable dataBill = DataProvider.Instance.ExecuteQuery("EXEC USP_GetBillByTableID @idTable", new object[] { tableID });

                if (dataBill.Rows.Count > 0)
                {
                    int billID = Convert.ToInt32(dataBill.Rows[0]["BillID"]);
                    DataProvider.Instance.ExecuteNonQuery("EXEC USP_DeleteBillInfo @idBill , @idFood", new object[] { billID, foodID });
                }

                LoadBill();
                InitData();
            }
        }

        private ObservableCollection<DataRowView> ConvertDataTableToCollection(DataTable dt)
        {
            var collection = new ObservableCollection<DataRowView>();
            if (dt == null) return collection;
            foreach (DataRow row in dt.Rows)
                collection.Add(dt.DefaultView[dt.Rows.IndexOf(row)]);
            return collection;
        }
        #endregion
    }
}