using RestaurantManagementSystem.DAL;
using RestaurantManagementSystem.Views;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace RestaurantManagementSystem
{
    public partial class MainWindow : Window
    {
        private ucWaiter _ucWaiter;
        private ucCashier _ucCashier;
        private BitmapImage _preloadedChefGif;
        private DispatcherTimer _clockTimer;
        private int _currentAccountRole = -1; 

        public MainWindow()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void StartRealTimeClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                // Kiểm tra an toàn trước khi xử lý thời gian thực
                if (AccountDAL.LoginAccount == null) return;

                DateTime now = DateTime.Now;
                txtDateDisplay.Text = $"Hôm nay, {now.ToString("dd/MM/yyyy")}";
                SetDynamicGreeting(now.Hour);
            };
            _clockTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Trích xuất quyền tài khoản ra biến tạm ngay từ khi ứng dụng tải xong
            if (AccountDAL.LoginAccount != null)
            {
                _currentAccountRole = Convert.ToInt32(AccountDAL.LoginAccount["Role"]);
            }

            PhanQuyen();
            LoadUserUI();
            ShowHomePage();
            StartRealTimeClock(); // Khởi chạy đồng hồ sau khi dữ liệu phân quyền sẵn sàng

            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            _ = Task.Run(() => PreloadResources()).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_preloadedChefGif != null)
                    {
                        WpfAnimatedGif.ImageBehavior.SetAnimatedSource(imgChef, _preloadedChefGif);
                    }
                    RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
                }), DispatcherPriority.Background);
            });

            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_ucWaiter == null) _ucWaiter = new ucWaiter();
                if (_ucCashier == null) _ucCashier = new ucCashier();
            }), DispatcherPriority.SystemIdle);
        }

        private void ShowHomePage()
        {
            if (MainContent.Content != null) MainContent.Content = null;
            if (WelcomePanel != null && WelcomePanel.Visibility != Visibility.Visible)
                WelcomePanel.Visibility = Visibility.Visible;

            UpdateHeader("Dashboard", "🏠", "Trang chủ / Bảng điều khiển");
        }

        private void PreloadResources()
        {
            try
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri("pack://application:,,,/chef_animated.gif");
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();
                _preloadedChefGif = img;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi tải hoạt ảnh: " + ex.Message);
            }
        }

        private void NavigateTo(UIElement content, string title, string icon, string breadcrumb)
        {
            if (WelcomePanel != null) WelcomePanel.Visibility = Visibility.Collapsed;
            MainContent.Content = content;
            UpdateHeader(title, icon, breadcrumb);
        }

        private void btnPhucVu_Click(object sender, RoutedEventArgs e)
        {
            if (_ucWaiter == null) _ucWaiter = new ucWaiter();
            NavigateTo(_ucWaiter, "Phục Vụ Món", "🍽️", "Trang chủ / Phục vụ");
        }

        private void btnThuNgan_Click(object sender, RoutedEventArgs e)
        {
            if (_ucCashier == null) _ucCashier = new ucCashier();
            NavigateTo(_ucCashier, "Thu Ngân", "💳", "Trang chủ / Thu ngân");
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new ucAdmin(), "Quản Trị Hệ Thống", "⚙️", "Trang chủ / Hệ thống");
        }

        private void btnHome_Click(object sender, RoutedEventArgs e) => ShowHomePage();

        private void UpdateHeader(string title, string icon, string breadcrumb)
        {
            if (txtCurrentTab.Text != title) txtCurrentTab.Text = title;
            if (txtCurrentIcon.Text != icon) txtCurrentIcon.Text = icon;
            if (txtBreadcrumb.Text != breadcrumb) txtBreadcrumb.Text = breadcrumb;
        }

        private void SetDynamicGreeting(int hour)
        {
            string greeting = (hour >= 5 && hour < 12) ? "☕ Chào buổi sáng," :
                              (hour >= 12 && hour < 18) ? "🌤️ Chào buổi chiều," : "🌙 Chào buổi tối,";

            if (_currentAccountRole == -1)
            {
                greeting = "👋 Chào mừng cộng tác viên mới,";
            }

            if (txtGreeting.Text != greeting) txtGreeting.Text = greeting;
        }

        public static bool CanStaffEditFood { get; set; } = true;
        public static bool CanStaffDeleteFood { get; set; } = true;
        public static bool CanStaffSendFood { get; set; } = true;

        public static bool CanCashierPay { get; set; } = true;
        public static bool CanCashierPrint { get; set; } = true;

        private void PhanQuyen()
        {
            spAdminSection.Visibility = (_currentAccountRole == 0) ? Visibility.Visible : Visibility.Collapsed;

            btnPhucVu.Visibility = (_currentAccountRole == 0 || _currentAccountRole == 1 || (_currentAccountRole >= 11 && _currentAccountRole <= 15))
                                   ? Visibility.Visible : Visibility.Collapsed;

            btnThuNgan.Visibility = (_currentAccountRole == 0 || _currentAccountRole == 2 || (_currentAccountRole >= 21 && _currentAccountRole <= 23))
                                   ? Visibility.Visible : Visibility.Collapsed;

            CanStaffEditFood = true;
            CanStaffDeleteFood = true;
            CanStaffSendFood = true;

            CanCashierPay = true;
            CanCashierPrint = true;

            //Quyền của phục vụ
            if (_currentAccountRole == 11) CanStaffEditFood = false;
            else if (_currentAccountRole == 12) CanStaffDeleteFood = false;
            else if (_currentAccountRole == 13) CanStaffSendFood = false;
            else if (_currentAccountRole == 14) { CanStaffEditFood = false; CanStaffDeleteFood = false; }
            else if (_currentAccountRole == 15) { CanStaffEditFood = false; CanStaffDeleteFood = false; CanStaffSendFood = false; }

            //Quyền của thu ngân
            if (_currentAccountRole == 21)
            {
                CanCashierPay = false;   
                CanCashierPrint = true;  // Vẫn cho phép in hóa đơn tạm (Nút in sáng)
            }
            else if (_currentAccountRole == 22)
            {
                CanCashierPay = true;    // Vẫn cho phép thanh toán (Nút thanh toán sáng)
                CanCashierPrint = false; 
            }
            else if (_currentAccountRole == 23)
            {
                CanCashierPay = false;   // Khóa cả hai tính năng
                CanCashierPrint = false;
            }
            else
            {
                // Trường hợp _currentAccountRole == 2 (Thu ngân chuẩn full quyền) hoặc Admin (0)
                CanCashierPay = true;
                CanCashierPrint = true;
            }

        }

        public void LoadUserUI()
        {
            if (AccountDAL.LoginAccount == null) return;

            string name = AccountDAL.LoginAccount["DisplayName"]?.ToString() ?? "USER";
            txtUserDisplayName.Text = name.ToUpper();
            txtAvatarInitial.Text = !string.IsNullOrWhiteSpace(name) ? name[0].ToString().ToUpper() : "H";

            if (_currentAccountRole == -1)
            {
                txtWelcomeTitle.Text = "Tài khoản đang chờ xác thực";
                txtWelcomeTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7675")); 

                txtWelcomeQuote.Text = "Tài khoản của bạn chưa được cấp quyền truy cập hệ thống. Vui lòng liên hệ Admin.";
                txtWelcomeQuote.FontStyle = FontStyles.Normal;

                // Vô hiệu hóa các nút chức năng 
                btnPhucVu.IsEnabled = false;
                btnThuNgan.IsEnabled = false;
            }
            else
            {
                txtWelcomeTitle.Text = "Chào mừng bạn quay trở lại";
                txtWelcomeTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3436"));

                txtWelcomeQuote.Text = "“Nấu ăn là nghệ thuật, quản lý là một khoa học.”";
                txtWelcomeQuote.FontStyle = FontStyles.Italic;
            }

            txtRoleBadge.Text = (_currentAccountRole == 0) ? "👑 Admin" :
                    (_currentAccountRole == 1 || (_currentAccountRole >= 11 && _currentAccountRole <= 15)) ? "👤 Staff" :
                    (_currentAccountRole == 2 || (_currentAccountRole >= 21 && _currentAccountRole <= 23)) ? "💵 Cashier" : "⌛ Chờ duyệt";
        }

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow wd = new ChangePasswordWindow();
            wd.Owner = this;
            wd.ShowDialog();
        }

        private void btnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog.Visibility = Visibility.Visible;
        }

        private void btnCancelLogout_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog.Visibility = Visibility.Collapsed;
        }

        private void btnConfirmLogout_Click(object sender, RoutedEventArgs e)
        {
            if (_clockTimer != null) _clockTimer.Stop();

            AccountDAL.LoginAccount = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }
        private bool isMenuCollapsed = false;

        private void btnToggleMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!isMenuCollapsed)
            {
                NavColumn.Width = new GridLength(70);

                UserInfoSection.Visibility = Visibility.Collapsed;

                lblMainMenu.Visibility = Visibility.Collapsed;
                lblUserSettings.Visibility = Visibility.Collapsed;

                txtHome.Visibility = Visibility.Collapsed;
                txtPhucVu.Visibility = Visibility.Collapsed;
                txtThuNgan.Visibility = Visibility.Collapsed;
                txtDoiMatKhau.Visibility = Visibility.Collapsed;
                txtDangXuat.Visibility = Visibility.Collapsed;

                btnToggleMenu.HorizontalAlignment = HorizontalAlignment.Center;
                txtToggleIcon.Text = "☰";
                txtToggleIcon.FontSize = 18;
                btnToggleMenu.Margin = new Thickness(0, 12, 0, 0);

                this.Tag = "Collapsed";
                isMenuCollapsed = true;
            }
            else
            {
                NavColumn.Width = new GridLength(240);

                UserInfoSection.Visibility = Visibility.Visible;
                lblMainMenu.Visibility = Visibility.Visible;
                lblUserSettings.Visibility = Visibility.Visible;

                txtHome.Visibility = Visibility.Visible;
                txtPhucVu.Visibility = Visibility.Visible;
                txtThuNgan.Visibility = Visibility.Visible;
                txtDoiMatKhau.Visibility = Visibility.Visible;
                txtDangXuat.Visibility = Visibility.Visible;

                txtToggleIcon.Text = "◧";
                txtToggleIcon.FontSize = 15;
                btnToggleMenu.HorizontalAlignment = HorizontalAlignment.Right;
                btnToggleMenu.Margin = new Thickness(0, 12, 15, 0);

                this.Tag = "Expanded";
                isMenuCollapsed = false;
            }
        }
    }
}