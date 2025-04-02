// --- File: FavoritesView.xaml.cs ---

using MySql.Data.MySqlClient;   // For MySQL interaction
using System;
using System.Collections.ObjectModel; // For ObservableCollection
using System.Diagnostics;           // For Process.Start and Debug
using System.Threading.Tasks;       // For async/await Task
using System.Windows;               // For UI elements like MessageBox, Visibility
using System.Windows.Controls;      // For UserControl, Button, DataGrid
using System.Linq;                  // For LINQ Any() method
using DataGridNamespace;            // For Session class

// تعليق: تأكد من تطابق مساحة الاسم مع x:Class في XAML وملفك التنظيمي
// Ensure this namespace matches the x:Class in the XAML and your project structure
namespace DataGridNamespace
{
    public partial class FavoritesView : UserControl
    {
        private ObservableCollection<Thesis> FavoriteTheses;
        private string connectionString = "Server=localhost;Database=gestion_theses;User ID=root;Password=;Allow User Variables=True;Use Affected Rows=False;";

        public FavoritesView()
        {
            InitializeComponent(); // تعليق: تهيئة عناصر XAML المسماة
                                   // Initialize named XAML elements
            FavoriteTheses = new ObservableCollection<Thesis>();
            // تعليق: التحقق من أن FavoritesDataGrid ليس null قبل تعيين ItemsSource (يجب أن يكون موجودًا بعد InitializeComponent)
            // Check that FavoritesDataGrid is not null before setting ItemsSource (should exist after InitializeComponent)
            if (FavoritesDataGrid != null)
            {
                FavoritesDataGrid.ItemsSource = FavoriteTheses;
            }
            else
            {
                Debug.WriteLine("ERROR: FavoritesDataGrid is null after InitializeComponent in FavoritesView.");
                // This indicates a serious problem, likely XAML parsing failed despite fixes
            }
            this.Loaded += View_Loaded;
        }

        private async void View_Loaded(object sender, RoutedEventArgs e)
        {
            // تعليق: استدعاء تحميل البيانات
            // Call data loading
            await LoadDataAsync();
        }

