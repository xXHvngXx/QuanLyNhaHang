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
        public ICommand DeleteFoodCommand { get; set; }
        public ICommand LoadDataCommand { get; set; }
        #endregion

        public WaiterViewModel()
        {
            LoadDataCommand = new RelayCommand<object>(p => InitData());

            AddFoodCommand = new RelayCommand<object>(
                p => ExecuteAddFood(),
                p => SelectedTable != null && SelectedCategory != null
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
                // 1. Lưu lại ID của bàn đang được chọn trước khi refresh
                int? selectedTableId = null;
                if (SelectedTable != null)
                {
                    selectedTableId = Convert.ToInt32(SelectedTable["TableID"]);
                }

                // 2. Tải lại dữ liệu từ Database
                DataTable dtTable = DataProvider.Instance.ExecuteQuery("SELECT * FROM TableFood");
                Tables = ConvertDataTableToCollection(dtTable);

                DataTable dtCategory = DataProvider.Instance.ExecuteQuery("SELECT * FROM Category");
                Categories = ConvertDataTableToCollection(dtCategory);

                // 3. Tìm lại bàn cũ trong danh sách mới và set lại SelectedTable
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
            DataTable dt = DataProvider.Instance.ExecuteQuery("SELECT * FROM Food WHERE CategoryID = @id", new object[] { id });
            Foods = ConvertDataTableToCollection(dt);
        }

        public void LoadBill()
        {
            if (SelectedTable == null)
            {
                BillDetails = null;
                return;
            }
            int tableID = Convert.ToInt32(SelectedTable["TableID"]);
            string query = @"SELECT f.FoodID, f.FoodName, bi.Quantity, f.Price, (bi.Quantity * f.Price) AS Total 
                             FROM Bill b JOIN BillInfo bi ON b.BillID = bi.BillID 
                             JOIN Food f ON bi.FoodID = f.FoodID 
                             WHERE b.TableID = @id AND b.Status = 0";

            DataTable dt = DataProvider.Instance.ExecuteQuery(query, new object[] { tableID });
            BillDetails = ConvertDataTableToCollection(dt);
        }

        void ExecuteAddFood()
        {
            if (SelectedTable == null || SelectedCategory == null || Foods == null) return;

            var selectedFoodRow = System.Windows.Application.Current.MainWindow.FindName("cbFood") is System.Windows.Controls.ComboBox cbFood ? cbFood.SelectedValue : null;
            if (selectedFoodRow == null && Foods.Count > 0)
                selectedFoodRow = Foods[0]["FoodID"];

            if (selectedFoodRow == null) return;

            int foodID = Convert.ToInt32(selectedFoodRow);
            int quantity = 1;
            int.TryParse(Quantity, out quantity);

            int tableID = Convert.ToInt32(SelectedTable["TableID"]);
            DataTable bill = DataProvider.Instance.ExecuteQuery("SELECT * FROM Bill WHERE TableID = @tableId AND Status = 0", new object[] { tableID });
            int billID;

            if (bill.Rows.Count == 0)
            {
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO Bill (DateCheckIn, TableID, Status, UserName) VALUES (GETDATE(), @tableId, 0, N'admin')", new object[] { tableID });
                DataProvider.Instance.ExecuteNonQuery("UPDATE TableFood SET Status = 1 WHERE TableID = @tableId", new object[] { tableID });

                billID = Convert.ToInt32(DataProvider.Instance.ExecuteQuery("SELECT MAX(BillID) FROM Bill").Rows[0][0]);
            }
            else
            {
                billID = Convert.ToInt32(bill.Rows[0]["BillID"]);
            }

            DataTable exist = DataProvider.Instance.ExecuteQuery("SELECT * FROM BillInfo WHERE BillID = @billId AND FoodID = @foodId", new object[] { billID, foodID });

            if (exist.Rows.Count > 0)
            {
                DataProvider.Instance.ExecuteNonQuery("UPDATE BillInfo SET Quantity += @qty WHERE BillID = @billId AND FoodID = @foodId", new object[] { quantity, billID, foodID });
            }
            else
            {
                DataProvider.Instance.ExecuteNonQuery("INSERT INTO BillInfo (BillID, FoodID, Quantity) VALUES (@billId, @foodId, @qty)", new object[] { billID, foodID, quantity });
            }

            LoadBill();
            InitData();
        }

        void ExecuteDeleteFood(DataRowView row)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int foodID = Convert.ToInt32(row["FoodID"]);
                int tableID = Convert.ToInt32(SelectedTable["TableID"]);

                DataTable dataBill = DataProvider.Instance.ExecuteQuery("SELECT BillID FROM Bill WHERE TableID = @tableID AND Status = 0", new object[] { tableID });

                if (dataBill.Rows.Count > 0)
                {
                    int billID = Convert.ToInt32(dataBill.Rows[0]["BillID"]);
                    DataProvider.Instance.ExecuteNonQuery("DELETE FROM BillInfo WHERE BillID = @billID AND FoodID = @foodID", new object[] { billID, foodID });
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