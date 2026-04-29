using RestaurantManagementSystem.BLL;
using RestaurantManagementSystem.ViewModels;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantManagementSystem.Views
{
    public partial class ucAccount : UserControl
    {
        public ucAccount()
        {
            InitializeComponent();
            this.DataContext = new AccountViewModel();
        }
    }
}