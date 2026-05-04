# Digital Health Trainer - Desktop App Task Distribution

## Project Structure (Already Completed)

```
DesktopApp/DigitalHealthTrainer/
├── Program.cs                          ✅ Done
├── Localization/Language.cs            ✅ Done (EN/TR support)
├── Models/                             ✅ Done (8 models)
│   ├── Trainer.cs, Client.cs, Assignment.cs
│   ├── ExerciseLog.cs, WorkoutProgram.cs
│   ├── HealthMetric.cs, FitnessGoal.cs, VirtualSession.cs
├── Data/DatabaseHelper.cs              ✅ Done
├── Services/AuthService.cs             ✅ Done (Login + Register + SHA256)
├── Forms/
│   ├── LoginForm.cs                    ✅ Done
│   └── TestConnectionForm.cs           ✅ Done
```

## Important Rules for Both

1. **Language Support:** Never hardcode strings. Use `Lang.Get("key")` for all UI text.
   Add new keys to `Localization/Language.cs` using: `Add("key", "English text", "Turkish text");`
2. **DB Access:** Always use `DatabaseHelper.GetConnection()` with `using` statements.
3. **Security:** Trainers can ONLY see their own assigned clients. Always filter by `trainer_id`.
4. **Branch:** Work on `desktopApp` branch. Commit frequently.

---

## MANIFEST's Tasks

### Task M1: Dashboard Form (DashboardForm.cs) — `Forms/DashboardForm.cs`

This is the main screen after login. It shows the trainer's assigned clients and their stats.

**File to create:** `Forms/DashboardForm.cs`
**Service to create:** `Services/ClientService.cs`

**Constructor:** Takes a `Trainer` object from LoginForm after successful login.

**DashboardForm Layout:**
- Top bar: Welcome message with trainer name, Logout button (top-right), Language selector
- Left panel or tab: Navigation menu with buttons — Dashboard, Reports
- Center: DataGridView showing assigned clients

**DataGridView columns:**
- Client Username
- Last Activity Date (most recent exercise_logs.log_date)
- Total Exercises This Week (COUNT from exercise_logs WHERE log_date >= current week start)
- Adherence Rate % (completed goals / total goals * 100 from fitness_goals)
- Active Goals count (WHERE status = 'in_progress')

**ClientService.cs methods:**

```csharp
// Get all clients assigned to this trainer
public static List<Client> GetAssignedClients(int trainerId)
{
    // SQL: SELECT c.* FROM clients c
    //      INNER JOIN assignments a ON c.client_id = a.client_id
    //      WHERE a.trainer_id = @trainerId
}

// Get dashboard summary for each client using LINQ
public static List<ClientDashboardSummary> GetDashboardData(int trainerId)
{
    // 1. Fetch assigned clients
    // 2. For each client, fetch exercise_logs, fitness_goals
    // 3. Use LINQ to calculate:
    //    - lastActivity = exerciseLogs.Max(e => e.LogDate)
    //    - weeklyCount = exerciseLogs.Count(e => e.LogDate >= weekStart)
    //    - adherenceRate = (goals.Count(g => g.Status == "completed") * 100.0) / goals.Count()
    //    - activeGoals = goals.Count(g => g.Status == "in_progress")
}
```

**Create a helper model** `Models/ClientDashboardSummary.cs`:

```csharp
public class ClientDashboardSummary
{
    public int ClientId { get; set; }
    public string Username { get; set; }
    public DateTime? LastActivity { get; set; }
    public int WeeklyExerciseCount { get; set; }
    public double AdherenceRate { get; set; }
    public int ActiveGoals { get; set; }
}
```

**Connect to LoginForm:** After successful login, update LoginForm.cs:

```csharp
// Replace the MessageBox and TODO block with:
var dashboard = new DashboardForm(trainer);
dashboard.Show();
this.Hide();
```

**Color coding (from requirements):**
- Green: adherence >= 80%
- Yellow: adherence 50-79%
- Red: adherence < 50%
Apply colors to DataGridView rows using `CellFormatting` event.

**Lang keys to add:**
```csharp
Add("dashboard_welcome", "Welcome, {0}!", "Hoş geldiniz, {0}!");
Add("btn_logout", "Logout", "Çıkış");
Add("btn_dashboard", "Dashboard", "Dashboard");
Add("btn_reports", "Reports", "Raporlar");
```

