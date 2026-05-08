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
        private readonly IMessageService _messageService;

        #region Properties
        private string _regUser;
        public string RegUser { get => _regUser; set => SetProperty(ref _regUser, value); }

        private bool _isProcessing;
        public bool IsProcessing { get => _isProcessing; set => SetProperty(ref _isProcessing, value); }

        public Action OnRegisterSuccess { get; set; }
        public Action<string, string> OnRegisterFail { get; set; }
        #endregion

        #region Commands
        public ICommand RegisterCommand { get; set; }
        #endregion

        public RegisterViewModel(IMessageService messageService)
        {
            _messageService = messageService;

            RegisterCommand = new RelayCommand<PasswordBox>(
                async (p) => await ExecuteRegister(p),
                (p) => !IsProcessing
            );
        }

        private async Task ExecuteRegister(PasswordBox passwordBox)
        {
            if (passwordBox == null) return;

            string user = (RegUser ?? "").Trim();
            string pass = passwordBox.Password;
            string displayName = "New Account";

            IsProcessing = true;

            try
            {
                // Validate định dạng tài khoản
                if (user.Length < 5 || !Regex.IsMatch(user, @"^[a-zA-Z0-9]+$"))
                {
                    OnRegisterFail?.Invoke("Tên tài khoản không hợp lệ!", "Phải từ 5 ký tự và không chứa ký tự đặc biệt.");
                    return;
                }

                // Validate độ dài mật khẩu
                if (pass.Length < 3)
                {
                    OnRegisterFail?.Invoke("Mật khẩu quá ngắn!", "Vui lòng nhập mật khẩu dài hơn.");
                    return;
                }

                // Kiểm tra tồn tại và Insert (Chạy ngầm)
                bool isSuccess = await Task.Run(() =>
                {
                    if (AccountDAL.Instance.CheckAccountExist(user))
                    {
                        return false;
                    }

                    string hashedPass = SecurityHelper.HashPassword(pass);
                    return AccountDAL.Instance.InsertAccount(user, displayName, hashedPass, -1);
                });

                if (isSuccess)
                {
                    _messageService.ShowInfo("Thành công", "Tài khoản đã được tạo! Vui lòng liên hệ Admin để được cấp quyền truy cập.");
                    OnRegisterSuccess?.Invoke();
                }
                else
                {
                    OnRegisterFail?.Invoke("Thất bại", "Tài khoản đã tồn tại hoặc có lỗi xảy ra.");
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
    }
}