using System;
using System.Windows;
using System.Windows.Input;
using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.Views;

namespace RestaurantManagementSystem.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        public ICommand ChangePasswordCommand { get; set; }

        public ChangePasswordViewModel()
        {
            ChangePasswordCommand = new RelayCommand<ChangePasswordWindow>((p) =>
            {
                if (p == null) return;

                string oldPass = p.txtOldPass.Password;
                string newPass = p.txtNewPass.Password;
                string confirmPass = p.txtConfirmPass.Password;

                if (string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // Gọi BLL xử lý
                string msg = AccountBLL.Instance.ChangePassword(oldPass, newPass, confirmPass);

                MessageBox.Show(msg, "Thông báo");

                if (msg.Contains("thành công"))
                {
                    p.Close();
                }
            });
        }
    }
}