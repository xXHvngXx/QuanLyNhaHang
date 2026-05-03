using System.Windows.Controls;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Views
{
    public partial class ucMonitor : UserControl
    {
        public ucMonitor()
        {
            InitializeComponent();

            // Gán DataContext cho View để các Binding trong XAML hoạt động
            this.DataContext = new MonitorViewModel();
        }
    }
}