using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessTracker.Data;

namespace FitnessTracker.Controllers;

// All report endpoints use LINQ for filtering, grouping and analytics
// satisfying req #4 (Progress Tracking & Analytics) and req #10 (Progress Report Generation).

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ReportsController(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // GET /api/reports/{clientId}/exercise-logs
    // Query params: from (date), to (date), exerciseType
    // Returns raw exercise logs filtered by date range and optional type (req #10).
    [HttpGet("{clientId:int}/exercise-logs")]
    public async Task<IActionResult> GetExerciseLogs(
        int clientId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? exerciseType)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);

        var query = db.ExerciseLogs
            .Where(l => l.ClientId == clientId && l.LogDate >= fromDate && l.LogDate <= toDate);

        if (!string.IsNullOrWhiteSpace(exerciseType))
            query = query.Where(l => l.ExerciseType == exerciseType);

        var logs = await query
            .OrderByDescending(l => l.LogDate)
            .Select(l => new
            {
                l.LogId,
                l.ExerciseType,
                l.DurationMinutes,
                l.Sets,
                l.Reps,
                l.CaloriesBurned,
                l.LogDate
            })
            .ToListAsync();

        return Ok(logs);
    }

    // GET /api/reports/{clientId}/weekly-averages
    // Query params: from, to, exerciseType
    // Returns LINQ-grouped weekly averages (req #4 — weekly averages, req #10).
    [HttpGet("{clientId:int}/weekly-averages")]
    public async Task<IActionResult> GetWeeklyAverages(
        int clientId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? exerciseType)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);

        var query = db.ExerciseLogs
            .Where(l => l.ClientId == clientId && l.LogDate >= fromDate && l.LogDate <= toDate);

        if (!string.IsNullOrWhiteSpace(exerciseType))
            query = query.Where(l => l.ExerciseType == exerciseType);

        var logs = await query.ToListAsync();

        // LINQ grouping by ISO week number
        var weeklyAverages = logs
            .GroupBy(l => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                l.LogDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday))
            .Select(g => new
            {
                WeekNumber = g.Key,
                WeekLabel = $"Week {g.Key}",
                AvgDurationMinutes = Math.Round(g.Average(l => l.DurationMinutes), 1),
                AvgCaloriesBurned = Math.Round(g.Average(l => l.CaloriesBurned ?? 0), 1),
                TotalSessions = g.Count(),
                ExerciseTypes = g.Select(l => l.ExerciseType).Distinct().OrderBy(t => t).ToList()
            })
            .OrderBy(w => w.WeekNumber)
            .ToList();

        return Ok(weeklyAverages);
    }

    // GET /api/reports/{clientId}/personal-records
    // Returns the best performance per exercise type (req #4 — personal records).
    [HttpGet("{clientId:int}/personal-records")]
    public async Task<IActionResult> GetPersonalRecords(int clientId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var logs = await db.ExerciseLogs
            .Where(l => l.ClientId == clientId)
            .ToListAsync();

        // LINQ: best (max) value per exercise type
        var records = logs
            .GroupBy(l => l.ExerciseType)
            .Select(g => new
            {
                ExerciseType = g.Key,
                MaxDurationMinutes = g.Max(l => l.DurationMinutes),
                MaxCaloriesBurned = g.Max(l => l.CaloriesBurned ?? 0),
                MaxReps = g.Max(l => l.Reps ?? 0),
                TotalSessions = g.Count(),
                LastPerformed = g.Max(l => l.LogDate)
            })
            .OrderBy(r => r.ExerciseType)
            .ToList();

        return Ok(records);
    }

    // GET /api/reports/{clientId}/health-metrics
    // Query params: from, to
    [HttpGet("{clientId:int}/health-metrics")]
    public async Task<IActionResult> GetHealthMetrics(
        int clientId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var fromDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var toDate = to ?? DateOnly.FromDateTime(DateTime.Today);

        var metrics = await db.HealthMetrics
            .Where(m => m.ClientId == clientId && m.RecordDate >= fromDate && m.RecordDate <= toDate)
            .OrderByDescending(m => m.RecordDate)
            .Select(m => new
            {
                m.MetricId,
                m.RecordDate,
                m.WeightKg,
                m.HeartRateBpm,
                m.SleepHours,
                m.WaterIntakeLiters
            })
            .ToListAsync();

        return Ok(metrics);
    }

    // GET /api/reports/{clientId}/goals
    // Query params: category (goal_type filter)
    // Returns goals with completion statistics (req #5, #10).
    [HttpGet("{clientId:int}/goals")]
    public async Task<IActionResult> GetGoals(int clientId, [FromQuery] string? category)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var query = db.FitnessGoals.Where(g => g.ClientId == clientId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(g => g.GoalType == category);

        var goals = await query
            .OrderBy(g => g.Status)
            .Select(g => new
            {
                g.GoalId,
                g.GoalType,
                g.TargetValue,
                g.CurrentValue,
                g.Deadline,
                g.Status
            })
            .ToListAsync();

        // LINQ stats (req #4 — goal completion percentages)
        var total = goals.Count;
        var completed = goals.Count(g => g.Status == "completed");
        var inProgress = goals.Count(g => g.Status == "in_progress");
        var missed = goals.Count(g => g.Status == "missed");
        var completionRate = total > 0 ? Math.Round(completed * 100.0 / total, 1) : 0.0;

        return Ok(new
        {
            Goals = goals,
            Stats = new { Total = total, Completed = completed, InProgress = inProgress, Missed = missed, CompletionRate = completionRate }
        });
    }

    // GET /api/reports/{clientId}/exercise-types
    // Returns the distinct exercise types logged by a client (for filter dropdowns).
    [HttpGet("{clientId:int}/exercise-types")]
    public async Task<IActionResult> GetExerciseTypes(int clientId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var types = await db.ExerciseLogs
            .Where(l => l.ClientId == clientId)
            .Select(l => l.ExerciseType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        return Ok(types);
    }
}
