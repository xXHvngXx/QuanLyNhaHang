using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set { _selectedTabIndex = value; OnPropertyChanged(); }
        }

        public ICommand ChangeTabCommand { get; set; }

        public AdminViewModel()
        {
            SelectedTabIndex = 0; // Mặc định mở Quản lý món ăn
            ChangeTabCommand = new RelayCommand<string>((p) =>
            {
                if (int.TryParse(p, out int index))
                {
                    SelectedTabIndex = index;
                }
            });
        }
    }
}
