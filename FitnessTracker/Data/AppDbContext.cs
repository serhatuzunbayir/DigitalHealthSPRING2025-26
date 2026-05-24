using Microsoft.EntityFrameworkCore;
using FitnessTracker.Models;

namespace FitnessTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<ExerciseLog> ExerciseLogs { get; set; }
    public DbSet<WorkoutProgram> WorkoutPrograms { get; set; }
    public DbSet<HealthMetric> HealthMetrics { get; set; }
    public DbSet<FitnessGoal> FitnessGoals { get; set; }
    public DbSet<VirtualSession> VirtualSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Assignment>()
            .HasIndex(a => new { a.TrainerId, a.ClientId })
            .IsUnique();

        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Username)
            .IsUnique();

        modelBuilder.Entity<Trainer>()
            .HasIndex(t => t.Username)
            .IsUnique();

        modelBuilder.Entity<FitnessGoal>()
            .Property(g => g.GoalType)
            .HasConversion<string>();

        modelBuilder.Entity<VirtualSession>()
            .Property(s => s.Status)
            .HasDefaultValue("scheduled");
    }
}
