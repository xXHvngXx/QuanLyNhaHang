using RestaurantManagementSystem.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantManagementSystem.Views
{
    public partial class ucMonitor : UserControl
    {
        public ucMonitor()
        {
            InitializeComponent();

            this.DataContext = new MonitorViewModel();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MonitorViewModel vm)
            {
                vm.Cleanup();
            }
        }
    }
}