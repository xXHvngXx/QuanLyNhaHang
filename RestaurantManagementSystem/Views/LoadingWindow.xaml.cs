using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace RestaurantManagementSystem.Views
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            StartLoadingLogic();
        }

        private void StartLoadingLogic()
        {
            if (this.Resources["MasterLoadingAnim"] is Storyboard sb)
            {
                sb.Completed += (s, e) =>
                {
                    LoginWindow login = new LoginWindow();

                    login.Show();

                    this.Close();
                };

                sb.Begin();
            }
        }
    }
}