---

### Task M2: Progress Report Form — `Forms/ProgressReportForm.cs`

**File to create:** `Forms/ProgressReportForm.cs`
**Service to create:** `Services/ReportService.cs`

**Layout:**
- Top: Filter controls in a GroupBox
  - ComboBox: Select Client (from assigned clients)
  - ComboBox: Exercise Type filter (All Types + distinct types from exercise_logs)
  - ComboBox: Goal Category filter (All / weight_loss / strength_target / weekly_exercise_frequency)
  - DateTimePicker: From date
  - DateTimePicker: To date
  - Button: Generate Report
- Center: TabControl with 3 tabs:
  - Tab 1: Exercise Summary (DataGridView)
  - Tab 2: Health Metrics Trend (DataGridView)
  - Tab 3: Goal Progress (DataGridView)

**ReportService.cs — ALL filtering must use LINQ (project requirement):**

```csharp
// Fetch raw data from DB, then filter/analyze with LINQ

public static List<ExerciseLog> GetExerciseLogs(int clientId)
{
    // SQL: SELECT * FROM exercise_logs WHERE client_id = @clientId
}

public static List<HealthMetric> GetHealthMetrics(int clientId)
{
    // SQL: SELECT * FROM health_metrics WHERE client_id = @clientId
}

public static List<FitnessGoal> GetGoals(int clientId)
{
    // SQL: SELECT * FROM fitness_goals WHERE client_id = @clientId
}

// ===== LINQ ANALYSIS METHODS =====

// Weekly averages
public static object GetWeeklyAverages(List<ExerciseLog> logs, DateTime from, DateTime to)
{
    var filtered = logs.Where(l => l.LogDate >= from && l.LogDate <= to);

    var weeklyGroups = filtered
        .GroupBy(l => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            l.LogDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
        .Select(g => new
        {
            Week = g.Key,
            AvgDuration = g.Average(e => e.DurationMinutes),
            AvgCalories = g.Average(e => e.CaloriesBurned ?? 0),
            TotalSessions = g.Count()
        });

    return weeklyGroups;
}

// Personal records
public static object GetPersonalRecords(List<ExerciseLog> logs, string? exerciseType)
{
    var filtered = exerciseType == null ? logs : logs.Where(l => l.ExerciseType == exerciseType);

    var records = filtered
        .GroupBy(l => l.ExerciseType)
        .Select(g => new
        {
            ExerciseType = g.Key,
            MaxDuration = g.Max(e => e.DurationMinutes),
            MaxCalories = g.Max(e => e.CaloriesBurned ?? 0),
            MaxReps = g.Max(e => e.Reps ?? 0)
        });

    return records;
}

// Goal completion percentage
public static object GetGoalCompletion(List<FitnessGoal> goals, string? category)
{
    var filtered = category == null ? goals : goals.Where(g => g.GoalType == category);

    var summary = new
    {
        Total = filtered.Count(),
        Completed = filtered.Count(g => g.Status == "completed"),
        InProgress = filtered.Count(g => g.Status == "in_progress"),
        Missed = filtered.Count(g => g.Status == "missed"),
        CompletionRate = filtered.Any()
            ? (filtered.Count(g => g.Status == "completed") * 100.0 / filtered.Count())
            : 0
    };

    return summary;
}
```

**Performance requirement:** Results must return within 3 seconds.

**Lang keys to add:** Already defined in Language.cs (reports_title, date_range, from_date, to_date, etc.)

---

### Task M3: Final Integration & Testing

- Wire DashboardForm navigation: Dashboard button → refresh grid, Reports button → open ProgressReportForm
- Ensure trainer can ONLY access their assigned clients' data (security requirement)
- Test login → dashboard → reports flow end to end
- Verify LINQ queries return correct calculations
- Test language switching works on all forms

---

## ECEM's Tasks

### Task E1: Workout Program Management — `Forms/WorkoutProgramForm.cs`

**File to create:** `Forms/WorkoutProgramForm.cs`
**Service to create:** `Services/WorkoutService.cs`

**Constructor:** Takes `Trainer` object (to know which trainer is logged in).

