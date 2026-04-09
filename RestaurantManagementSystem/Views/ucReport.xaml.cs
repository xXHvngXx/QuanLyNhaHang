using LiveCharts;
using LiveCharts.Wpf;
using RestaurantManagementSystem.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics; 
using System.IO;

namespace RestaurantManagementSystem.Views
{
    public partial class ucReport : UserControl
    {
        // 1. Khai báo Property cho Binding
        public ChartValues<double> ChartValues { get; set; }
        public List<string> ChartLabels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ucReport()
        {
            InitializeComponent();

            ChartValues = new ChartValues<double>();
            ChartLabels = new List<string>();
            YFormatter = value => value.ToString("N0") + " đ";

            // Cầu nối DataContext
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Mặc định lùi 7 ngày để demo dữ liệu
            dtpFrom.SelectedDate = DateTime.Now.AddDays(-7);
            dtpTo.SelectedDate = DateTime.Now;

            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                // Kiểm tra nếu các DatePicker chưa có giá trị
                DateTime from = dtpFrom.SelectedDate ?? DateTime.Now.AddDays(-7);
                DateTime to = dtpTo.SelectedDate ?? DateTime.Now;

                // --- PHẦN 1: ĐỔ DỮ LIỆU BẢNG ---
                DataTable dtBill = BillBLL.Instance.GetListBill(from, to);
                if (dgBill != null) dgBill.ItemsSource = dtBill.DefaultView;

                // --- PHẦN 2: XỬ LÝ BIỂU ĐỒ ---
                DataTable dtChart = BillBLL.Instance.GetChartData(from, to);

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

                    // --- PHẦN 3: FLEX COLOR (NGẢ MÀU KINH TẾ) ---
                    double averageRevenue = totalRevenue / dtChart.Rows.Count;
                    UpdateChartColor(averageRevenue);
                }
                else
                {
                    // Nếu không có dữ liệu, vẽ một điểm 0 để biểu đồ không empty
                    ChartValues.Add(0);
                    ChartLabels.Add("N/A");
                    UpdateChartColor(0);
                }
            }
            catch (Exception)
            {
                // Silent catch hoặc log lỗi (nếu cần)
            }
        }

        // Hàm phụ trách việc "đổi màu" dựa trên doanh thu trung bình
        private void UpdateChartColor(double average)
        {
            // Kiểm tra ChartSeries đã được khởi tạo trong XAML chưa
            if (ChartSeries == null) return;

            var bc = new BrushConverter();
            string strokeColor, fillColor;

            if (average >= 400000) // 🟢 XANH LÁ: KINH TẾ TỐT
            {
                strokeColor = "#2ECC71";
                fillColor = "#202ECC71";
            }
            else if (average >= 300000) // 🟡 VÀNG: CẢNH BÁO
            {
                strokeColor = "#F1C40F";
                fillColor = "#20F1C40F";
            }
            else // 🔴 ĐỎ: NGUY HIỂM / ĐE DỌA
            {
                strokeColor = "#E74C3C";
                fillColor = "#20E74C3C";
            }

            ChartSeries.Stroke = (Brush)bc.ConvertFrom(strokeColor);
            ChartSeries.Fill = (Brush)bc.ConvertFrom(fillColor);
        }

        private void btnViewReport_Click(object sender, RoutedEventArgs e) => LoadReport();

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. ĐỊNH NGHĨA BIẾN 
                string from = dtpFrom.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                string to = dtpTo.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

                // Lấy đường dẫn tuyệt đối của thư mục chứa file .exe đang chạy 
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = System.IO.Path.Combine(appFolder, "export_report.py");

                // 2. KIỂM TRA FILE TỒN TẠI
                if (!System.IO.File.Exists(scriptPath))
                {
                    MessageBox.Show("Chưa thấy file Python tại: " + scriptPath, "Lỗi đường dẫn");
                    return;
                }

                // 3. CẤU HÌNH GỌI PYTHON
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python";

                // Truyền tham số: script, từ ngày, đến ngày và dấu "." cho server portable
                start.Arguments = $"\"{scriptPath}\" {from} {to} .";

                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.CreateNoWindow = true;

                // ĐẢM BẢO HIỂN THỊ TIẾNG VIỆT KHÔNG LỖI (UTF-8)
                start.StandardOutputEncoding = System.Text.Encoding.UTF8;
                start.StandardErrorEncoding = System.Text.Encoding.UTF8;

                using (Process process = Process.Start(start))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (output.Contains("Success:"))
                    {
                        string filePath = output.Replace("Success: ", "").Trim();
                        MessageBox.Show("Xuất báo cáo thành công rồi sếp ơi!", "Thành công");

                        // Mở file Excel vừa tạo
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    else
                    {
                        // Lúc này các thông báo lỗi hoặc "Không có dữ liệu" sẽ hiện tiếng Việt chuẩn
                        MessageBox.Show("Thông báo hệ thống: \n" + (string.IsNullOrEmpty(error) ? output : error));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        private void dtp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) LoadReport();
        }
    }
}