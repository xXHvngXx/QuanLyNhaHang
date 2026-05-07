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

            viewModel.OnPrintSuccess = () => {
                this.ShowSuccessNotification("XUẤT BILL THÀNH CÔNG", "Hóa đơn đã được gửi tới máy in/PDF!");
            };

            this.DataContext = viewModel;
        }


        private void btnShowPayConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CashierViewModel vm)
            {
                if (vm.SelectedTable != null && vm.TotalAmount > 0)
                {
                    payConfirmOverlay.Visibility = Visibility.Visible;
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
                if (vm.PayCommand != null && vm.PayCommand.CanExecute(null))
                {
                    vm.PayCommand.Execute(null);
                }
            }

            ShowSuccessNotification("THANH TOÁN THÀNH CÔNG", "Bàn đã được cập nhật trạng thái trống.");
        }


        public async void ShowSuccessNotification(string title = "THÀNH CÔNG", string message = "Thao tác đã hoàn tất!")
        {
            txtNotifyTitle.Text = title;
            txtNotifyContent.Text = message;

            brdSuccessNotify.Visibility = Visibility.Visible;

            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(2500);

            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, e) => brdSuccessNotify.Visibility = Visibility.Collapsed;
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}