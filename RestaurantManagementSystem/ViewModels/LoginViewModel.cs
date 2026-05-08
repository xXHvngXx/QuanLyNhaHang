using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;


namespace RestaurantManagementSystem.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        #region Properties
        private string _userName;
        public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

        private bool _isProcessing;
        public bool IsProcessing { get => _isProcessing; set => SetProperty(ref _isProcessing, value); }

        public Action OnLoginSuccess { get; set; }
        public Action OnLoginFail { get; set; }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; set; }
        #endregion

        public LoginViewModel(IMessageService messageService)
        {
            _messageService = messageService;

            // Khởi tạo Command với async/await chuẩn
            LoginCommand = new RelayCommand<PasswordBox>(
                async (p) => await ExecuteLogin(p),
                (p) => !IsProcessing
            );
        }

        private async Task ExecuteLogin(PasswordBox passwordBox)
        {
            if (passwordBox == null || string.IsNullOrWhiteSpace(UserName))
            {
                _messageService.ShowError("Thông báo", "Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            string passWord = passwordBox.Password.Trim();
            IsProcessing = true;

            try
            {
                // Task.Run giúp giải phóng UI Thread khi truy vấn SQL
                bool isSuccess = await Task.Run(() => LoginLogic(UserName, passWord));

                if (isSuccess)
                {
                    OnLoginSuccess?.Invoke();
                }
                else
                {
                    OnLoginFail?.Invoke();
                   
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", ex.Message);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool LoginLogic(string userName, string passWord)
        {
            string query = "EXEC USP_Login @userName";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result != null && result.Rows.Count > 0)
            {
                string hashFromDB = result.Rows[0]["Password"].ToString();

                // Kiểm tra bảo mật bằng Helper
                if (SecurityHelper.VerifyPassword(passWord, hashFromDB))
                {
                    // Lưu thông tin người đăng nhập vào tầng DAL
                    AccountDAL.LoginAccount = result.Rows[0];
                    return true;
                }
            }
            return false;
        }
    }
}