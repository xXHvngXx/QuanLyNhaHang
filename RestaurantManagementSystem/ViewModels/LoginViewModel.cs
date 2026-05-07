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
    //Kiểm tra select và fix thành thủ tục sp sau
    public class LoginViewModel : BaseViewModel
    {
        #region Properties
        private string _userName;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public Action OnLoginSuccess { get; set; }
        public Action OnLoginFail { get; set; }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; set; }
        #endregion

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>(
                async (p) => await ExecuteLogin(p),
                (p) => !IsProcessing
            );
        }

        private async Task ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null || string.IsNullOrEmpty(UserName)) return;

            string passWord = passwordBox.Password.Trim();
            IsProcessing = true; // Lúc này CanExecute sẽ trả về false, nút Login tự Disable

            try
            {
                // Chạy logic kiểm tra Database ở luồng ngầm
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
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                IsProcessing = false; // Xử lý xong, nút Login sẽ Enable trở lại
            }
        }

        private bool LoginLogic(string userName, string passWord)
        {
            string query = "EXEC USP_Login @userName";
            DataTable result = DataProvider.Instance.ExecuteQuery(query, new object[] { userName });

            if (result.Rows.Count > 0)
            {
                string hashFromDB = result.Rows[0]["Password"].ToString();

                if (SecurityHelper.VerifyPassword(passWord, hashFromDB))
                {
                    AccountDAL.LoginAccount = result.Rows[0];
                    return true;
                }
            }
            return false;
        }
    }
}