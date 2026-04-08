using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RestaurantManagementSystem.Views
{
    public partial class ucMonitor : UserControl
    {
        DispatcherTimer timer;

        public ucMonitor()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); 
            timer.Tick += (s, e) => LoadData();
            timer.Start();
        }

        private void LoadData()
        {
            // 1. Load thẻ nhân sự (View mới)
            icStaff.ItemsSource = DataProvider.Instance.ExecuteQuery("SELECT * FROM View_GiamSatNhanSu").DefaultView;

            // 2. Load nhật ký audit 
            dgAudit.ItemsSource = DataProvider.Instance.ExecuteQuery("SELECT TOP 30 * FROM View_AuditSecurity ORDER BY Ngày DESC, Giờ DESC").DefaultView;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadData();
        private void UserControl_Loaded(object sender, RoutedEventArgs e) => LoadData();
    }
}