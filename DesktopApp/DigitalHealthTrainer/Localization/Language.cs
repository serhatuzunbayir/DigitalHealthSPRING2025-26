namespace DigitalHealthTrainer.Localization
{
    public enum AppLanguage
    {
        English,
        Turkish
    }

    public static class Lang
    {
        private static AppLanguage _current = AppLanguage.English;
        private static Dictionary<string, Dictionary<AppLanguage, string>> _strings = new();

        public static AppLanguage Current
        {
            get => _current;
            set => _current = value;
        }

        static Lang()
        {
            // ========== GENERAL ==========
            Add("app_title", "Digital Health Trainer", "Digital Health Trainer");
            Add("error", "Error", "Hata");
            Add("success", "Success", "Başarılı");
            Add("save", "Save", "Kaydet");
            Add("cancel", "Cancel", "İptal");
            Add("delete", "Delete", "Sil");
            Add("edit", "Edit", "Düzenle");
            Add("close", "Close", "Kapat");
            Add("back", "← Back", "← Geri");
            Add("refresh", "Refresh", "Yenile");
            Add("search", "Search", "Ara");
            Add("filter", "Filter", "Filtrele");
            Add("yes", "Yes", "Evet");
            Add("no", "No", "Hayır");
            Add("loading", "Loading...", "Yükleniyor...");
            Add("connection_error", "Connection error: ", "Bağlantı hatası: ");

            // ========== LOGIN FORM ==========
            Add("login_title", "Trainer Login", "Trainer Girişi");
            Add("register_title", "Trainer Registration", "Trainer Kaydı");
            Add("username", "Username:", "Kullanıcı Adı:");
            Add("password", "Password:", "Şifre:");
            Add("email", "E-mail:", "E-posta:");
            Add("btn_login", "Login", "Giriş Yap");
            Add("btn_register", "Register", "Kayıt Ol");
            Add("login_success", "Login successful! Redirecting...", "Giriş başarılı! Yönlendiriliyorsunuz...");
            Add("login_failed", "Invalid username or password!", "Kullanıcı adı veya şifre hatalı!");
            Add("login_welcome", "Welcome, {0}!", "Hoş geldiniz, {0}!");
            Add("register_success", "Registration successful! You can now login.", "Kayıt başarılı! Şimdi giriş yapabilirsiniz.");
            Add("register_duplicate", "This username is already taken.", "Bu kullanıcı adı zaten alınmış.");
            Add("register_error", "Registration error: ", "Kayıt hatası: ");
            Add("fields_required", "All fields are required.", "Tüm alanları doldurun.");
            Add("login_fields_required", "Username and password are required.", "Kullanıcı adı ve şifre boş bırakılamaz.");
            Add("not_tested", "Not tested yet.", "Henüz test edilmedi.");

            // ========== LANGUAGE SELECTOR ==========
            Add("language", "Language:", "Dil:");

            // ========== CONNECTION TEST ==========
            Add("test_title", "Database Connection Test", "Veritabanı Bağlantı Testi");
            Add("btn_test", "Test Connection", "Bağlantıyı Test Et");
            Add("test_connecting", "Connecting...", "Bağlanılıyor...");
            Add("test_success", "Connection successful!", "Bağlantı başarılı!");
            Add("test_failed", "Connection failed!", "Bağlantı başarısız!");
            Add("test_no_tables", "(No tables found)", "(Henüz tablo oluşturulmamış)");

            // ========== DASHBOARD ==========
            Add("dashboard_title", "Dashboard - Client Monitoring", "Dashboard - Client Takibi");
            Add("client_name", "Client", "Danışan");
            Add("last_activity", "Last Activity", "Son Aktivite");
            Add("adherence_rate", "Adherence Rate", "Uyum Oranı");
            Add("progress", "Progress", "İlerleme");
            Add("assigned_clients", "Assigned Clients", "Atanmış Danışanlar");
            Add("no_clients", "No assigned clients found.", "Atanmış danışan bulunamadı.");

            // ========== WORKOUT PROGRAMS ==========
            Add("workout_title", "Workout Program Management", "Antrenman Programı Yönetimi");
            Add("program_name", "Program Name:", "Program Adı:");
            Add("description", "Description:", "Açıklama:");
            Add("select_client", "Select Client:", "Danışan Seç:");
            Add("created_date", "Created Date", "Oluşturma Tarihi");
            Add("add_program", "Add Program", "Program Ekle");
            Add("update_program", "Update Program", "Programı Güncelle");
            Add("delete_program", "Delete Program", "Programı Sil");
            Add("program_saved", "Program saved successfully.", "Program başarıyla kaydedildi.");
            Add("program_deleted", "Program deleted successfully.", "Program başarıyla silindi.");
            Add("confirm_delete", "Are you sure you want to delete this program?", "Bu programı silmek istediğinize emin misiniz?");

            // ========== VIRTUAL SESSIONS ==========
            Add("sessions_title", "Virtual Session Management", "Sanal Seans Yönetimi");
            Add("session_time", "Session Time", "Seans Zamanı");
            Add("duration", "Duration (min)", "Süre (dk)");
            Add("status", "Status", "Durum");
            Add("status_scheduled", "Scheduled", "Planlandı");
            Add("status_canceled", "Canceled", "İptal Edildi");
            Add("status_completed", "Completed", "Tamamlandı");
            Add("mark_completed", "Mark as Completed", "Tamamlandı İşaretle");
            Add("cancel_session", "Cancel Session", "Seansı İptal Et");

            // ========== NOTIFICATIONS ==========
            Add("notification", "Notification", "Bildirim");
            Add("session_created", "New session scheduled: {0}", "Yeni seans planlandı: {0}");
            Add("session_canceled", "Session canceled: {0}", "Seans iptal edildi: {0}");
            Add("session_updated", "Session updated: {0}", "Seans güncellendi: {0}");

            // ========== PROGRESS REPORTS ==========
            Add("reports_title", "Progress Reports", "İlerleme Raporları");
            Add("date_range", "Date Range:", "Tarih Aralığı:");
            Add("from_date", "From:", "Başlangıç:");
            Add("to_date", "To:", "Bitiş:");
            Add("exercise_type", "Exercise Type:", "Egzersiz Tipi:");
            Add("goal_category", "Goal Category:", "Hedef Kategorisi:");
            Add("generate_report", "Generate Report", "Rapor Oluştur");
            Add("weekly_avg", "Weekly Average", "Haftalık Ortalama");
            Add("personal_record", "Personal Record", "Kişisel Rekor");
            Add("goal_completion", "Goal Completion %", "Hedef Tamamlanma %");
            Add("all_types", "All Types", "Tüm Tipler");
        }

        private static void Add(string key, string en, string tr)
        {
            _strings[key] = new Dictionary<AppLanguage, string>
            {
                { AppLanguage.English, en },
                { AppLanguage.Turkish, tr }
            };
        }

        public static string Get(string key)
        {
            if (_strings.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(_current, out var text))
                    return text;
            }
            return $"[{key}]"; // fallback: göster ki eksik key belli olsun
        }

        // Format destekli versiyon: Lang.Format("login_welcome", username)
        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
