using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace DataGridNamespace
{
    public partial class EditMemberWindow : Window
    {
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string UserEmail { get; set; }

        private string userId;

        public EditMemberWindow(string userName, string userRole, string userEmail, string id)
        {
            InitializeComponent();

            // تعيين المعرف
            userId = id;

            // تعيين البيانات للخصائص المرتبطة بالـ DataBinding
            UserName = userName;
            UserRole = userRole;
            UserEmail = userEmail;

            // ضبط الـ DataContext حتى يتم ربط الحقول النصية بهذه الخصائص
            this.DataContext = this;

            // تعيين القيمة في ComboBox بناءً على الـ UserRole
            positionComboBox.SelectedItem = positionComboBox.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(item => ((ComboBoxItem)item).Content.ToString() == UserRole);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // سلسلة الاتصال وجملة التحديث
            string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=";
            string query = $"UPDATE users SET Nom = @Nom, Role = @Role, Email = @Email WHERE Id = @Id";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // أخذ القيم من الحقول النصية و ComboBox
                    cmd.Parameters.AddWithValue("@Nom", usernameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Role", (positionComboBox.SelectedItem as ComboBoxItem).Content.ToString()); // أخذ القيمة من ComboBox
                    cmd.Parameters.AddWithValue("@Email", emailTextBox.Text);
                    cmd.Parameters.AddWithValue("@Id", userId);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("User updated successfully.");
                this.Close();  // إغلاق نافذة التعديل
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}");
            }
        }
    }
}