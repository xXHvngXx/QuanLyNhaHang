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

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
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

        public ICommand BackupCommand { get; set; }
        public ICommand RestoreCommand { get; set; }
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

            BackupCommand = new RelayCommand<object>(
                (p) => ExecuteBackup(),
                (p) => true // Bất kỳ lúc nào Admin cũng có thể bấm backup
            );

            RestoreCommand = new RelayCommand<object>(
                (p) => ExecuteRestore(),
                (p) => true
            );
            RefreshData();
        }

        private void RefreshData()
        {
            SearchText = string.Empty; 

            var dt = AccountBLL.Instance.GetAccounts();
            if (dt != null)
                AccountList = dt.DefaultView;

            UserName = "";
            DisplayName = "";
            AccountType = -99;
            IsUserNameReadOnly = false;
            SelectedItem = null;
        }

        private void ApplyFilter()
        {
            if (AccountList == null) return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                AccountList.RowFilter = string.Empty;
            }
            else
            {
                // Lọc theo UserName hoặc DisplayName
                string filter = SearchText.Replace("'", "''");
                AccountList.RowFilter = $"UserName LIKE '%{filter}%' OR DisplayName LIKE '%{filter}%'";
            }
        }

        private void ExecuteBackup()
        {
            // Mở hộp thoại chọn nơi lưu file .bak
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = "SQL Server Backup (*.bak)|*.bak";
            sfd.FileName = $"QL_NhaHang_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    string query = "EXEC USP_BackupDatabase @path";

                    // Nạp giá trị đường dẫn vào mảng parameter
                    object[] parameters = new object[] { sfd.FileName };

                    // Gọi trực tiếp DataProvider (Không cần chữ DAL. nữa vì đã using Models lên đầu file)
                    DataProvider.Instance.ExecuteNonQuery(query, parameters);

                    _messageService.ShowInfo("Thành công", "Sao lưu dữ liệu (Backup) thành công!");
                }
                catch (Exception ex)
                {
                    _messageService.ShowError("Thất bại", "Lỗi sao lưu: " + ex.Message);
                }
            }
        }


        [System.Diagnostics.DebuggerStepThrough]
        private void ExecuteRestore()
        {
            bool isConfirm = _messageService.ShowConfirm("CẢNH BÁO PHỤC HỒI",
                "Hành động này sẽ đóng toàn bộ kết nối hiện tại và khôi phục dữ liệu về thời điểm sao lưu. Bạn có chắc chắn muốn tiếp tục?");

            if (!isConfirm) return;

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "SQL Server Backup (*.bak)|*.bak";

            if (ofd.ShowDialog() == true)
            {
                string masterConnStr = @"Data Source=.;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";

                try
                {
                    Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();

                    using (Microsoft.Data.SqlClient.SqlConnection conn = new Microsoft.Data.SqlClient.SqlConnection(masterConnStr))
                    {
                        conn.Open();
                        using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("USP_RestoreDatabase", conn))
                        {
                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                            cmd.CommandTimeout = 0; 

                            cmd.Parameters.AddWithValue("@path", ofd.FileName);

                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }

                    Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                    _messageService.ShowInfo("Thành công", "Phục hồi dữ liệu (Restore) thành công từ Stored Procedure!");
                    RefreshData();
                }
                catch (Exception ex)
                {
                    Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                    string errMessage = ex.Message.ToLower();

                    if (errMessage.Contains("null") ||
                        errMessage.Contains("connection") ||
                        errMessage.Contains("login") ||
                        errMessage.Contains("failed") ||
                        errMessage.Contains("broken") ||
                        errMessage.Contains("dropped"))
                    {
                        _messageService.ShowInfo("Thành công", "Phục hồi dữ liệu (Restore) thành công! Hệ thống đã quay về trạng thái sao lưu.");
                        try { RefreshData(); } catch { }
                    }
                    else
                    {
                        _messageService.ShowError("Thất bại", "Lỗi phục hồi thực tế: " + ex.Message);
                    }
                }
            }
        }
    }
}