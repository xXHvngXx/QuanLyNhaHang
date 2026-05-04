using System.Windows;
using System.Windows.Input;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Views
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
            this.DataContext = new ChangePasswordViewModel();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Eye_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn.Tag.ToString() == "Old") ShowPass(txtOldPass, txtOldPassVisible);
            else if (btn.Tag.ToString() == "New") ShowPass(txtNewPass, txtNewPassVisible);
            else if (btn.Tag.ToString() == "Confirm") ShowPass(txtConfirmPass, txtConfirmPassVisible);
        }

        private void Eye_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn.Tag.ToString() == "Old") HidePass(txtOldPass, txtOldPassVisible);
            else if (btn.Tag.ToString() == "New") HidePass(txtNewPass, txtNewPassVisible);
            else if (btn.Tag.ToString() == "Confirm") HidePass(txtConfirmPass, txtConfirmPassVisible);
        }

        private void ShowPass(System.Windows.Controls.PasswordBox p, System.Windows.Controls.TextBox t)
        {
            t.Text = p.Password;
            p.Visibility = Visibility.Collapsed;
            t.Visibility = Visibility.Visible;
        }

        private void HidePass(System.Windows.Controls.PasswordBox p, System.Windows.Controls.TextBox t)
        {
            p.Visibility = Visibility.Visible;
            t.Visibility = Visibility.Collapsed;
        }
    }
}