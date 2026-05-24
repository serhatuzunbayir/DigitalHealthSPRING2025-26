namespace DigitalHealthTrainer.Services
{
    // DELEGATE tanımı (hocanın istediği delegate requirement)
    public delegate void SessionNotificationHandler(string message, int sessionId, string status);

    public static class NotificationService
    {
        // EVENT tanımları (hocanın istediği event requirement)
        public static event SessionNotificationHandler? OnSessionCreated;
        public static event SessionNotificationHandler? OnSessionCanceled;
        public static event SessionNotificationHandler? OnSessionCompleted;
        public static event SessionNotificationHandler? OnSessionStarted;

        // Yeni session oluşturulduğunda bildirim gönder
        public static void NotifySessionCreated(int sessionId, string clientName, DateTime sessionTime)
        {
            try
            {
                string message = $"New session created for {clientName} at {sessionTime:dd/MM/yyyy HH:mm}";
                OnSessionCreated?.Invoke(message, sessionId, "scheduled");
            }
            catch (Exception)
            {
                // Bildirim hatası uygulamayı kesmemeli
            }
        }

        // Session iptal edildiğinde bildirim gönder
        public static void NotifySessionCanceled(int sessionId, string clientName, DateTime sessionTime)
        {
            try
            {
                string message = $"Session canceled for {clientName} ({sessionTime:dd/MM/yyyy HH:mm})";
                OnSessionCanceled?.Invoke(message, sessionId, "canceled");
            }
            catch (Exception)
            {
                // Bildirim hatası uygulamayı kesmemeli
            }
        }

        // Session tamamlandığında bildirim gönder
        public static void NotifySessionCompleted(int sessionId, string clientName, DateTime sessionTime)
        {
            try
            {
                string message = $"Session completed for {clientName} ({sessionTime:dd/MM/yyyy HH:mm})";
                OnSessionCompleted?.Invoke(message, sessionId, "completed");
            }
            catch (Exception)
            {
                // Bildirim hatası uygulamayı kesmemeli
            }
        }

        // Live session başladığında bildirim gönder
        public static void NotifySessionStarted(int sessionId, string clientName, DateTime sessionTime)
        {
            try
            {
                string message = $"🔴 LIVE session started with {clientName} at {sessionTime:HH:mm}";
                OnSessionStarted?.Invoke(message, sessionId, "in_progress");
            }
            catch (Exception)
            {
                // Bildirim hatası uygulamayı kesmemeli
            }
        }
    }
}
