// --- File: ThesisView.xaml.cs ---

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataGridNamespace; // تعليق: للوصول إلى Session

// تعليق: تأكد من تطابق مساحة الاسم مع x:Class في XAML
// Ensure this namespace matches the x:Class in the XAML
namespace DataGridNamespace
{
    public partial class ThesisView : UserControl
    {
        private ObservableCollection<Thesis> AllTheses;
        private ICollectionView ThesesView;
        private string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=;Allow User Variables=True;Use Affected Rows=False;";

        public ThesisView()
        {
            InitializeComponent(); // تعليق: تهيئة XAML
            AllTheses = new ObservableCollection<Thesis>();
            ThesesView = CollectionViewSource.GetDefaultView(AllTheses);
            ThesesView.Filter = FilterTheses;
            // تعليق: التحقق من Null لـ DataGrid
            // Null check for DataGrid
            if (ThesesDataGrid != null)
            {
                ThesesDataGrid.ItemsSource = ThesesView;
            }
            else { Debug.WriteLine("ERROR: ThesesDataGrid is null after InitializeComponent."); }
            this.Loaded += View_Loaded;
        }

        private async void View_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AllTheses.Any()) { await LoadDataAsync(); }
        }

        // --- Database Access Methods ---
        private async Task LoadDataAsync()
        {
            // تعليق: التحقق من Null
            // Null checks
            if (LoadingIndicator == null || ThesesDataGrid == null) { Debug.WriteLine("ERROR: UI elements null in LoadDataAsync (ThesisView)."); return; }

            LoadingIndicator.Visibility = Visibility.Visible;
            ThesesDataGrid.Visibility = Visibility.Collapsed;
            AllTheses.Clear();

            int currentUserId = Session.CurrentUserId;
            HashSet<int> favoriteIds = new HashSet<int>();

            if (currentUserId != -1) { favoriteIds = await GetFavoriteThesisIdsAsync(currentUserId); }
            else { Debug.WriteLine("ThesisView LoadData: User not logged in."); }

            // تعليق: استخدام using لإدارة الموارد
            // Use using for resource management
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                MySqlDataReader reader = null;
                try
                {
                    await conn.OpenAsync();
                    Debug.WriteLine("DB Connection Opened (LoadAllTheses)");
                    cmd.CommandText = "SELECT id, titre, auteur, speciality, Type, mots_cles, annee, Resume, fichier, user_id FROM theses ORDER BY annee DESC, titre ASC";

                    // --- FIX: Explicit Cast ---
                    reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    // --- END FIX ---

                    if (reader.HasRows)
                    {
                        int idOrdinal = reader.GetOrdinal("id");
                        int titreOrdinal = reader.GetOrdinal("titre");
                        int auteurOrdinal = reader.GetOrdinal("auteur");
                        int specialityOrdinal = reader.GetOrdinal("speciality");
                        int typeOrdinal = reader.GetOrdinal("Type");
                        int motsClesOrdinal = reader.GetOrdinal("mots_cles");
                        int anneeOrdinal = reader.GetOrdinal("annee");
                        int resumeOrdinal = reader.GetOrdinal("Resume");
                        int fichierOrdinal = reader.GetOrdinal("fichier");
                        int userIdOrdinal = reader.GetOrdinal("user_id");

                        while (await reader.ReadAsync())
                        {
                            int thesisId = reader.GetInt32(idOrdinal);
                            // تعليق: استخدام الخصائص الصحيحة من Thesis.cs
                            // Use correct properties from Thesis.cs
                            var thesis = new Thesis
                            {
                                Id = thesisId,         // Use Id
                                Title = reader.IsDBNull(titreOrdinal) ? "" : reader.GetString(titreOrdinal),
                                Author = reader.IsDBNull(auteurOrdinal) ? "" : reader.GetString(auteurOrdinal),
                                Specialty = reader.IsDBNull(specialityOrdinal) ? "" : reader.GetString(specialityOrdinal),
                                Type = reader.IsDBNull(typeOrdinal) ? "" : reader.GetString(typeOrdinal),
                                Keyword = reader.IsDBNull(motsClesOrdinal) ? "" : reader.GetString(motsClesOrdinal),
                                Year = reader.IsDBNull(anneeOrdinal) ? DateTime.MinValue : reader.GetDateTime(anneeOrdinal), // DateTime
                                Resume = reader.IsDBNull(resumeOrdinal) ? "" : reader.GetString(resumeOrdinal),       // Use Resume
                                PdfLink = reader.IsDBNull(fichierOrdinal) ? "" : reader.GetString(fichierOrdinal),
                                UserId = reader.IsDBNull(userIdOrdinal) ? -1 : reader.GetInt32(userIdOrdinal),         // Use UserId
                                IsFavorite = favoriteIds.Contains(thesisId)                                           // Use IsFavorite
                            };
                            AllTheses.Add(thesis);
                        }
                        await reader.DisposeAsync(); // Dispose reader here
                    }
                    else { Debug.WriteLine("No theses found in DB."); }
                }
                catch (Exception ex) { Debug.WriteLine($"Error loading theses: {ex}"); MessageBox.Show($"Error: {ex.Message}"); }
                finally
                {
                    if (reader != null && !reader.IsClosed) { await reader.DisposeAsync(); } // FIX: Check IsClosed
                    Debug.WriteLine("DB Resources Disposed (LoadAllTheses)");
                    // تعليق: التحقق من Null
                    // Null checks
                    if (LoadingIndicator != null) LoadingIndicator.Visibility = Visibility.Collapsed;
                    if (ThesesDataGrid != null) ThesesDataGrid.Visibility = Visibility.Visible;
                }
            } // End using conn, cmd
        }

        // تعليق: جلب معرفات المفضلة
        // Get favorite IDs
        private async Task<HashSet<int>> GetFavoriteThesisIdsAsync(int userId)
        {
            var favoriteIds = new HashSet<int>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("SELECT these_id FROM favoris WHERE user_id = @userId", conn))
            {
                MySqlDataReader reader = null;
                try
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    await conn.OpenAsync();
                    // --- FIX: Explicit Cast ---
                    reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    // --- END FIX ---
                    int idOrdinal = reader.GetOrdinal("these_id");
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(idOrdinal)) favoriteIds.Add(reader.GetInt32(idOrdinal));
                    }
                    await reader.DisposeAsync(); // Dispose reader here
                }
                catch (Exception ex) { Debug.WriteLine($"Error fetching favorite IDs: {ex.ToString()}"); }
                finally { if (reader != null && !reader.IsClosed) { await reader.DisposeAsync(); } } // FIX: Check IsClosed
            } // using handles disposal
            return favoriteIds;
        }

        // تعليق: إضافة مفضلة
        // Add favorite
        private async Task<bool> AddFavoriteDbAsync(int userId, int thesisId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("INSERT IGNORE INTO favoris (user_id, these_id) VALUES (@userId, @thesisId)", conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@thesisId", thesisId); // Use Id
                    await conn.OpenAsync();
                    int result = await cmd.ExecuteNonQueryAsync();
                    return true; // Success if no exception
                }
                catch (Exception ex) { Debug.WriteLine($"Error adding fav DB: {ex.ToString()}"); return false; }
            } // using handles disposal
        }

        // تعليق: إزالة مفضلة
        // Remove favorite
        private async Task<bool> RemoveFavoriteDbAsync(int userId, int thesisId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("DELETE FROM favoris WHERE user_id = @userId AND these_id = @thesisId", conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@thesisId", thesisId); // Use Id
                    await conn.OpenAsync();
                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0; // Success if rows affected
                }
                catch (Exception ex) { Debug.WriteLine($"Error removing fav DB: {ex.ToString()}"); return false; }
            } // using handles disposal
        }

        // --- UI Event Handlers ---
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) { ThesesView?.Refresh(); }
        private bool FilterTheses(object item)
        {
            if (!(item is Thesis thesis)) return false;
            // تعليق: التحقق من Null لـ SearchTextBox
            // Null check for SearchTextBox
            if (SearchTextBox == null) return true;
            string searchText = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchText) || searchText.Equals(SearchTextBox.Tag?.ToString(), StringComparison.OrdinalIgnoreCase)) return true;
            searchText = searchText.ToLowerInvariant();
            // تعليق: استخدام الخصائص الصحيحة
            // Use correct properties
            return (thesis.Title?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   (thesis.Author?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   (thesis.Keyword?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   (thesis.Specialty?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   (thesis.Type?.ToLowerInvariant().Contains(searchText) ?? false) ||
                   thesis.DisplayYear.ToString().Contains(searchText); // Use DisplayYear
        }
        private void ViewPdfButton_Click(object sender, RoutedEventArgs e)
        { /* ... (Same PDF logic) ... */
            if (sender is Button button && button.CommandParameter is string pdfLink && !string.IsNullOrEmpty(pdfLink))
            {
                try { Process.Start(new ProcessStartInfo(pdfLink) { UseShellExecute = true }); }
                catch (Exception ex) { MessageBox.Show($"Could not open PDF link: {ex.Message}", "Error"); Debug.WriteLine($"PDF Error: {ex}"); }
            }
            else { MessageBox.Show("Invalid PDF link.", "Warning"); }
        }
        private async void ToggleFavoriteButton_Click(object sender, RoutedEventArgs e)
        { /* ... (Same Toggle logic, uses Session and DB helpers) ... */
            if (!(sender is Button button && button.CommandParameter is Thesis thesis)) return;
            int currentUserId = Session.CurrentUserId;
            if (currentUserId == -1) { MessageBox.Show("Please log in.", "Login Required"); return; }
            bool addingFavorite = !thesis.IsFavorite; // Use IsFavorite
            bool successDb;
            button.IsEnabled = false;
            Debug.WriteLine($"Toggling favorite for Thesis {thesis.Id} (Adding: {addingFavorite}) for User {currentUserId}"); // Use Id
            if (addingFavorite) { successDb = await AddFavoriteDbAsync(currentUserId, thesis.Id); } // Use Id
            else { successDb = await RemoveFavoriteDbAsync(currentUserId, thesis.Id); } // Use Id
            if (successDb)
            {
                thesis.IsFavorite = addingFavorite; // Use IsFavorite
                ThesesView.Refresh();
                Debug.WriteLine($"UI State updated for Thesis {thesis.Id}"); // Use Id
            }
            else { Debug.WriteLine($"DB operation failed for Thesis {thesis.Id}"); } // Use Id
            button.IsEnabled = true;
        } // End ToggleFavorite
    } // End Class
} // End Namespace