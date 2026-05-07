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

        public MainWindow()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);

            InitializeComponent();

            StartRealTimeClock();

            this.Loaded += MainWindow_Loaded;
        }

        private void StartRealTimeClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                DateTime now = DateTime.Now;

                txtDateDisplay.Text = $"Hôm nay, {now.ToString("dd/MM/yyyy")}";

                SetDynamicGreeting(now.Hour);
            };
            _clockTimer.Start();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PhanQuyen();
            LoadUserUI();
            ShowHomePage();

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
                _preloadedChefGif = new BitmapImage();
                _preloadedChefGif.BeginInit();
                _preloadedChefGif.UriSource = new Uri("pack://application:,,,/chef_animated.gif");
                _preloadedChefGif.CacheOption = BitmapCacheOption.OnLoad;
                _preloadedChefGif.EndInit();
                _preloadedChefGif.Freeze();
            }
            catch { }
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

            if (txtGreeting.Text != greeting) txtGreeting.Text = greeting;
        }

        private void PhanQuyen()
        {
            if (AccountDAL.LoginAccount == null) return;
            int role = (int)AccountDAL.LoginAccount["Role"];
            spAdminSection.Visibility = (role == 0) ? Visibility.Visible : Visibility.Collapsed;
            btnPhucVu.Visibility = (role <= 1) ? Visibility.Visible : Visibility.Collapsed;
            btnThuNgan.Visibility = (role == 0 || role == 2) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void LoadUserUI()
        {
            if (AccountDAL.LoginAccount == null) return;
            string name = AccountDAL.LoginAccount["DisplayName"]?.ToString() ?? "USER";
            txtUserDisplayName.Text = name.ToUpper();
            txtAvatarInitial.Text = !string.IsNullOrEmpty(name) ? name[0].ToString().ToUpper() : "H";
            int type = (int)AccountDAL.LoginAccount["Role"];
            txtRoleBadge.Text = (type == 0) ? "👑 Admin" : (type == 1) ? "👤 Staff" : "💵 Cashier";
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
    }
}