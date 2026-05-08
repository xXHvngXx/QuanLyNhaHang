using System;
using System.Windows;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.Views;

namespace RestaurantManagementSystem.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private readonly IMessageService _messageService;

        #region Properties
        #endregion

        #region Commands
        public ICommand ChangePasswordCommand { get; set; }
        #endregion

        public ChangePasswordViewModel(IMessageService messageService)
        {
            _messageService = messageService;
            InitCommands();
        }

        private void InitCommands()
        {
            ChangePasswordCommand = new RelayCommand<ChangePasswordWindow>(
                execute: (window) => ExecuteChangePassword(window),
                canExecute: (window) => window != null
            );
        }

        private void ExecuteChangePassword(ChangePasswordWindow window)
        {
            string oldPass = window.txtOldPass.Password;
            string newPass = window.txtNewPass.Password;
            string confirmPass = window.txtConfirmPass.Password;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
            {
                _messageService.ShowError("Thông báo", "Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                string msg = AccountBLL.Instance.ChangePassword(oldPass, newPass, confirmPass);

                if (msg.Contains("thành công"))
                {
                    window.Tag = "SUCCESS";
                    _messageService.ShowInfo("Thành công", msg);
                    window.Close(); // Thường đổi mật khẩu xong sẽ đóng cửa sổ
                }
                else
                {
                    window.Tag = "FAILED";
                    _messageService.ShowError("Lỗi", msg);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError("Lỗi hệ thống", ex.Message);
            }
        }
    }
}