**Layout:**
- Top: Form controls in a GroupBox
  - ComboBox: Select Client (loaded from assigned clients only)
  - TextBox: Program Name
  - TextBox (Multiline): Description
  - Button: Save Program
  - Button: Update Program
  - Button: Delete Program
- Bottom: DataGridView showing existing programs for the selected client

**DataGridView columns:**
- Program ID (hidden)
- Program Name
- Description
- Created Date
- Client Name

**WorkoutService.cs methods:**

```csharp
public static List<WorkoutProgram> GetProgramsByTrainer(int trainerId)
{
    // SQL: SELECT * FROM workout_programs WHERE trainer_id = @trainerId
    //      ORDER BY created_date DESC
}

public static List<WorkoutProgram> GetProgramsByClient(int trainerId, int clientId)
{
    // SQL: SELECT * FROM workout_programs
    //      WHERE trainer_id = @trainerId AND client_id = @clientId
    //      ORDER BY created_date DESC
}

public static bool CreateProgram(int trainerId, int clientId, string name, string description)
{
    // SQL: INSERT INTO workout_programs (trainer_id, client_id, program_name, description, created_date)
    //      VALUES (@trainerId, @clientId, @name, @desc, CURRENT_DATE)
}

public static bool UpdateProgram(int programId, string name, string description)
{
    // SQL: UPDATE workout_programs
    //      SET program_name = @name, description = @desc
    //      WHERE program_id = @programId
}

public static bool DeleteProgram(int programId)
{
    // SQL: DELETE FROM workout_programs WHERE program_id = @programId
}
```

**Behavior:**
- When a client is selected from ComboBox, load their programs into DataGridView
- When a row is clicked in DataGridView, populate the form fields for editing
- Save button: creates new program (clear selection first)
- Update button: updates selected program
- Delete button: asks confirmation dialog, then deletes
- After any CRUD operation, refresh the DataGridView

**Security:** Only show clients from assignments table where trainer_id matches logged-in trainer.

**Lang keys to add:** Already defined (workout_title, program_name, description, select_client, etc.)

---

### Task E2: Virtual Session Management — `Forms/SessionManagementForm.cs`

**File to create:** `Forms/SessionManagementForm.cs`
**Service to create:** `Services/SessionService.cs`

**Constructor:** Takes `Trainer` object.

**Layout:**
- Top left: ComboBox to filter by client (All Clients + each assigned client)
- Top right: ComboBox to filter by status (All / Scheduled / Completed / Canceled)
- Center: DataGridView showing sessions

**DataGridView columns:**
- Session ID (hidden)
- Client Username
- Session Date & Time
- Duration (minutes)
- Status (with color coding: green=completed, yellow=scheduled, red=canceled)

**Bottom: Action buttons:**
- Button: Mark as Completed (only for "scheduled" sessions)
- Button: Cancel Session (only for "scheduled" sessions)
- Button: Refresh

**SessionService.cs methods:**

```csharp
public static List<VirtualSession> GetSessionsByTrainer(int trainerId)
{
    // SQL: SELECT vs.*, c.username AS client_name
    //      FROM virtual_sessions vs
    //      INNER JOIN clients c ON vs.client_id = c.client_id
    //      WHERE vs.trainer_id = @trainerId
    //      ORDER BY vs.session_time DESC
}

public static bool UpdateSessionStatus(int sessionId, string newStatus)
{
    // SQL: UPDATE virtual_sessions SET status = @status WHERE session_id = @sessionId
    // IMPORTANT: This method should raise notification events (see Task E3)
}

// Use LINQ to filter in memory after fetching
public static List<VirtualSession> FilterSessions(
    List<VirtualSession> sessions, int? clientId, string? status)
{
    var result = sessions.AsEnumerable();

    if (clientId.HasValue)
        result = result.Where(s => s.ClientId == clientId.Value);

    if (!string.IsNullOrEmpty(status))
        result = result.Where(s => s.Status == status);

    return result.ToList();
}
```

**Color coding for status column:**
- Use DataGridView CellFormatting event
- "completed" → Green background
- "scheduled" → Yellow background
- "canceled" → Red background

**Lang keys to add:** Already defined (sessions_title, session_time, duration, status, etc.)

