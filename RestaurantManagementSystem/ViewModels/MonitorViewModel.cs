using System;
using System.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.DAL;

namespace RestaurantManagementSystem.ViewModels
{
    public class MonitorViewModel : BaseViewModel
    {
        private DispatcherTimer _dataTimer;
        private DispatcherTimer _clockTimer;

        private DataView _staffList;
        public DataView StaffList { get => _staffList; set { _staffList = value; OnPropertyChanged(); } }

        private DataView _auditLogs;
        public DataView AuditLogs { get => _auditLogs; set { _auditLogs = value; OnPropertyChanged(); } }

        public string CameraNote => $"LIVE | CAMERA_01 | {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

        public ICommand RefreshCommand { get; set; }

        public MonitorViewModel()
        {
            RefreshCommand = new RelayCommand<object>(async p => await LoadDataAsync());

            _dataTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
            _dataTimer.Tick += async (s, e) => await LoadDataAsync();
            _dataTimer.Start();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => OnPropertyChanged(nameof(CameraNote));
            _clockTimer.Start();

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    string queryStaff = "SELECT * FROM View_GiamSatNhanSu WITH (NOLOCK) WHERE ChucVu NOT LIKE N'%Quản trị%'";
                    DataTable dtStaff = DataProvider.Instance.ExecuteQuery(queryStaff);

                    string queryAudit = "SELECT TOP 30 * FROM View_AuditSecurity WITH (NOLOCK) ORDER BY Ngày DESC, Giờ DESC";
                    DataTable dtAudit = DataProvider.Instance.ExecuteQuery(queryAudit);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StaffList = dtStaff.DefaultView;
                        AuditLogs = dtAudit.DefaultView;
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}