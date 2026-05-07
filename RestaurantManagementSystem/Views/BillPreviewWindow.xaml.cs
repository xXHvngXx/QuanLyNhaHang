using System.Reflection.Metadata;
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
            docViewer.Document = doc;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintDocument(docViewer.Document.DocumentPaginator, "In Hoa Don");

                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}