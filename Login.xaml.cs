using DataGrid;
using DataGridNamespace;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyProject
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void GoToLayout2_Click(object sender, RoutedEventArgs e)
        {
            Layout1.Visibility = Visibility.Collapsed;
            Layout2.Visibility = Visibility.Visible;
        }

        private void GoToLayout1_Click(object sender, RoutedEventArgs e)
        {
            Layout2.Visibility = Visibility.Collapsed;
            Layout1.Visibility = Visibility.Visible;
        }

        // Layout1 Events
        private void textUser_L1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUser_L1.Focus();
        }

        private void txtUser_L1_TextChanged(object sender, TextChangedEventArgs e)
        {
            textUser_L1.Visibility = string.IsNullOrEmpty(txtUser_L1.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void textPassword_L1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword_L1.Focus();
        }

        private void txtPassword_L1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            textPassword_L1.Visibility = string.IsNullOrEmpty(txtPassword_L1.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // زر تسجيل الدخول في Layout1
        private void SignIn_L1_Click(object sender, RoutedEventArgs e)
        {
            // تعليق: التحقق من أن حقول اسم المستخدم وكلمة المرور ليست فارغة
            // Validate that username and password fields are not empty
            if (string.IsNullOrWhiteSpace(txtUser_L1.Text) || string.IsNullOrWhiteSpace(txtPassword_L1.Password))
            {
                MessageBox.Show("Please enter both Username and Password.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // تعليق: إيقاف التنفيذ إذا كانت المدخلات غير صالحة
                        // Stop execution if input is invalid
            }

            // تعليق: سلسلة الاتصال بقاعدة البيانات
            // Database connection string
            string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=;Allow User Variables=True;Use Affected Rows=False;";

            // تعليق: تعريف متغير الاتصال خارج كتلة try للسماح بالوصول إليه في finally
            // Define connection variable outside try block to allow access in finally
            MySqlConnection conn = null;

            try
            {
                // تعليق: إنشاء وفتح الاتصال بقاعدة البيانات
                // Create and open the database connection
                conn = new MySqlConnection(connectionString);
                conn.Open();
                Debug.WriteLine("Login attempt: DB Connection Opened.");

                // تعليق: استعلام SQL للتحقق من بيانات الاعتماد وجلب الدور والمعرف
                // SQL query to check credentials and fetch Role and ID
                // !!! تنبيه أمني خطير: هذا الاستعلام يتحقق من كلمة مرور نصية. يجب تغييره للتحقق من هاش كلمة المرور باستخدام bcrypt. !!!
                // !!! CRITICAL SECURITY WARNING: This query checks plaintext password. MUST be changed to verify password hash using bcrypt. !!!
                string loginQuery = "SELECT Role, id FROM users WHERE Nom=@Nom AND Password=@Password LIMIT 1";

                // تعليق: استخدام 'using' يضمن التخلص التلقائي من الأمر عند الخروج من الكتلة
                // Using 'using' ensures the command is automatically disposed when exiting the block
                using (MySqlCommand loginCmd = new MySqlCommand(loginQuery, conn))
                {
                    // تعليق: إضافة معاملات لمنع SQL Injection
                    // Add parameters to prevent SQL Injection
                    loginCmd.Parameters.AddWithValue("@Nom", txtUser_L1.Text);
                    loginCmd.Parameters.AddWithValue("@Password", txtPassword_L1.Password); // !!! خطر أمني !!!

                    // تعليق: استخدام 'using' للقارئ أيضًا للتخلص التلقائي
                    // Using 'using' for the reader as well for automatic disposal
                    using (MySqlDataReader reader = loginCmd.ExecuteReader())
                    {
                        // تعليق: التحقق مما إذا كان القارئ قد وجد صفًا مطابقًا (بيانات اعتماد صالحة)
                        // Check if the reader found a matching row (valid credentials)
                        if (reader.Read())
                        {
                            // --- Login Successful ---
                            // تعليق: قراءة الدور والمعرف من نتيجة الاستعلام
                            // Read Role and ID from the query result
                            string role = reader.IsDBNull(0) ? "Simpleuser" : reader.GetString(0); // تعليق: استخدام "Simpleuser" كدور افتراضي إذا كان فارغًا
                            int userId = reader.GetInt32(1); // تعليق: قراءة المعرف

                            reader.Close(); // تعليق: إغلاق القارئ بمجرد الانتهاء من القراءة

                            // تعليق: تخزين معلومات المستخدم في فئة الجلسة الثابتة
                            // Store user information in the static Session class
                            Session.CurrentUserId = userId;
                            Session.CurrentUserRole = role;
                            Debug.WriteLine($"Login Success: Session Set - UserID={Session.CurrentUserId}, Role={Session.CurrentUserRole}");

                            // تعليق: إظهار رسالة ترحيب للمستخدم
                            // Show a welcome message to the user
                            MessageBox.Show($"مرحبا بك! تم تسجيل الدخول بنجاح.");

                            // تعليق: إنشاء وعرض نافذة لوحة التحكم الرئيسية
                            // Create and show the main Dashboard window
                            DashboardView dashboard = new DashboardView();
                            dashboard.Show();

                            // تعليق: إغلاق نافذة تسجيل الدخول الحالية بعد نجاح تسجيل الدخول
                            // Close the current login window after successful login
                            this.Close();
                        }
                        else
                        {
                            // --- Login Failed ---
                            // تعليق: لم يتم العثور على مستخدم مطابق (اسم مستخدم أو كلمة مرور غير صحيحة)
                            // No matching user found (incorrect username or password)
                            reader.Close(); // تعليق: إغلاق القارئ
                            Debug.WriteLine($"Login Failed: Invalid credentials for username '{txtUser_L1.Text}'.");
                            MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.");
                        }
                    } // تعليق: يتم التخلص من القارئ هنا بواسطة using
                } // تعليق: يتم التخلص من الأمر هنا بواسطة using
            }
            catch (MySqlException mysqlEx) // تعليق: التقاط أخطاء MySQL المحددة
            {
                Debug.WriteLine($"MySQL Login Error: {mysqlEx.ToString()}");
                MessageBox.Show($"Database error during login. Please ensure the database server is running and accessible.\nDetails: {mysqlEx.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) // تعليق: التقاط أي أخطاء عامة أخرى
            {
                Debug.WriteLine($"General Login Error: {ex.ToString()}");
                MessageBox.Show($"An unexpected error occurred during login: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally // تعليق: كتلة finally تضمن إغلاق الاتصال دائمًا
            {
                // تعليق: إغلاق الاتصال إذا تم إنشاؤه وكان مفتوحًا
                // Close the connection if it was created and is open
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    try
                    {
                        conn.Close();
                        Debug.WriteLine("Login attempt: DB Connection Closed in finally.");
                    }
                    catch (Exception closeEx)
                    {
                        Debug.WriteLine($"Error closing connection: {closeEx.Message}"); // Log error during close
                    }
                }
            }
        } // End SignIn_L1_Click

    // ... (SignUp_L2_Click should also be modified later for hashing) ...
    // ... (Other methods like textUser_L1_MouseDown etc. remain if needed for UI hints) ...
 // End Class Login

        // Layout2 Events (للتسجيل)
        private void textUser_L2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtUser_L2.Focus();
        }

        private void txtUser_L2_TextChanged(object sender, TextChangedEventArgs e)
        {
            textUser_L2.Visibility = string.IsNullOrEmpty(txtUser_L2.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void textEmail_L2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail_L2.Focus();
        }

        private void txtEmail_L2_TextChanged(object sender, TextChangedEventArgs e)
        {
            textEmail_L2.Visibility = string.IsNullOrEmpty(txtEmail_L2.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void textPassword_L2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword_L2.Focus();
        }

        private void txtPassword_L2_PasswordChanged(object sender, RoutedEventArgs e)
        {
            textPassword_L2.Visibility = string.IsNullOrEmpty(txtPassword_L2.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SignUp_L2_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUser_L2.Text) || string.IsNullOrEmpty(txtEmail_L2.Text) || string.IsNullOrEmpty(txtPassword_L2.Password))
            {
                MessageBox.Show("Please fill Username, Email & Password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(txtEmail_L2.Text))
            {
                MessageBox.Show("Invalid Email!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var selectedItem = roleCombo_L2.SelectedItem as ComboBoxItem;
            string roleValue = (selectedItem != null) ? selectedItem.Content.ToString() : "Unknown";
            string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE Email = @Email";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail_L2.Text);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("Cet email est déjà utilisé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string insertQuery = "INSERT INTO users (Nom, Email, Password, Role) VALUES (@Nom, @Email, @Password, @Role)";
                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nom", txtUser_L2.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail_L2.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword_L2.Password);
                        cmd.Parameters.AddWithValue("@Role", roleValue);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show($"Inscription réussie en tant que {roleValue} !");
                Layout2.Visibility = Visibility.Collapsed;
                Layout1.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'inscription : {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
    }
}