        // --- Database Access Methods ---
        private async Task LoadDataAsync()
        {
            // تعليق: التحقق من وجود عناصر الواجهة قبل استخدامها
            // Check UI elements exist before using them
            if (LoadingIndicator == null || NoFavoritesMessage == null || FavoritesDataGrid == null)
            {
                Debug.WriteLine("ERROR: UI elements null in LoadDataAsync (FavoritesView).");
                return; // Cannot proceed if essential UI is missing
            }

            LoadingIndicator.Visibility = Visibility.Visible;
            NoFavoritesMessage.Visibility = Visibility.Collapsed;
            FavoritesDataGrid.Visibility = Visibility.Collapsed;
            FavoriteTheses.Clear();

            int currentUserId = Session.CurrentUserId;
            if (currentUserId == -1)
            {
                MessageBox.Show("Please log in.", "Login Required");
                LoadingIndicator.Visibility = Visibility.Collapsed;
                NoFavoritesMessage.Visibility = Visibility.Visible;
                return;
            }

            // تعليق: استخدام using لإدارة الموارد
            // Use using for resource management
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                MySqlDataReader reader = null; // تعليق: تعريف خارج try/using لـ reader
                try
                {
                    await conn.OpenAsync();
                    Debug.WriteLine("DB Connection Opened (LoadFavorites)");
                    cmd.CommandText = @"SELECT t.id, t.titre, t.auteur, t.speciality, t.Type, t.mots_cles, t.annee, t.Resume, t.fichier, t.user_id FROM theses t INNER JOIN favoris f ON t.id = f.these_id WHERE f.user_id = @userId ORDER BY t.annee DESC, t.titre ASC";
                    cmd.Parameters.AddWithValue("@userId", currentUserId);

                    // تعليق: تحويل صريح إلى MySqlDataReader
                    // Explicit cast to MySqlDataReader
                    reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        // تعليق: الحصول على الفهارس
                        // Get ordinals
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

                        // تعليق: قراءة وتعبئة المجموعة
                        // Read and populate collection
                        while (await reader.ReadAsync())
                        {
                            // تعليق: استخدام الخصائص الصحيحة من Thesis.cs
                            // Use correct properties from Thesis.cs
                            var thesis = new Thesis
                            {
                                Id = reader.GetInt32(idOrdinal),
                                Title = reader.IsDBNull(titreOrdinal) ? "" : reader.GetString(titreOrdinal),
                                Author = reader.IsDBNull(auteurOrdinal) ? "" : reader.GetString(auteurOrdinal),
                                Specialty = reader.IsDBNull(specialityOrdinal) ? "" : reader.GetString(specialityOrdinal),
                                Type = reader.IsDBNull(typeOrdinal) ? "" : reader.GetString(typeOrdinal),
                                Keyword = reader.IsDBNull(motsClesOrdinal) ? "" : reader.GetString(motsClesOrdinal),
                                Year = reader.IsDBNull(anneeOrdinal) ? DateTime.MinValue : reader.GetDateTime(anneeOrdinal),
                                Resume = reader.IsDBNull(resumeOrdinal) ? "" : reader.GetString(resumeOrdinal),
                                PdfLink = reader.IsDBNull(fichierOrdinal) ? "" : reader.GetString(fichierOrdinal),
                                UserId = reader.IsDBNull(userIdOrdinal) ? -1 : reader.GetInt32(userIdOrdinal),
                                IsFavorite = true
                            };
                            FavoriteTheses.Add(thesis);
                        }
                        await reader.DisposeAsync(); // تعليق: تخلص من القارئ هنا بعد الانتهاء
                    }
                    else { Debug.WriteLine($"No favorites found for User {currentUserId}"); }
                }
                catch (Exception ex) { Debug.WriteLine($"Error loading favorites: {ex.ToString()}"); MessageBox.Show($"Error: {ex.Message}"); }
                finally
                {
                    // تعليق: التأكد من التخلص من القارئ مرة أخرى (آمن إذا تم التخلص منه بالفعل)
                    // Ensure reader disposal again (safe if already disposed)
                    if (reader != null && !reader.IsClosed) { await reader.DisposeAsync(); } // FIX: Check IsClosed, not IsDisposed
                    Debug.WriteLine("DB Resources Disposed (LoadFavorites)");
                    LoadingIndicator.Visibility = Visibility.Collapsed;

                    // تعليق: إظهار الرسالة أو الجدول
                    // Show message or grid
                    bool hasFavorites = FavoriteTheses.Any();
                    // تعليق: التحقق من Null مرة أخرى قبل الوصول
                    // Null-check again before accessing
                    if (FavoritesDataGrid != null) FavoritesDataGrid.Visibility = hasFavorites ? Visibility.Visible : Visibility.Collapsed;
                    if (NoFavoritesMessage != null) NoFavoritesMessage.Visibility = hasFavorites ? Visibility.Collapsed : Visibility.Visible;
                }
            } // End using conn, cmd
        }

        // تعليق: إزالة مفضلة من قاعدة البيانات
        // Remove favorite from DB
        private async Task<bool> RemoveFavoriteDbAsync(int userId, int thesisId)
        {
            // تعليق: استخدام using لإدارة الموارد
            // Use using for resource management
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand("DELETE FROM favoris WHERE user_id = @userId AND these_id = @thesisId", conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@thesisId", thesisId); // Use Id
                    await conn.OpenAsync();
                    int result = await cmd.ExecuteNonQueryAsync();
                    Debug.WriteLine($"RemoveFavoriteDb Result: {result}");
                    return result > 0;
                }
                catch (Exception ex) { Debug.WriteLine($"Error removing fav DB: {ex.ToString()}"); return false; }
            } // using handles disposal
        }

        // --- UI Event Handlers ---
        private void ViewPdfButton_Click(object sender, RoutedEventArgs e)
        { /* ... (Same PDF logic) ... */
            if (sender is Button button && button.CommandParameter is string pdfLink && !string.IsNullOrEmpty(pdfLink))
            {
                try { Process.Start(new ProcessStartInfo(pdfLink) { UseShellExecute = true }); }
                catch (Exception ex) { MessageBox.Show($"Could not open PDF link: {ex.Message}", "Error"); Debug.WriteLine($"PDF Error: {ex}"); }
            }
            else { MessageBox.Show("Invalid PDF link.", "Warning"); }
        }

        private async void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button && button.CommandParameter is Thesis thesisToRemove)) return;
            int currentUserId = Session.CurrentUserId;
            if (currentUserId == -1) return;
            MessageBoxResult confirm = MessageBox.Show($"Remove '{thesisToRemove.Title}' from favorites?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.No) return;
            button.IsEnabled = false;
            // تعليق: استدعاء الدالة باستخدام thesisToRemove.Id الصحيح
            // Call function using correct thesisToRemove.Id
            bool successDb = await RemoveFavoriteDbAsync(currentUserId, thesisToRemove.Id);
            if (successDb)
            {
                FavoriteTheses.Remove(thesisToRemove);
                Debug.WriteLine($"Removed Thesis {thesisToRemove.Id} from Favorites list UI."); // Use Id
                bool hasFavorites = FavoriteTheses.Any();
                // تعليق: التحقق من Null
                // Null checks
                if (FavoritesDataGrid != null) FavoritesDataGrid.Visibility = hasFavorites ? Visibility.Visible : Visibility.Collapsed;
                if (NoFavoritesMessage != null) NoFavoritesMessage.Visibility = hasFavorites ? Visibility.Collapsed : Visibility.Visible;
                if (!hasFavorites) { Debug.WriteLine("Favorites list now empty."); }
            }
            else
            {
                button.IsEnabled = true;
                MessageBox.Show($"Failed to remove favorite.", "Error");
            }
        } // End RemoveFavorite
    } // End Class
} // End Namespace