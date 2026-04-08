using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using RestaurantManagementSystem.BLL;

namespace RestaurantManagementSystem.Views
{
    public partial class ucAccount : UserControl
    {
        public ucAccount()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            dgAccount.ItemsSource = AccountBLL.Instance.GetAccounts().DefaultView;

            txtUserName.Text = "";
            txtDisplayName.Text = "";
            cbAccountType.SelectedIndex = 0;
            // Khi thêm mới thì được phép gõ UserName, khi sửa/xóa thì khóa lại
            txtUserName.IsReadOnly = false;
            txtUserName.Focus();
        }

        private void dgAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAccount.SelectedItem is DataRowView row)
            {
                txtUserName.Text = row["UserName"].ToString();
                txtDisplayName.Text = row["DisplayName"].ToString();

                // Binding lại Role: 0 là Admin, 1 là Nhân Viên
                int role = Convert.ToInt32(row["Role"]);
                cbAccountType.SelectedIndex = role;

                // Chọn trên bảng tức là đang muốn Sửa/Xóa, KHÔNG ĐƯỢC sửa UserName (Khóa chính)
                txtUserName.IsReadOnly = true;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int role = cbAccountType.SelectedIndex; // 0 hoặc 1
            string msg = AccountBLL.Instance.AddAccount(txtUserName.Text, txtDisplayName.Text, role);
            MessageBox.Show(msg, "Thông báo");
            RefreshData();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            int role = cbAccountType.SelectedIndex;
            string msg = AccountBLL.Instance.UpdateAccount(txtUserName.Text, txtDisplayName.Text, role);
            MessageBox.Show(msg, "Thông báo");
            RefreshData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Xóa tài khoản này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string msg = AccountBLL.Instance.DeleteAccount(txtUserName.Text);
                MessageBox.Show(msg, "Thông báo");
                RefreshData();
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Đặt lại mật khẩu về '1' cho tài khoản này?", "Khôi phục", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string msg = AccountBLL.Instance.ResetPassword(txtUserName.Text);
                MessageBox.Show(msg, "Thông báo");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}