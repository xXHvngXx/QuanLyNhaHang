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
                        IsUserNameReadOnly = true; 

                        // Lấy mã Role thực tế từ cơ sở dữ liệu (Ví dụ: 0, 1, 2, 21, 22, 23...)
                        int currentRole = Convert.ToInt32(value["Role"]);

                        // QUẢN TRỊ VIÊN (Role = 0)
                        if (currentRole == 0)
                        {
                            AccountType = 0;
                            IsFormatStaff = false;   // Ẩn bảng Phục vụ
                            IsFormatCashier = false; // Ẩn bảng Thu ngân
                        }
                        else if (currentRole == -1)
                        {
                            AccountType = -1;        // Đóng băng
                            IsFormatStaff = false;   // Ẩn bảng Phục vụ
                            IsFormatCashier = false; // Ẩn bảng Thu ngân
                        }
                        // THU NGÂN (Role = 2 hoặc các mã bị giới hạn quyền như 21, 22, 23)
                        else if (currentRole == 2 || currentRole == 21 || currentRole == 22 || currentRole == 23)
                        {
                            AccountType = 2; // Chọn mục Thu ngân ở ComboBox Loại tài khoản
                            IsFormatStaff = false;   // Ẩn bảng Phục vụ
                            IsFormatCashier = true;  // Mở hiện bảng CheckBox của Thu ngân

                            // Kiểm tra xem trong DB tài khoản này đang bị phạt/khóa quyền gì thì bỏ tích tương ứng
                            if (currentRole == 21)
                            {
                                CanCheckout = false;    
                                CanPrintDraft = true;
                            }
                            else if (currentRole == 22)
                            {
                                CanCheckout = true;     
                                CanPrintDraft = false;  
                            }
                            else if (currentRole == 23)
                            {
                                CanCheckout = false;   
                                CanPrintDraft = false;
                            }
                            else
                            {
                                CanCheckout = true;     // Role = 2: Đầy đủ quyền chuẩn
                                CanPrintDraft = true;
                            }
                        }
                        // PHỤC VỤ (Role = 1 hoặc các mã hạn chế 11, 12, 13...)
                        else
                        {
                            AccountType = 1;
                            IsFormatStaff = true;    // Hiện bảng Phục vụ (để tạm đó, xử lý sau)
                            IsFormatCashier = false; // Ẩn bảng Thu ngân

                            CanEditFood = true;
                            CanDeleteFood = true;
                            CanSendFood = true;

                            // Kiểm tra xem trong DB tài khoản này đang bị phạt/khóa quyền gì thì bỏ tích tương ứng
                            if (currentRole == 11) CanEditFood = false;
                            else if (currentRole == 12) CanDeleteFood = false;
                            else if (currentRole == 13) CanSendFood = false;
                            else if (currentRole == 14) { CanEditFood = false; CanDeleteFood = false; }
                            else if (currentRole == 15) { CanEditFood = false; CanDeleteFood = false; CanSendFood = false; }
                        }
                    }
                    else
                    {
                        // Reset dữ liệu về trống khi không chọn dòng nào
                        UserName = "";
                        DisplayName = "";
                        AccountType = -99;
                        IsUserNameReadOnly = false;
                        IsFormatStaff = false;
                        IsFormatCashier = false;
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

        private bool _isFormatStaff;
        public bool IsFormatStaff
        {
            get => _isFormatStaff;
            set => SetProperty(ref _isFormatStaff, value);
        }

        private bool _canEditFood;
        public bool CanEditFood
        {
            get => _canEditFood;
            set => SetProperty(ref _canEditFood, value);
        }

        private bool _canDeleteFood;
        public bool CanDeleteFood
        {
            get => _canDeleteFood;
            set => SetProperty(ref _canDeleteFood, value);
        }

        private bool _canSendFood;
        public bool CanSendFood
        {
            get => _canSendFood;
            set => SetProperty(ref _canSendFood, value);
        }

        private bool _isFormatCashier;
        public bool IsFormatCashier
        {
            get => _isFormatCashier;
            set => SetProperty(ref _isFormatCashier, value);
        }

        private bool _canPrintDraft = true;
        public bool CanPrintDraft
        {
            get => _canPrintDraft;
            set => SetProperty(ref _canPrintDraft, value);
        }

        private bool _canCheckout = true;
        public bool CanCheckout
        {
            get => _canCheckout;
            set => SetProperty(ref _canCheckout, value);
        }

        public ICommand UpdateStaffPermissionCommand { get; set; }
        public ICommand UpdateCashierPermissionCommand { get; set; }
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

            // --- KHỞI TẠO COMMAND: ÁP ĐẶT QUYỀN CHO PHỤC VỤ ---
            UpdateStaffPermissionCommand = new RelayCommand<object>(
                (p) => {
                    try
                    {
                        int calculatedRole = 1; // Mặc định ban đầu đầy đủ quyền

                        // Tính toán mã Role phạt dựa theo các CheckBox bị bỏ tích (false)
                        if (!CanEditFood && CanDeleteFood && CanSendFood) calculatedRole = 11;
                        else if (CanEditFood && !CanDeleteFood && CanSendFood) calculatedRole = 12;
                        else if (CanEditFood && CanDeleteFood && !CanSendFood) calculatedRole = 13;
                        else if (!CanEditFood && !CanDeleteFood && CanSendFood) calculatedRole = 14;
                        else if (!CanEditFood && !CanDeleteFood && !CanSendFood) calculatedRole = 15;

                        string query = string.Format("UPDATE dbo.Account SET Role = {0} WHERE UserName = N'{1}'", calculatedRole, UserName);
                        DataProvider.Instance.ExecuteNonQuery(query);

                        _messageService.ShowInfo("Thành công", string.Format("Đã áp đặt giới hạn tính năng cho Phục vụ: {0}", UserName));
                        RefreshData(); // Tải lại bảng để cập nhật giao diện
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError("Lỗi hệ thống", "Không thể cập nhật quyền phục vụ: " + ex.Message);
                    }
                },
                (p) => SelectedItem != null && AccountType == 1 // Chỉ sáng nút khi đang chọn một tài khoản Phục vụ
            );

            // --- KHỞI TẠO COMMAND: ÁP ĐẶT QUYỀN CHO THU NGÂN ---
            UpdateCashierPermissionCommand = new RelayCommand<object>(
                (p) => {
                    try
                    {
                        int calculatedRole = 2; // Mặc định đầy đủ quyền

            
                        // Cấm Thanh Toán (CanCheckout == false)
                        // Cấm In (CanPrintDraft == false)

                        if (!CanCheckout && !CanPrintDraft) calculatedRole = 23; // Cấm cả hai
                        else if (!CanCheckout) calculatedRole = 21;              // Chỉ cấm Thanh toán
                        else if (!CanPrintDraft) calculatedRole = 22;            // Chỉ cấm In
                        else calculatedRole = 2;                                 // Đầy đủ quyền

                        string query = string.Format("UPDATE dbo.Account SET Role = {0} WHERE UserName = N'{1}'", calculatedRole, UserName);
                        DataProvider.Instance.ExecuteNonQuery(query);

                        _messageService.ShowInfo("Thành công", string.Format("Đã áp đặt giới hạn cho Thu ngân: {0}", UserName));

                        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError("Lỗi hệ thống", "Không thể cập nhật quyền thu ngân: " + ex.Message);
                    }
                },
                (p) => SelectedItem != null && AccountType == 2
            );
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

            IsFormatStaff = false;
            IsFormatCashier = false;

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