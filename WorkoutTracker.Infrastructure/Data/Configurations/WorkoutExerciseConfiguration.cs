using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        // Table name
        builder.ToTable("WorkoutExercises");

        // Primary key (explicit even though EF infers it)
        builder.HasKey(we => we.Id);

        // Properties
        builder.Property(we => we.Order)
            .IsRequired();

        // Audit fields
        builder.Property(we => we.CreatedAt)
            .IsRequired();

        builder.Property(we => we.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(we => we.Workout)
            .WithMany(w => w.WorkoutExercises)
            .HasForeignKey(we => we.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(we => we.Exercise)
            .WithMany()
            .HasForeignKey(we => we.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(we => we.Sets)
            .WithOne(s => s.WorkoutExercise)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(we => we.WorkoutId)
            .HasDatabaseName("IX_WorkoutExercises_WorkoutId");

        builder.HasIndex(we => we.ExerciseId)
            .HasDatabaseName("IX_WorkoutExercises_ExerciseId");

        builder.HasIndex(we => new { we.WorkoutId, we.ExerciseId })
            .HasDatabaseName("IX_WorkoutExercises_WorkoutId_ExerciseId")
            .IsUnique();
            
        builder.HasIndex(we => new { we.WorkoutId, we.Order })
            .HasDatabaseName("IX_WorkoutExercises_WorkoutId_Order");
    }
}
