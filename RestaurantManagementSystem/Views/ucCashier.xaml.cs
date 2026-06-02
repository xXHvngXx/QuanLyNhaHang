using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class ucCashier : UserControl
    {
        public ucCashier()
        {
            InitializeComponent();

            var messageService = new MessageService();
            var viewModel = new CashierViewModel(messageService);

            // Đăng ký sự kiện thông báo khi in hóa đơn thành công
            viewModel.OnPrintSuccess = () => {
                this.ShowSuccessNotification("XUẤT BILL THÀNH CÔNG !");
            };

            this.DataContext = viewModel;
        }


        private void btnShowPayConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CashierViewModel vm)
            {
                if (vm.SelectedTable != null && vm.TotalAmount > 0)
                {
                    txtCustomerMoney.Text = "";
                    lblChangeMoney.Text = "0 VNĐ";
                    btnConfirmPay.IsEnabled = false;

                    btnCancelPay.IsEnabled = true;

                    payConfirmOverlay.Visibility = Visibility.Visible;
                    txtCustomerMoney.Focus();
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn bàn có hóa đơn để thanh toán!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        private void btnCancelPay_Click(object sender, RoutedEventArgs e)
        {
            payConfirmOverlay.Visibility = Visibility.Collapsed;
        }


        private void btnConfirmPay_Click(object sender, RoutedEventArgs e)
        {
            payConfirmOverlay.Visibility = Visibility.Collapsed;

            if (this.DataContext is CashierViewModel vm)
            {
                if (vm.SelectedTable == null) return;

                string tableName = vm.SelectedTable["TableName"]?.ToString() ?? "vừa chọn";

                if (vm.PayCommand != null && vm.PayCommand.CanExecute(null))
                {
                    try
                    {
                        vm.PayCommand.Execute(null);

                        ShowSuccessNotification("THANH TOÁN THÀNH CÔNG", $"{tableName} đã được cập nhật trạng thái trống.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi thanh toán: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        public async void ShowSuccessNotification(string title = "THÀNH CÔNG", string message = "Thao tác đã hoàn tất!")
        {
            txtNotifyTitle.Text = title;
            txtNotifyContent.Text = message;

            brdSuccessNotify.Visibility = Visibility.Visible;

            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(3000);

            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, e) => brdSuccessNotify.Visibility = Visibility.Collapsed;
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void txtCustomerMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.DataContext is CashierViewModel vm)
            {
                string rawText = txtCustomerMoney.Text.Replace(",", "").Replace(".", "").Trim();

                if (string.IsNullOrEmpty(rawText))
                {
                    lblChangeMoney.Text = "0 VNĐ";
                    btnConfirmPay.IsEnabled = false;

                    btnCancelPay.IsEnabled = true;

                    vm.CustomerMoney = 0;
                    vm.ChangeMoney = 0;
                    return;
                }

                if (decimal.TryParse(rawText, out decimal customerMoney))
                {
             
                    if (customerMoney != 0)
                    {
                        btnCancelPay.IsEnabled = false;
                    }
                    else
                    {
                        btnCancelPay.IsEnabled = true; 
                    }

                    txtCustomerMoney.TextChanged -= txtCustomerMoney_TextChanged;
                    txtCustomerMoney.Text = string.Format("{0:N0}", customerMoney);
                    txtCustomerMoney.SelectionStart = txtCustomerMoney.Text.Length;
                    txtCustomerMoney.TextChanged += txtCustomerMoney_TextChanged;

                    decimal totalAmount = vm.TotalAmount;
                    decimal change = customerMoney - totalAmount;

                    vm.CustomerMoney = customerMoney;
                    vm.ChangeMoney = change >= 0 ? change : 0;

                    if (change >= 0)
                    {
                        lblChangeMoney.Text = string.Format("{0:N0} VNĐ", change);
                        lblChangeMoney.Foreground = System.Windows.Media.Brushes.Navy;
                        btnConfirmPay.IsEnabled = true;
                    }
                    else
                    {
                        lblChangeMoney.Text = $"Thiếu {string.Format("{0:N0}", Math.Abs(change))} VNĐ";
                        lblChangeMoney.Foreground = System.Windows.Media.Brushes.Crimson;
                        btnConfirmPay.IsEnabled = false;
                    }
                }
            }
        }
    }
}