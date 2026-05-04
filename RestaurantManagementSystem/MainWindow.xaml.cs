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
        // Chuyển sang Lazy Initialization để tiết kiệm RAM và tăng tốc khởi động
        private ucWaiter _ucWaiter;
        private ucCashier _ucCashier;
        private BitmapImage _preloadedChefGif;

        public MainWindow()
        {
            // Tối ưu hóa render mức độ thấp ngay lập tức
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);

            InitializeComponent();

            // Ưu tiên hiện lời chào ngay (vì nó là text tĩnh, cực nhẹ)
            SetDynamicGreeting();

            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Bước 1: Hiện ngay bộ khung và dashboard để người dùng không phải chờ
            PhanQuyen();
            LoadUserUI();
            ShowHomePage();

            // Giải phóng UI để Window render mượt mà
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            // Bước 2: Nạp GIF bất đồng bộ (Task.Run giúp không bị khựng UI)
            _ = Task.Run(() => PreloadResources()).ContinueWith(t =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_preloadedChefGif != null)
                    {
                        WpfAnimatedGif.ImageBehavior.SetAnimatedSource(imgChef, _preloadedChefGif);
                    }
                    // Trả lại chất lượng ảnh cao sau khi mọi thứ đã ổn định
                    RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
                }), DispatcherPriority.Background);
            });

            // Bước 3: Nạp ngầm các UserControl khi hệ thống thực sự RẢNH (Idle)
            // Việc này giúp lúc bấm nút chuyển trang sẽ không bị delay
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_ucWaiter == null) _ucWaiter = new ucWaiter();
                if (_ucCashier == null) _ucCashier = new ucCashier();
            }), DispatcherPriority.SystemIdle);
        }

        private void ShowHomePage()
        {
            // Chỉ cập nhật nếu thực sự cần thiết để tránh re-layout tốn sức máy
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
                // OnLoad giúp giải phóng file ngay sau khi nạp vào RAM, cực nhanh
                _preloadedChefGif.CacheOption = BitmapCacheOption.OnLoad;
                _preloadedChefGif.EndInit();
                _preloadedChefGif.Freeze();
            }
            catch { }
        }

        // Tối ưu hàm chuyển trang để dùng chung, code sẽ sạch hơn
        private void NavigateTo(UIElement content, string title, string icon, string breadcrumb)
        {
            if (WelcomePanel != null) WelcomePanel.Visibility = Visibility.Collapsed;
            MainContent.Content = content;
            UpdateHeader(title, icon, breadcrumb);
        }

        private void btnPhucVu_Click(object sender, RoutedEventArgs e)
        {
            if (_ucWaiter == null) _ucWaiter = new ucWaiter(); // Double check
            NavigateTo(_ucWaiter, "Phục Vụ Món", "🍽️", "Trang chủ / Phục vụ");
        }

        private void btnThuNgan_Click(object sender, RoutedEventArgs e)
        {
            if (_ucCashier == null) _ucCashier = new ucCashier(); // Double check
            NavigateTo(_ucCashier, "Thu Ngân", "💳", "Trang chủ / Thu ngân");
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            // Trang Admin thường ít dùng, nạp trực tiếp để tiết kiệm RAM ban đầu
            NavigateTo(new ucAdmin(), "Quản Trị Hệ Thống", "⚙️", "Trang chủ / Hệ thống");
        }

        private void btnHome_Click(object sender, RoutedEventArgs e) => ShowHomePage();

        private void UpdateHeader(string title, string icon, string breadcrumb)
        {
            // Chỉ gán text khi giá trị thay đổi để tránh CPU render lại textblock
            if (txtCurrentTab.Text != title) txtCurrentTab.Text = title;
            if (txtCurrentIcon.Text != icon) txtCurrentIcon.Text = icon;
            if (txtBreadcrumb.Text != breadcrumb) txtBreadcrumb.Text = breadcrumb;
        }

        private void SetDynamicGreeting()
        {
            int hour = DateTime.Now.Hour;
            txtGreeting.Text = (hour < 12) ? "Chào buổi sáng," :
                               (hour < 18) ? "Chào buổi chiều," : "Chào buổi tối,";
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

        private void btnDoiMatKhau_Click(object sender, RoutedEventArgs e) => new ChangePasswordWindow { Owner = this }.Show();

        // Khi bấm nút Đăng xuất ở Sidebar
        private void btnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog.Visibility = Visibility.Visible;
        }

        // Khi bấm nút "Hủy" hoặc click ra ngoài
        private void btnCancelLogout_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog.Visibility = Visibility.Collapsed;
        }

        // Khi bấm nút "Đăng xuất" trong khung thông báo
        private void btnConfirmLogout_Click(object sender, RoutedEventArgs e)
        {
            AccountDAL.LoginAccount = null;
            this.Close();
        }
    }
}