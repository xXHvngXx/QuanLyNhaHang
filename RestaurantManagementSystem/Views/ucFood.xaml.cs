using System.Windows.Controls;
using RestaurantManagementSystem.ViewModels;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Views
{
    public partial class ucFood : UserControl
    {
        public ucFood()
        {
            InitializeComponent();
            this.DataContext = new FoodViewModel(new MessageService());
        }
    }
}