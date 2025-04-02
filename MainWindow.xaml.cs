using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace DataGridNamespace
{
    public partial class MainWindow : Page
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadMembers();  // تحميل بيانات الأعضاء عند بدء تحميل الصفحة
        }

        // دالة لتحميل بيانات الأعضاء من قاعدة البيانات
        private void LoadMembers()
        {
            string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=";
            string query = "SELECT Id, Nom, Role, Email FROM users";
            DataTable dataTable = new DataTable();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, conn);
                    dataAdapter.Fill(dataTable);
                }
                MembersDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}");
            }
        }

        // دوال التعامل مع الأزرار داخل الصفحة
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add new manager functionality goes here.");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    DataRowView dataRow = btn.DataContext as DataRowView;
                    if (dataRow == null)
                    {
                        MessageBox.Show("No row data found for editing.");
                        return;
                    }
                    string userId = dataRow["Id"].ToString();
                    string userName = dataRow["Nom"].ToString();
                    string userRole = dataRow["Role"].ToString();
                    string userEmail = dataRow["Email"].ToString();
                    EditMemberWindow editWindow = new EditMemberWindow(userName, userRole, userEmail, userId);
                    editWindow.ShowDialog();
                    LoadMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in EditButton_Click: " + ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    DataRowView dataRow = btn.DataContext as DataRowView;
                    if (dataRow == null)
                    {
                        MessageBox.Show("No row data found for deletion.");
                        return;
                    }
                    string userId = dataRow["Id"].ToString();
                    string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=";
                    string query = $"DELETE FROM users WHERE Id = {userId}";
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("User deleted successfully.");
                    LoadMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in DeleteButton_Click: " + ex.Message);
            }
        }
    }
}
