using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data;

public class WorkoutTrackerDbContext(DbContextOptions<WorkoutTrackerDbContext> options) : DbContext(options)
{
    // DbSet properties - each becomes a table
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<Program> Programs { get; set; } = null!;
    public DbSet<ProgramExercise> ProgramExercises { get; set; } = null!;
    public DbSet<Workout> Workouts { get; set; } = null!;
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; } = null!;
    public DbSet<Set> Sets { get; set; } = null!;

    // Automatic audit field handling
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from separate classes
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkoutTrackerDbContext).Assembly);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseModel>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
