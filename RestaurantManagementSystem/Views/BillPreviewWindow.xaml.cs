using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RestaurantManagementSystem.Views
{
    public partial class BillPreviewWindow : Window
    {
        public BillPreviewWindow(FixedDocument doc)
        {
            InitializeComponent();

            if (doc != null)
            {
                docViewer.Document = doc;
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pd = new PrintDialog();

                // Hiển thị hộp thoại chọn máy in
                if (pd.ShowDialog() == true)
                {
                    // Thực hiện in tài liệu
                    pd.PrintDocument(docViewer.Document.DocumentPaginator, "In hóa đơn nhà hàng");

                    // Trả về true để báo cho phía gọi window này biết là đã in thành công
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi máy in: " + ex.Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}