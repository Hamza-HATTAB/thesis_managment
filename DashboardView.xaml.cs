// --- File: DashboardView.xaml.cs ---
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridNamespace; // For Session, Views, MainWindow, Logout etc. (Adjust if needed)
 using MyProject; // Only if Login/Profile are truly in a different project/namespace
using System.Diagnostics;
using System; // For StringComparison

namespace DataGrid // Or your actual namespace for DashboardView
{
    public partial class DashboardView : Window
    {
        public DashboardView()
        {
            InitializeComponent();
            // تعليق: التحقق من تسجيل الدخول عند الفتح
            // Check login on opening
            if (Session.CurrentUserId == -1)
            {
                Debug.WriteLine("CRITICAL: DashboardView loaded but Session.CurrentUserId is -1. Redirecting.");
                MessageBox.Show("Session error. Please log in again.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                // تعليق: افتراض أن Login موجود في DataGridNamespace الآن
                // Assuming Login is in DataGridNamespace now
                Login loginView = new Login();
                loginView.Show();
                this.Close();
                return;
            }
            Debug.WriteLine($"DashboardView Initialized for UserID: {Session.CurrentUserId}, Role: {Session.CurrentUserRole}");
            // تعليق: تحميل الواجهة الافتراضية
            // Load default view
            NavigateToThesis(ThesisButton);
        }

        // --- Window Dragging ---
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { try { DragMove(); } catch (InvalidOperationException) { /* Ignore */ } }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) try { DragMove(); } catch (InvalidOperationException) { /* Ignore */ } }

        // --- Navigation Helpers ---
        private void UpdateButtonBackgrounds(Button activeButton)
        {
            // تعليق: إعادة تعيين كافة الأزرار وتعيين الزر النشط
            // Reset all buttons and set the active one
            var activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7B5CD6"));
            Button[] navButtons = { ThesisButton, MembersButton, ProfileButton, FavoritesButton, DashboardButton }; // Include DashboardButton if it exists
            foreach (var btn in navButtons)
            {
                if (btn != null) btn.Background = Brushes.Transparent;
            }
            if (activeButton != null) activeButton.Background = activeBrush;
        }

        private void NavigateFrame(object pageInstance)
        {
            // تعليق: التحقق من Frame والمحتوى قبل التنقل
            // Check Frame and content before navigating
            if (MainFrame == null) { Debug.WriteLine("ERROR: MainFrame is null!"); return; }
            if (pageInstance == null) { Debug.WriteLine("ERROR: pageInstance is null!"); return; }

            if (MainFrame.Content?.GetType() == pageInstance.GetType())
            {
                Debug.WriteLine($"Navigation skipped: Already displaying {pageInstance.GetType().Name}");
                return;
            }
            Debug.WriteLine($"Navigating Frame to {pageInstance.GetType().Name}");
            MainFrame.Navigate(pageInstance);
            // تعليق: تنظيف سجل التنقل
            // Clear navigation history
            while (MainFrame.NavigationService.CanGoBack) { MainFrame.NavigationService.RemoveBackEntry(); }
        }

        // --- Sidebar Button Click Handlers ---
        private void ThesisButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ThesisButton_Click executing...");
            NavigateToThesis(sender as Button);
        }
        private void NavigateToThesis(Button senderButton)
        {
            UpdateButtonBackgrounds(senderButton ?? ThesisButton);
            NavigateFrame(new ThesisView()); // تعليق: افتراض ThesisView في DataGridNamespace
        }

        private void MembersButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MembersButton_Click executing...");
            if (Session.CurrentUserRole?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                UpdateButtonBackgrounds(sender as Button);
                NavigateFrame(new MainWindow()); // تعليق: افتراض MainWindow في DataGridNamespace
            }
            else
            {
                Debug.WriteLine($"Access Denied for Members. Role: '{Session.CurrentUserRole}'");
                MessageBox.Show("Access Denied: Administrators only.", "Permission Error");
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ProfileButton_Click executing...");
            if (Session.CurrentUserId != -1)
            {
                UpdateButtonBackgrounds(sender as Button);
                NavigateFrame(new ProfileView()); // تعليق: افتراض ProfileView في DataGridNamespace
            }
            else { MessageBox.Show("Please log in.", "Login Required"); }
        }

        private void FavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("FavoritesButton_Click executing...");
            if (Session.CurrentUserId != -1)
            {
                UpdateButtonBackgrounds(sender as Button);
                NavigateFrame(new FavoritesView()); // تعليق: افتراض FavoritesView في DataGridNamespace
            }
            else { MessageBox.Show("Please log in.", "Login Required"); }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DashboardButton_Click executing (Redirecting to Thesis)...");
            NavigateToThesis(sender as Button);
        }

        // --- Logout & Window Controls ---
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LogoutButton_Click executing...");
            LogoutConfirmationWindow confirmWindow = new LogoutConfirmationWindow();
            if (confirmWindow.ShowDialog() == true && confirmWindow.IsConfirmed)
            {
                Session.CurrentUserId = -1;
                Session.CurrentUserRole = string.Empty;
                Debug.WriteLine("Session Cleared.");
                // تعليق: افتراض Login في DataGridNamespace
                // Assuming Login is in DataGridNamespace
                Login loginWindow = new Login();
                loginWindow.Show();
                this.Close();
            }
            else { Debug.WriteLine("Logout Cancelled."); }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) { this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }
    }
}