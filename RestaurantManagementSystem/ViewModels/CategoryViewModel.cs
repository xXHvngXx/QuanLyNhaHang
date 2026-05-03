using System;
using System.Data;
using System.Linq;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        #region Properties
        private DataView _categoryList;
        public DataView CategoryList
        {
            get => _categoryList;
            set { _categoryList = value; OnPropertyChanged(); }
        }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (_selectedCategory != null)
                {
                    CategoryID = _selectedCategory["CategoryID"].ToString();
                    CategoryName = _selectedCategory["CategoryName"].ToString();
                }
            }
        }

        private string _categoryID;
        public string CategoryID
        {
            get => _categoryID;
            set { _categoryID = value; OnPropertyChanged(); }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand LoadDataCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        #endregion

        public CategoryViewModel(IMessageService messageService)
        {
            _messageService = messageService;

            LoadDataCommand = new RelayCommand<object>(p => RefreshData(), p => true);

            AddCommand = new RelayCommand<object>(
                p => {
                    string message = CategoryBLL.Instance.AddCategory(CategoryName);
                    _messageService.ShowInfo("Thông báo", message);
                    RefreshData();
                },
                p => !string.IsNullOrEmpty(CategoryName)
            );

            EditCommand = new RelayCommand<object>(
                p => {
                    string message = CategoryBLL.Instance.UpdateCategory(CategoryID, CategoryName);
                    _messageService.ShowInfo("Thông báo", message);
                    RefreshData();
                },
                p => _selectedCategory != null
            );

            DeleteCommand = new RelayCommand<object>(
                p => {
                    if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn muốn xóa danh mục này?"))
                    {
                        string message = CategoryBLL.Instance.DeleteCategory(CategoryID);
                        _messageService.ShowInfo("Thông báo", message);
                        RefreshData();
                    }
                },
                p => _selectedCategory != null
            );

            ClearCommand = new RelayCommand<object>(p => RefreshData(), p => true);

            RefreshData();
        }

        private void RefreshData()
        {
            DataTable dt = CategoryBLL.Instance.GetCategories();
            if (dt != null)
            {
                //Tạo DataView từ DataTable
                DataView dv = dt.DefaultView;

                
                dv.Sort = "CategoryID ASC";

                // Gán View đã sắp xếp này cho danh sách hiển thị
                CategoryList = dv;

                //Tính ID tự tăng cho ô nhập liệu 
                if (dt.Rows.Count > 0)
                {
                    int maxId = dt.AsEnumerable().Max(r => Convert.ToInt32(r["CategoryID"]));
                    CategoryID = (maxId + 1).ToString();
                }
                else
                {
                    CategoryID = "1";
                }
            }
            CategoryName = string.Empty;
            SelectedCategory = null;
        }
    }
}