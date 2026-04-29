using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;
using System.Printing;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantManagementSystem.Views
{
    public partial class ucCashier : UserControl
    {
        public ucCashier()
        {
            InitializeComponent();
            this.DataContext = new CashierViewModel(new MessageService());
        }

        private void btnShowBill_Click(object sender, RoutedEventArgs e)
        {
            receiptOverlay.Visibility = Visibility.Visible;
        }

        private void btnCloseReceipt_Click(object sender, RoutedEventArgs e)
        {
            receiptOverlay.Visibility = Visibility.Collapsed;
        }

        private void btnExportReceipt_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(printTicket, "HoaDonNhaHang");
            }
        }
    }
}