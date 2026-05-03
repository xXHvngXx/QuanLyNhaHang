using LiveCharts;
using LiveCharts.Wpf;
using RestaurantManagementSystem.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace RestaurantManagementSystem.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        // Properties cho UI Binding
        private DateTime _fromDate;
        public DateTime FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime _toDate;
        public DateTime ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

        private DataView _billList;
        public DataView BillList { get => _billList; set { _billList = value; OnPropertyChanged(); } }

        // LiveCharts Properties
        public ChartValues<double> ChartValues { get; set; }
        public List<string> ChartLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        // Commands
        public ICommand LoadReportCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }

        // Delegate báo về View xử lý UI (màu sắc biểu đồ)
        public Action<double> OnReportLoaded;

        public ReportViewModel()
        {
            // 1. Khởi tạo giá trị mặc định
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;
            ChartValues = new ChartValues<double>();
            ChartLabels = new List<string>();
            YFormatter = value => value.ToString("N0") + " đ";

            // 2. FIX FULL: Khởi tạo Command khớp với RelayCommand<T>
            // Tham số 1: Action<object> (Hàm thực thi)
            // Tham số 2: Predicate<object> (Điều kiện thực thi)
            LoadReportCommand = new RelayCommand<object>(
                (p) => { LoadReport(); },
                (p) => true
            );

            ExportExcelCommand = new RelayCommand<object>(
                (p) => { ExportExcel(); },
                (p) => true
            );
        }

        public void LoadReport()
        {
            try
            {
                // Lấy dữ liệu từ tầng BLL
                DataTable dtBill = BillBLL.Instance.GetListBill(FromDate, ToDate);
                BillList = dtBill.DefaultView;

                DataTable dtChart = BillBLL.Instance.GetChartData(FromDate, ToDate);

                ChartValues.Clear();
                ChartLabels.Clear();
                double totalRevenue = 0;

                if (dtChart != null && dtChart.Rows.Count > 0)
                {
                    foreach (DataRow row in dtChart.Rows)
                    {
                        double dailyTotal = Convert.ToDouble(row["Total"]);
                        ChartValues.Add(dailyTotal);
                        ChartLabels.Add(Convert.ToDateTime(row["Date"]).ToString("dd/MM"));
                        totalRevenue += dailyTotal;
                    }
                    double average = totalRevenue / dtChart.Rows.Count;

                    // Bắn sự kiện về View để cập nhật màu Stroke/Fill
                    OnReportLoaded?.Invoke(average);
                }
                else
                {
                    ChartValues.Add(0);
                    ChartLabels.Add("N/A");
                    OnReportLoaded?.Invoke(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi LoadReport: " + ex.Message);
            }
        }

        private void ExportExcel()
        {
            try
            {
                string fromStr = FromDate.ToString("yyyy-MM-dd");
                string toStr = ToDate.ToString("yyyy-MM-dd");
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = System.IO.Path.Combine(appFolder, "export_report.py");

                if (!System.IO.File.Exists(scriptPath))
                {
                    MessageBox.Show("Không tìm thấy file script Python!");
                    return;
                }

                ProcessStartInfo start = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" {fromStr} {toStr} .",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                using (Process process = Process.Start(start))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (output.Contains("Success:"))
                    {
                        string filePath = output.Replace("Success: ", "").Trim();
                        MessageBox.Show("Xuất báo cáo thành công sếp ơi!", "Thông báo");
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    else
                    {
                        MessageBox.Show("Lỗi Python: " + (string.IsNullOrEmpty(error) ? output : error));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}