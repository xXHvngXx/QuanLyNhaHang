using System;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public ICommand ChangeTabCommand { get; set; }

        public AdminViewModel()
        {
            SelectedTabIndex = 0; // Mặc định mở Tab đầu tiên (thường là Món ăn hoặc Thống kê)

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