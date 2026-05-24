using FitnessTracker.Models;

namespace FitnessTracker.Services;

// ── Delegate definitions ─────────────────────────────────────────────────────
public delegate void SessionNotificationHandler(VirtualSession session, string message);

/// <summary>
/// Uses delegates and events to notify clients about session changes.
/// Requirement 7: Session Notification System
/// </summary>
public class SessionNotificationService
{
    public event SessionNotificationHandler? OnSessionScheduled;
    public event SessionNotificationHandler? OnSessionCanceled;
    public event SessionNotificationHandler? OnSessionReminder;

    private readonly List<NotificationEntry> _notifications = new();

    public IReadOnlyList<NotificationEntry> GetNotifications(int clientId) =>
        _notifications.Where(n => n.ClientId == clientId).OrderByDescending(n => n.At).ToList();

    public void NotifyScheduled(VirtualSession session)
    {
        var msg = $"Session with {session.Trainer?.Username ?? "your trainer"} scheduled for {session.SessionTime:MMM d, HH:mm}";
        _notifications.Add(new NotificationEntry(session.ClientId, msg, "scheduled", DateTime.UtcNow));
        InvokeSafely(OnSessionScheduled, session, msg);
    }

    public void NotifyCanceled(VirtualSession session)
    {
        var msg = $"Session on {session.SessionTime:MMM d, HH:mm} was canceled.";
        _notifications.Add(new NotificationEntry(session.ClientId, msg, "canceled", DateTime.UtcNow));
        InvokeSafely(OnSessionCanceled, session, msg);
    }

    public void NotifyReminder(VirtualSession session)
    {
        var msg = $"Reminder: You have a session with {session.Trainer?.Username ?? "your trainer"} in 1 hour!";
        _notifications.Add(new NotificationEntry(session.ClientId, msg, "reminder", DateTime.UtcNow));
        InvokeSafely(OnSessionReminder, session, msg);
    }

    // Guarantee delivery — no silently dropped notifications (Non-Functional Req 4)
    private static void InvokeSafely(SessionNotificationHandler? handler, VirtualSession session, string message)
    {
        if (handler == null) return;
        foreach (var del in handler.GetInvocationList().Cast<SessionNotificationHandler>())
        {
            try { del(session, message); }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NotificationService] Handler error: {ex.Message}");
            }
        }
    }
}

public record NotificationEntry(int ClientId, string Message, string Type, DateTime At);
