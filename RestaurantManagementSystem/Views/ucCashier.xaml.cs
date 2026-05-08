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
                this.ShowSuccessNotification("XUẤT BILL THÀNH CÔNG", "Hóa đơn đã được gửi tới máy in/PDF!");
            };

            this.DataContext = viewModel;
        }

        
        private void btnShowPayConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CashierViewModel vm)
            {
                // Chỉ hiện xác nhận khi đã chọn bàn và có tiền thanh toán
                if (vm.SelectedTable != null && vm.TotalAmount > 0)
                {
                    payConfirmOverlay.Visibility = Visibility.Visible;
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
            // Cập nhật nội dung thông báo
            txtNotifyTitle.Text = title;
            txtNotifyContent.Text = message;

            brdSuccessNotify.Visibility = Visibility.Visible;

            // Hiệu ứng Fade In (Mờ dần sang hiện rõ)
            DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeIn);

            // Chờ người dùng đọc tin nhắn (3 giây)
            await Task.Delay(3000);

            // Hiệu ứng Fade Out (Hiện rõ sang mờ dần)
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            fadeOut.Completed += (s, e) => brdSuccessNotify.Visibility = Visibility.Collapsed;
            brdSuccessNotify.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}