using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.Models; 

namespace RestaurantManagementSystem.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService = new MessageService();

        #region Properties
        private DataView _accountList;
        public DataView AccountList
        {
            get => _accountList;
            set => SetProperty(ref _accountList, value);
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private int _accountType;
        public int AccountType
        {
            get => _accountType;
            set => SetProperty(ref _accountType, value);
        }

        private bool _isUserNameReadOnly;
        public bool IsUserNameReadOnly
        {
            get => _isUserNameReadOnly;
            set => SetProperty(ref _isUserNameReadOnly, value);
        }

        private DataRowView _selectedItem;
        public DataRowView SelectedItem
        {
            get => _selectedItem;
            set 
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (value != null)
                    {
                        UserName = value["UserName"]?.ToString();
                        DisplayName = value["DisplayName"]?.ToString();

                        AccountType = Convert.ToInt32(value["Role"]);

                        IsUserNameReadOnly = true; // Khóa TextBox UserName không cho sửa
                    }
                    else
                    {
                        UserName = "";
                        DisplayName = "";
                        AccountType = -1; // Reset về mặc định
                        IsUserNameReadOnly = false; 
                    }
                }
            }
        }
        #endregion

        #region Commands
        public ICommand LoadCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        #endregion

        public AccountViewModel()
        {
            LoadCommand = new RelayCommand<object>((p) => RefreshData());
            ClearCommand = new RelayCommand<object>((p) => RefreshData());

            AddCommand = new RelayCommand<object>(
                (p) => {
                    if (string.IsNullOrEmpty(UserName))
                    {
                        _messageService.ShowError("Lỗi", "Vui lòng nhập tên tài khoản!");
                        return;
                    }
                    string msg = AccountBLL.Instance.AddAccount(UserName, DisplayName, AccountType);
                    _messageService.ShowInfo("Thông báo", msg);
                    RefreshData();
                },
                (p) => !IsUserNameReadOnly
            );

            EditCommand = new RelayCommand<object>(
                (p) => {
                    string msg = AccountBLL.Instance.UpdateAccount(UserName, DisplayName, AccountType);
                    _messageService.ShowInfo("Thông báo", msg);
                    RefreshData();
                },
                (p) => SelectedItem != null
            );

            DeleteCommand = new RelayCommand<object>(
                (p) => {
                            // Lấy Role của tài khoản đang được chọn từ ComboBox/Property
                    int selectedRole = AccountType;

                            // Chặn xóa bất kỳ tài khoản nào thuộc nhóm Quản trị (Role = 0)
                    if (selectedRole == 0)
                    {
                        _messageService.ShowError("Lỗi bảo mật", "Không được phép xóa tài khoản thuộc nhóm Quản trị viên!");
                        return;
                    }

                    // Thêm kiểm tra nếu Admin tự xóa chính mình
                    string currentLoggedUser = RestaurantManagementSystem.DAL.AccountDAL.LoginAccount["UserName"].ToString();
                    if (UserName.ToLower() == currentLoggedUser.ToLower())
                    {
                        _messageService.ShowError("Lỗi", "Bạn không thể tự xóa tài khoản của chính mình khi đang đăng nhập!");
                        return;
                    }

                    // Xác nhận xóa các role khác (1, 2, -1)
                    if (_messageService.ShowConfirm("Xác nhận", $"Bạn có chắc chắn muốn xóa tài khoản '{UserName}'?"))
                    {
                        string msg = AccountBLL.Instance.DeleteAccount(UserName);
                        _messageService.ShowInfo("Thông báo", msg);
                        RefreshData();
                    }
                },
                (p) => SelectedItem != null
            );

            ResetPasswordCommand = new RelayCommand<object>(
                (p) => {
                    string confirmMsg = $"Đặt lại mật khẩu cho '{UserName}'? \n(Mật khẩu mới sẽ trùng với tên tài khoản)";

                    if (_messageService.ShowConfirm("Xác nhận Reset", confirmMsg))
                    {
                        string msg = AccountBLL.Instance.ResetPassword(UserName);
                        _messageService.ShowInfo("Thông báo", msg);
                    }
                },
                (p) => SelectedItem != null
            );

            RefreshData();
        }

        private void RefreshData()
        {
            var dt = AccountBLL.Instance.GetAccounts();
            if (dt != null)
                AccountList = dt.DefaultView;

            UserName = "";
            DisplayName = "";
            AccountType = -1;
            IsUserNameReadOnly = false;
            SelectedItem = null;
        }
    }
}