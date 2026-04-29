using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        #region Properties
        private string _regUser;
        public string RegUser
        {
            get => _regUser;
            set => SetProperty(ref _regUser, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        // Tín hiệu báo View chạy hiệu ứng thành công/thất bại
        public Action OnRegisterSuccess { get; set; }
        public Action<string, string> OnRegisterFail { get; set; }
        #endregion

        #region Commands
        public ICommand RegisterCommand { get; set; }
        #endregion

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand<object>(
                async (p) => await ExecuteRegister(p), 
                (p) => !IsProcessing                   
            );
        }

        private async Task ExecuteRegister(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string user = (RegUser ?? "").Trim();
            string pass = passwordBox.Password;
            string displayName = "Nhân Viên Phục Vụ"; 

            IsProcessing = true;

            try
            {
                if (user.Length < 5 || !Regex.IsMatch(user, @"^[a-zA-Z0-9]+$"))
                {
                    OnRegisterFail?.Invoke("Tên tài khoản không hợp lệ!", "Phải từ 5 ký tự và không chứa ký tự đặc biệt.");
                    return;
                }

                if (pass.Length < 3)
                {
                    OnRegisterFail?.Invoke("Mật khẩu quá ngắn!", "Vui lòng nhập mật khẩu dài hơn.");
                    return;
                }

                bool isExist = await Task.Run(() => AccountDAL.Instance.CheckAccountExist(user));
                if (isExist)
                {
                    OnRegisterFail?.Invoke("Tài khoản đã tồn tại!", "Vui lòng chọn một tên đăng nhập khác.");
                    return;
                }

                string hashedPass = SecurityHelper.HashPassword(pass);
                bool isSuccess = await Task.Run(() => AccountDAL.Instance.InsertAccount(user, displayName, hashedPass, 1));

                if (isSuccess)
                    OnRegisterSuccess?.Invoke();
                else
                    MessageBox.Show("Đăng ký thất bại, vui lòng thử lại!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}