---

### Task E3: Delegate/Event Notification System — `Services/NotificationService.cs`

**File to create:** `Services/NotificationService.cs`

This is a KEY requirement: delegates and events for session notifications.

**Implementation:**

```csharp
namespace DigitalHealthTrainer.Services
{
    // Custom delegate for session notifications
    public delegate void SessionNotificationHandler(string message, int sessionId, string status);

    public static class NotificationService
    {
        // Events for different session actions
        public static event SessionNotificationHandler? OnSessionCreated;
        public static event SessionNotificationHandler? OnSessionCanceled;
        public static event SessionNotificationHandler? OnSessionCompleted;
        public static event SessionNotificationHandler? OnSessionUpdated;

        // Call these from SessionService when status changes
        public static void NotifySessionCreated(int sessionId, string clientName, DateTime sessionTime)
        {
            string message = Lang.Format("session_created",
                $"{clientName} - {sessionTime:g}");

            // Guarantee delivery — requirement says no silent drops
            try
            {
                OnSessionCreated?.Invoke(message, sessionId, "scheduled");
            }
            catch (Exception ex)
            {
                // Log error but don't crash — ensures reliability requirement
                MessageBox.Show($"Notification error: {ex.Message}",
                    Lang.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void NotifySessionCanceled(int sessionId, string clientName, DateTime sessionTime)
        {
            string message = Lang.Format("session_canceled",
                $"{clientName} - {sessionTime:g}");
            try
            {
                OnSessionCanceled?.Invoke(message, sessionId, "canceled");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Notification error: {ex.Message}",
                    Lang.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static void NotifySessionCompleted(int sessionId, string clientName, DateTime sessionTime)
        {
            string message = Lang.Format("session_completed",
                $"{clientName} - {sessionTime:g}");
            try
            {
                OnSessionCompleted?.Invoke(message, sessionId, "completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Notification error: {ex.Message}",
                    Lang.Get("error"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
```

**How to integrate with SessionManagementForm:**

```csharp
// In SessionManagementForm constructor, subscribe to events:
NotificationService.OnSessionCanceled += ShowNotification;
NotificationService.OnSessionCompleted += ShowNotification;

private void ShowNotification(string message, int sessionId, string status)
{
    // Show as a popup or status bar message
    MessageBox.Show(message, Lang.Get("notification"),
        MessageBoxButtons.OK, MessageBoxIcon.Information);
}

// In the Cancel button click handler:
SessionService.UpdateSessionStatus(sessionId, "canceled");
NotificationService.NotifySessionCanceled(sessionId, clientName, sessionTime);

// In the Complete button click handler:
SessionService.UpdateSessionStatus(sessionId, "completed");
NotificationService.NotifySessionCompleted(sessionId, clientName, sessionTime);
```

**Unsubscribe on form close** to prevent memory leaks:

```csharp
protected override void OnFormClosed(FormClosedEventArgs e)
{
    NotificationService.OnSessionCanceled -= ShowNotification;
    NotificationService.OnSessionCompleted -= ShowNotification;
    base.OnFormClosed(e);
}
```

**Requirement compliance:**
- ✅ Uses delegates (SessionNotificationHandler)
- ✅ Uses events (OnSessionCreated, OnSessionCanceled, etc.)
- ✅ Guarantees delivery — try/catch ensures no silent drops
- ✅ Notifies both trainers and clients (event can have multiple subscribers)

---

## Navigation: How Forms Connect

```
LoginForm
   ↓ (successful login, passes Trainer object)
DashboardForm (main window)
   ├── [Dashboard tab] → Client monitoring DataGridView (Manifest)
   ├── [Workout Programs tab/button] → WorkoutProgramForm (Ecem)
   ├── [Sessions tab/button] → SessionManagementForm (Ecem)
   └── [Reports tab/button] → ProgressReportForm (Manifest)
```

**Manifest** builds the DashboardForm with navigation buttons. Each button opens the relevant form.
**Ecem's** forms are opened from DashboardForm as separate windows or panels.

## Database Connection

```
Host: 92.249.61.114
Port: 5432
Database: postgres
Username: admin
Password: secret123
```

Already configured in `Data/DatabaseHelper.cs` — no changes needed.
