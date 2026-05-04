using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;

namespace RestaurantManagementSystem.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
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
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    UserName = value["UserName"].ToString();
                    DisplayName = value["DisplayName"].ToString();
                    AccountType = Convert.ToInt32(value["Role"]);
                    IsUserNameReadOnly = true;
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
                        MessageBox.Show("Vui lòng nhập tên tài khoản!");
                        return;
                    }
                    // Mật khẩu mặc định sẽ được xử lý trong BLL dựa trên UserName
                    string msg = AccountBLL.Instance.AddAccount(UserName, DisplayName, AccountType);
                    MessageBox.Show(msg, "Thông báo");
                    RefreshData();
                },
                (p) => !IsUserNameReadOnly // Chỉ cho phép Add khi đang ở chế độ tạo mới
            );

            EditCommand = new RelayCommand<object>(
                (p) => {
                    string msg = AccountBLL.Instance.UpdateAccount(UserName, DisplayName, AccountType);
                    MessageBox.Show(msg, "Thông báo");
                    RefreshData();
                },
                (p) => SelectedItem != null
            );

            DeleteCommand = new RelayCommand<object>(
                (p) => {
                    if (UserName.ToLower() == "admin")
                    {
                        MessageBox.Show("Không được xóa tài khoản quản trị gốc!");
                        return;
                    }

                    if (MessageBox.Show($"Xóa tài khoản '{UserName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        string msg = AccountBLL.Instance.DeleteAccount(UserName);
                        MessageBox.Show(msg, "Thông báo");
                        RefreshData();
                    }
                },
                (p) => SelectedItem != null
            );

            ResetPasswordCommand = new RelayCommand<object>(
                (p) => {
                     // Thông báo rõ ràng: mật khẩu mới sẽ trùng với UserName
                    string confirmMsg = $"Đặt lại mật khẩu cho '{UserName}'? \n(Mật khẩu mới sẽ trùng với tên tài khoản)";

                    if (MessageBox.Show(confirmMsg, "Xác nhận Reset", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        string msg = AccountBLL.Instance.ResetPassword(UserName);
                        MessageBox.Show(msg, "Thông báo");
                    }
                },
                (p) => SelectedItem != null && AccountType != 0 //Chặn thay đổi mật khẩu cho quyền quản trị viên
            );

            RefreshData();
        }

        private void RefreshData()
        {
            AccountList = AccountBLL.Instance.GetAccounts().DefaultView;
            UserName = "";
            DisplayName = "";
            AccountType = 1; // Mặc định khi Clear là nhân viên Phục vụ (Role 1)
            IsUserNameReadOnly = false;
            SelectedItem = null;
        }
    }
}