using System.Windows;
using System.Windows.Controls;
using System.Data;
using RestaurantManagementSystem.ViewModels;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Views
{
    public partial class ucCategory : UserControl
    {
        public ucCategory()
        {
            InitializeComponent();
            this.DataContext = new CategoryViewModel(new MessageService());
        }
    }
}