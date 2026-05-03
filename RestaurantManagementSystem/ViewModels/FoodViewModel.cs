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
        public DataView FoodList
        {
            get => _foodList;
            set { _foodList = value; OnPropertyChanged(); }
        }

        private DataView _categoryList;
        public DataView CategoryList
        {
            get => _categoryList;
            set { _categoryList = value; OnPropertyChanged(); }
        }

        private DataRowView _selectedFood;
        public DataRowView SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                OnPropertyChanged();
                if (_selectedFood != null)
                {
                    FoodID = _selectedFood["FoodID"].ToString();
                    FoodName = _selectedFood["FoodName"].ToString();
                    SelectedCategoryID = _selectedFood["CategoryID"];
                    Price = _selectedFood["Price"].ToString();
                }
            }
        }

        private string _foodID;
        public string FoodID { get => _foodID; set { _foodID = value; OnPropertyChanged(); } }

        private string _foodName;
        public string FoodName { get => _foodName; set { _foodName = value; OnPropertyChanged(); } }

        private object _selectedCategoryID;
        public object SelectedCategoryID { get => _selectedCategoryID; set { _selectedCategoryID = value; OnPropertyChanged(); } }

        private string _price;
        public string Price { get => _price; set { _price = value; OnPropertyChanged(); } }
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

            LoadDataCommand = new RelayCommand<object>(p => RefreshData(), p => true);

            AddCommand = new RelayCommand<object>(
                p => {
                    string msg = FoodBLL.Instance.AddFood(FoodName, SelectedCategoryID, Price);
                    _messageService.ShowInfo("Thông báo", msg);
                    RefreshData();
                },
                p => !string.IsNullOrEmpty(FoodName) && SelectedCategoryID != null
            );

            EditCommand = new RelayCommand<object>(
                p => {
                    string msg = FoodBLL.Instance.UpdateFood(FoodID, FoodName, SelectedCategoryID, Price);
                    _messageService.ShowInfo("Thông báo", msg);
                    RefreshData();
                },
                p => _selectedFood != null
            );

            DeleteCommand = new RelayCommand<object>(
                p => {
                    if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc muốn xóa món ăn này?"))
                    {
                        string msg = FoodBLL.Instance.DeleteFood(FoodID);
                        _messageService.ShowInfo("Thông báo", msg);
                        RefreshData();
                    }
                },
                p => _selectedFood != null
            );

            ClearCommand = new RelayCommand<object>(p => RefreshData(), p => true);

            RefreshData();
        }

        private void RefreshData()
        {
            // Load ComboBox Danh mục
            CategoryList = CategoryBLL.Instance.GetCategories().DefaultView;

            // Load DataGrid Món ăn
            DataTable dt = FoodBLL.Instance.GetFoods();
            if (dt != null)
            {
                DataView dv = dt.DefaultView;
                dv.Sort = "FoodID ASC"; // Sắp xếp tăng dần
                FoodList = dv;

                // Tự động tính ID tiếp theo
                if (dt.Rows.Count > 0)
                {
                    int maxId = dt.AsEnumerable().Max(r => Convert.ToInt32(r["FoodID"]));
                    FoodID = (maxId + 1).ToString();
                }
                else { FoodID = "1"; }
            }

            // Reset form
            FoodName = string.Empty;
            Price = string.Empty;
            SelectedCategoryID = null;
            SelectedFood = null;
        }
    }
}