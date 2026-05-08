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
        public DataView CategoryList { get => _categoryList; set => SetProperty(ref _categoryList, value); }

        private DataRowView _selectedCategory;
        public DataRowView SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    CategoryID = _selectedCategory["CategoryID"].ToString();
                    CategoryName = _selectedCategory["CategoryName"].ToString();
                }
            }
        }

        private string _categoryID;
        public string CategoryID { get => _categoryID; set => SetProperty(ref _categoryID, value); }

        private string _categoryName;
        public string CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }
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
            InitCommands();
            RefreshData();
        }

        private void InitCommands()
        {
            LoadDataCommand = new RelayCommand<object>(p => RefreshData());

            AddCommand = new RelayCommand<object>(
                p => ExecuteAdd(),
                p => !string.IsNullOrEmpty(CategoryName)
            );

            EditCommand = new RelayCommand<object>(
                p => ExecuteEdit(),
                p => SelectedCategory != null
            );

            DeleteCommand = new RelayCommand<object>(
                p => ExecuteDelete(),
                p => SelectedCategory != null
            );

            ClearCommand = new RelayCommand<object>(p => RefreshData());
        }

        #region Execution Logic
        private void RefreshData()
        {
            DataTable dt = CategoryBLL.Instance.GetCategories();
            if (dt != null)
            {
                DataView dv = dt.DefaultView;
                dv.Sort = "CategoryID ASC";
                CategoryList = dv;

                // Tối ưu tính ID tự tăng bằng LINQ
                int nextId = dt.Rows.Count > 0
                    ? dt.AsEnumerable().Max(r => Convert.ToInt32(r["CategoryID"])) + 1
                    : 1;

                CategoryID = nextId.ToString();
            }

            CategoryName = string.Empty;
            SelectedCategory = null; // Trả về trạng thái chuẩn để ô nhập liệu hiện ID mới
        }

        private void ExecuteAdd()
        {
            string message = CategoryBLL.Instance.AddCategory(CategoryName);
            _messageService.ShowInfo("Thông báo", message);
            RefreshData();
        }

        private void ExecuteEdit()
        {
            string message = CategoryBLL.Instance.UpdateCategory(CategoryID, CategoryName);
            _messageService.ShowInfo("Thông báo", message);
            RefreshData();
        }

        private void ExecuteDelete()
        {
            if (_messageService.ShowConfirm("Xác nhận", "Bạn có chắc chắn muốn xóa danh mục này?"))
            {
                string message = CategoryBLL.Instance.DeleteCategory(CategoryID);
                _messageService.ShowInfo("Thông báo", message);
                RefreshData();
            }
        }
        #endregion
    }
}