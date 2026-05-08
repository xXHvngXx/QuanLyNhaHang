using System;
using System.Data;
using System.Linq;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.ViewModels
{
    public class FoodViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        #region Properties
        private DataView _foodList;
        public DataView FoodList { get => _foodList; set => SetProperty(ref _foodList, value); }

        private DataView _categoryList;
        public DataView CategoryList { get => _categoryList; set => SetProperty(ref _categoryList, value); }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood
        {
            get => _selectedFood;
            set
            {
                if (SetProperty(ref _selectedFood, value) && value != null)
                {
                    FoodID = _selectedFood["FoodID"].ToString();
                    FoodName = _selectedFood["FoodName"].ToString();
                    SelectedCategoryID = _selectedFood["CategoryID"];
                    Price = _selectedFood["Price"].ToString();
                }
            }
        }

        private string _foodID;
        public string FoodID { get => _foodID; set => SetProperty(ref _foodID, value); }

        private string _foodName;
        public string FoodName { get => _foodName; set => SetProperty(ref _foodName, value); }

        private object _selectedCategoryID;
        public object SelectedCategoryID { get => _selectedCategoryID; set => SetProperty(ref _selectedCategoryID, value); }

        private string _price;
        public string Price { get => _price; set => SetProperty(ref _price, value); }
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        #endregion

        public FoodViewModel(IMessageService messageService)
        {
            _messageService = messageService;
            InitCommands();
            RefreshData();
        }

        private void InitCommands()
        {
            LoadDataCommand = new RelayCommand<object>(p => RefreshData());

            AddCommand = new RelayCommand<object>(
                p => ExecuteAdd(),
                p => !string.IsNullOrWhiteSpace(FoodName) && SelectedCategoryID != null && !string.IsNullOrWhiteSpace(Price)
            );

            EditCommand = new RelayCommand<object>(
                p => ExecuteEdit(),
                p => SelectedFood != null
            );

            DeleteCommand = new RelayCommand<object>(
                p => ExecuteDelete(),
                p => SelectedFood != null
            );

            ClearCommand = new RelayCommand<object>(p => RefreshData());
        }

        #region Execution Logic
        private void RefreshData()
        {
            try
            {
                // Load ComboBox Danh mục
                CategoryList = CategoryBLL.Instance.GetCategories().DefaultView;

                // Load DataGrid Món ăn
                DataTable dt = FoodBLL.Instance.GetFoods();
                if (dt != null)
                {
                    DataView dv = dt.DefaultView;
                    dv.Sort = "FoodID ASC";
                    FoodList = dv;

                    int nextId = dt.Rows.Count > 0
                        ? dt.AsEnumerable().Max(r => Convert.ToInt32(r["FoodID"])) + 1
                        : 1;
                    FoodID = nextId.ToString();
                }

                // Reset form
                FoodName = string.Empty;
                Price = "0"; 
                SelectedCategoryID = null;
                SelectedFood = null;
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi tải dữ liệu", ex.Message);
            }
        }

        private void ExecuteAdd()
        {
            if (!decimal.TryParse(Price, out decimal p) || p < 0)
            {
                _messageService.ShowError("Lỗi", "Giá tiền không hợp lệ!");
                return;
            }

            string msg = FoodBLL.Instance.AddFood(FoodName, SelectedCategoryID, Price);
            _messageService.ShowInfo("Thông báo", msg);
            RefreshData();
        }

        private void ExecuteEdit()
        {
            string msg = FoodBLL.Instance.UpdateFood(FoodID, FoodName, SelectedCategoryID, Price);
            _messageService.ShowInfo("Thông báo", msg);
            RefreshData();
        }

        private void ExecuteDelete()
        {
            if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc muốn xóa món ăn này?"))
            {
                string msg = FoodBLL.Instance.DeleteFood(FoodID);
                _messageService.ShowInfo("Thông báo", msg);
                RefreshData();
            }
        }
        #endregion
    }
}