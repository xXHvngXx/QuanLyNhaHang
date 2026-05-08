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
        private readonly DispatcherTimer _dataTimer;
        private readonly DispatcherTimer _clockTimer;

        #region Properties
        private DataView _staffList;
        public DataView StaffList { get => _staffList; set => SetProperty(ref _staffList, value); }

        private DataView _auditLogs;
        public DataView AuditLogs { get => _auditLogs; set => SetProperty(ref _auditLogs, value); }

        // CameraNote sẽ tự cập nhật mỗi giây nhờ OnPropertyChanged
        public string CameraNote => $"LIVE | CAMERA_01 | {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        #endregion

        #region Commands
        public ICommand RefreshCommand { get; set; }
        #endregion

        public MonitorViewModel()
        {
            RefreshCommand = new RelayCommand<object>(async p => await LoadDataAsync());

            // Timer cập nhật dữ liệu (mỗi 5 giây)
            _dataTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _dataTimer.Tick += async (s, e) => await LoadDataAsync();
            _dataTimer.Start();

            // Timer cập nhật đồng hồ Camera (mỗi 1 giây)
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => OnPropertyChanged(nameof(CameraNote));
            _clockTimer.Start();

            // Load lần đầu
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

                    string queryAudit = @"SELECT TOP 30 
                                    [Ngày], 
                                    [Giờ], 
                                    [Mã Bill], 
                                    [Hành Động], 
                                    [Tiền Trước], 
                                    [Tiền Sau], 
                                    [Nhân Viên], 
                                    [Chi Tiết] 
                                  FROM View_AuditSecurity WITH (NOLOCK) 
                                  ORDER BY [Ngày] DESC, [Giờ] DESC";

                    DataTable dtAudit = DataProvider.Instance.ExecuteQuery(queryAudit);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StaffList = dtStaff?.DefaultView;
                        AuditLogs = dtAudit?.DefaultView;
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Monitor Error: " + ex.Message);
            }
        }

        public void Cleanup()
        {
            _dataTimer?.Stop();
            _clockTimer?.Stop();
        }
    }
}