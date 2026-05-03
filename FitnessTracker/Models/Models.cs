using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessTracker.Models;

[Table("clients")]
public class Client
{
    [Key, Column("client_id")]
    public int ClientId { get; set; }

    [Column("username"), MaxLength(50)]
    public string Username { get; set; } = "";

    [Column("password_hash"), MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [Column("email"), MaxLength(100)]
    public string Email { get; set; } = "";

    public ICollection<ExerciseLog> ExerciseLogs { get; set; } = new List<ExerciseLog>();
    public ICollection<WorkoutProgram> WorkoutPrograms { get; set; } = new List<WorkoutProgram>();
    public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();
    public ICollection<FitnessGoal> FitnessGoals { get; set; } = new List<FitnessGoal>();
    public ICollection<VirtualSession> VirtualSessions { get; set; } = new List<VirtualSession>();
}

[Table("trainers")]
public class Trainer
{
    [Key, Column("trainer_id")]
    public int TrainerId { get; set; }

    [Column("username"), MaxLength(50)]
    public string Username { get; set; } = "";

    [Column("password_hash"), MaxLength(255)]
    public string PasswordHash { get; set; } = "";

    [Column("email"), MaxLength(100)]
    public string Email { get; set; } = "";
}

[Table("assignments")]
public class Assignment
{
    [Key, Column("assignment_id")]
    public int AssignmentId { get; set; }

    [Column("trainer_id")]
    public int TrainerId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [ForeignKey("TrainerId")]
    public Trainer Trainer { get; set; } = null!;

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;
}

[Table("exercise_logs")]
public class ExerciseLog
{
    [Key, Column("log_id")]
    public int LogId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("exercise_type"), MaxLength(50)]
    public string ExerciseType { get; set; } = "";

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("sets")]
    public int? Sets { get; set; }

    [Column("reps")]
    public int? Reps { get; set; }

    [Column("calories_burned")]
    public int? CaloriesBurned { get; set; }

    [Column("log_date")]
    public DateOnly LogDate { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;
}

[Table("workout_programs")]
public class WorkoutProgram
{
    [Key, Column("program_id")]
    public int ProgramId { get; set; }

    [Column("trainer_id")]
    public int TrainerId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("program_name"), MaxLength(100)]
    public string ProgramName { get; set; } = "";

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_date")]
    public DateOnly CreatedDate { get; set; }

    [ForeignKey("TrainerId")]
    public Trainer Trainer { get; set; } = null!;

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;
}

[Table("health_metrics")]
public class HealthMetric
{
    [Key, Column("metric_id")]
    public int MetricId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("weight_kg")]
    public decimal? WeightKg { get; set; }

    [Column("heart_rate_bpm")]
    public int? HeartRateBpm { get; set; }

    [Column("sleep_hours")]
    public decimal? SleepHours { get; set; }

    [Column("water_intake_liters")]
    public decimal? WaterIntakeLiters { get; set; }

    [Column("record_date")]
    public DateOnly RecordDate { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;
}

[Table("fitness_goals")]
public class FitnessGoal
{
    [Key, Column("goal_id")]
    public int GoalId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("goal_type"), MaxLength(50)]
    public string GoalType { get; set; } = ""; // weight_loss | strength_target | weekly_exercise_frequency

    [Column("target_value")]
    public decimal TargetValue { get; set; }

    [Column("current_value")]
    public decimal? CurrentValue { get; set; }

    [Column("deadline")]
    public DateOnly? Deadline { get; set; }

    [Column("status"), MaxLength(20)]
    public string? Status { get; set; } // completed | in_progress | missed

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;
}

[Table("virtual_sessions")]
public class VirtualSession
{
    [Key, Column("session_id")]
    public int SessionId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [Column("trainer_id")]
    public int TrainerId { get; set; }

    [Column("session_time")]
    public DateTime SessionTime { get; set; }

    [Column("duration_minutes")]
    public int DurationMinutes { get; set; }

    [Column("status"), MaxLength(20)]
    public string Status { get; set; } = "scheduled"; // scheduled | canceled | completed

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;

    [ForeignKey("TrainerId")]
    public Trainer Trainer { get; set; } = null!;
}
