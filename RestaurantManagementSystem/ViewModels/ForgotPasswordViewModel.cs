using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        public Action OnResetSuccess { get; set; }
        public Action<string> OnResetFail { get; set; }

        #region Properties
        private string _userName;
        public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); } }

        private string _inputDisplayName;
        public string InputDisplayName { get => _inputDisplayName; set { _inputDisplayName = value; OnPropertyChanged(); } }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        #endregion

        public ICommand ResetCommand { get; set; }

        public ForgotPasswordViewModel()
        {
            ResetCommand = new RelayCommand<PasswordBox>((p) => ExecuteReset(p), (p) => true);
        }

        public void ExecuteReset(object p = null)
        {

            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(InputDisplayName) ||
                string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                OnResetFail?.Invoke("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                OnResetFail?.Invoke("Mật khẩu xác nhận không khớp!");
                return;
            }

            if (!AccountDAL.Instance.CheckAccountExist(UserName))
            {
                OnResetFail?.Invoke("Tài khoản không tồn tại.");
                return;
            }

            string realDisplayName = AccountDAL.Instance.GetDisplayNameByUserName(UserName);
            if (InputDisplayName != realDisplayName)
            {
                OnResetFail?.Invoke("Tên hiển thị không khớp.");
                return;
            }

            string hashedPass = SecurityHelper.HashPassword(NewPassword);
            if (AccountDAL.Instance.ResetPassword(UserName, hashedPass))
            {
                OnResetSuccess?.Invoke();
            }
            else
            {
                OnResetFail?.Invoke("Có lỗi xảy ra khi cập nhật!");
            }
        }
    }
}