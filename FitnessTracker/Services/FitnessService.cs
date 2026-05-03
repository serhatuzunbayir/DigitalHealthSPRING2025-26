using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;
using FitnessTracker.Models;

namespace FitnessTracker.Services;

public class FitnessService
{
    private readonly AppDbContext _db;

    public FitnessService(AppDbContext db)
    {
        _db = db;
    }

    // ─── Exercise Logs ───────────────────────────────────────────────────────

    public async Task<List<ExerciseLog>> GetLogsAsync(int clientId, int days = 30)
    {
        var since = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
        return await _db.ExerciseLogs
            .Where(l => l.ClientId == clientId && l.LogDate >= since)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();
    }

    public async Task AddLogAsync(ExerciseLog log)
    {
        _db.ExerciseLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLogAsync(int logId)
    {
        var log = await _db.ExerciseLogs.FindAsync(logId);
        if (log != null)
        {
            _db.ExerciseLogs.Remove(log);
            await _db.SaveChangesAsync();
        }
    }

    // ─── Health Metrics ──────────────────────────────────────────────────────

    public async Task<List<HealthMetric>> GetMetricsAsync(int clientId, int days = 30)
    {
        var since = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
        return await _db.HealthMetrics
            .Where(m => m.ClientId == clientId && m.RecordDate >= since)
            .OrderByDescending(m => m.RecordDate)
            .ToListAsync();
    }

    public async Task AddMetricAsync(HealthMetric metric)
    {
        _db.HealthMetrics.Add(metric);
        await _db.SaveChangesAsync();
    }

    // ─── Fitness Goals ───────────────────────────────────────────────────────

    public async Task<List<FitnessGoal>> GetGoalsAsync(int clientId)
    {
        return await _db.FitnessGoals
            .Where(g => g.ClientId == clientId)
            .OrderBy(g => g.Status)
            .ToListAsync();
    }

    public async Task AddGoalAsync(FitnessGoal goal)
    {
        goal.Status = "in_progress";
        _db.FitnessGoals.Add(goal);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateGoalProgressAsync(int goalId, decimal currentValue)
    {
        var goal = await _db.FitnessGoals.FindAsync(goalId);
        if (goal == null) return;

        goal.CurrentValue = currentValue;

        goal.Status = goal.GoalType switch
        {
            "weight_loss"   => currentValue <= goal.TargetValue ? "completed" : "in_progress",
            "strength_target" => currentValue >= goal.TargetValue ? "completed" : "in_progress",
            "weekly_exercise_frequency" => currentValue >= goal.TargetValue ? "completed" : "in_progress",
            _ => "in_progress"
        };

        if (goal.Deadline.HasValue && goal.Status != "completed" &&
            DateOnly.FromDateTime(DateTime.Today) > goal.Deadline.Value)
            goal.Status = "missed";

        await _db.SaveChangesAsync();
    }

    // ─── Virtual Sessions ────────────────────────────────────────────────────

    public async Task<List<VirtualSession>> GetSessionsAsync(int clientId)
    {
        return await _db.VirtualSessions
            .Include(s => s.Trainer)
            .Where(s => s.ClientId == clientId && s.SessionTime >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(s => s.SessionTime)
            .ToListAsync();
    }

    public async Task<List<Trainer>> GetAssignedTrainersAsync(int clientId)
    {
        return await _db.Assignments
            .Where(a => a.ClientId == clientId)
            .Include(a => a.Trainer)
            .Select(a => a.Trainer)
            .ToListAsync();
    }

    public async Task RequestSessionAsync(VirtualSession session)
    {
        session.Status = "scheduled";
        _db.VirtualSessions.Add(session);
        await _db.SaveChangesAsync();
    }

    public async Task CancelSessionAsync(int sessionId)
    {
        var session = await _db.VirtualSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = "canceled";
            await _db.SaveChangesAsync();
        }
    }

    // ─── Workout Programs ────────────────────────────────────────────────────

    public async Task<List<WorkoutProgram>> GetProgramsAsync(int clientId)
    {
        return await _db.WorkoutPrograms
            .Include(p => p.Trainer)
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();
    }

    // ─── Analytics (LINQ) ────────────────────────────────────────────────────

    public async Task<DashboardStats> GetDashboardStatsAsync(int clientId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
        var monthAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

        var logs = await _db.ExerciseLogs
            .Where(l => l.ClientId == clientId && l.LogDate >= monthAgo)
            .ToListAsync();

        var goals = await _db.FitnessGoals
            .Where(g => g.ClientId == clientId)
            .ToListAsync();

        var lastMetric = await _db.HealthMetrics
            .Where(m => m.ClientId == clientId)
            .OrderByDescending(m => m.RecordDate)
            .FirstOrDefaultAsync();

        var upcomingSession = await _db.VirtualSessions
            .Include(s => s.Trainer)
            .Where(s => s.ClientId == clientId && s.Status == "scheduled" && s.SessionTime > DateTime.UtcNow)
            .OrderBy(s => s.SessionTime)
            .FirstOrDefaultAsync();

        return new DashboardStats
        {
            TotalCaloriesMonth = logs.Sum(l => l.CaloriesBurned ?? 0),
            TotalMinutesMonth = logs.Sum(l => l.DurationMinutes),
            SessionsThisWeek = logs.Count(l => l.LogDate >= weekAgo),
            GoalCompletionPercent = goals.Count == 0 ? 0 :
                (int)(goals.Count(g => g.Status == "completed") * 100.0 / goals.Count),
            ActiveGoals = goals.Count(g => g.Status == "in_progress"),
            MissedGoals = goals.Count(g => g.Status == "missed"),
            LatestWeight = lastMetric?.WeightKg,
            LatestHeartRate = lastMetric?.HeartRateBpm,
            UpcomingSession = upcomingSession,
            WeeklyCalories = logs
                .Where(l => l.LogDate >= weekAgo)
                .GroupBy(l => l.LogDate)
                .OrderBy(g => g.Key)
                .Select(g => new DailyCalories
                {
                    Date = g.Key,
                    Calories = g.Sum(l => l.CaloriesBurned ?? 0)
                }).ToList()
        };
    }
}

public class DashboardStats
{
    public int TotalCaloriesMonth { get; set; }
    public int TotalMinutesMonth { get; set; }
    public int SessionsThisWeek { get; set; }
    public int GoalCompletionPercent { get; set; }
    public int ActiveGoals { get; set; }
    public int MissedGoals { get; set; }
    public decimal? LatestWeight { get; set; }
    public int? LatestHeartRate { get; set; }
    public VirtualSession? UpcomingSession { get; set; }
    public List<DailyCalories> WeeklyCalories { get; set; } = new();
}

public class DailyCalories
{
    public DateOnly Date { get; set; }
    public int Calories { get; set; }
}
