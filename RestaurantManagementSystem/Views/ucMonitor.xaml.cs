using System.Windows.Controls;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Views
{
    public partial class ucMonitor : UserControl
    {
        public ucMonitor()
        {
            InitializeComponent();

            this.DataContext = new MonitorViewModel();
        }
    }
}