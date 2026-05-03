using RestaurantManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestaurantManagementSystem.ViewModels; 
namespace RestaurantManagementSystem.Views
{
    /// <summary>
    /// Interaction logic for ucWaiter.xaml
    /// </summary>
    public partial class ucWaiter : UserControl
    {


        public ucWaiter()
        {
            InitializeComponent();
            this.DataContext = new WaiterViewModel();

        }
    }
}
