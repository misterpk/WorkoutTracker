using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class ProgramExerciseConfiguration : IEntityTypeConfiguration<ProgramExercise>
{
    public void Configure(EntityTypeBuilder<ProgramExercise> builder)
    {
        // Table name
        builder.ToTable("ProgramExercises");

        // Primary key (explicit even though EF infers it)
        builder.HasKey(pe => pe.Id);

        // Properties
        builder.HasOne(pe => pe.Program)
            .WithMany(p => p.ProgramExercises)
            .HasForeignKey(pe => pe.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.Exercise)
            .WithMany(e => e.ProgramExercises)
            .HasForeignKey(pe => pe.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(pe => pe.Order)
            .IsRequired();

        builder.Property(pe => pe.PlannedSets);

        builder.Property(pe => pe.PlannedReps);

        builder.Property(pe => pe.PlannedWeight)
            .HasColumnType("decimal(5,2)");

        builder.Property(pe => pe.PlannedRestTimeSeconds);

        // Audit fields
        builder.Property(pe => pe.CreatedAt)
            .IsRequired();

        builder.Property(pe => pe.UpdatedAt)
            .IsRequired();

        // Index for performance
        builder.HasIndex(pe => new { pe.ProgramId, pe.ExerciseId })
            .HasDatabaseName("IX_ProgramExercises_ProgramId_ExerciseId")
            .IsUnique();

        builder.HasIndex(pe => pe.ProgramId)
            .HasDatabaseName("IX_ProgramExercises_ProgramId");
            
        builder.HasIndex(pe => pe.ExerciseId)
            .HasDatabaseName("IX_ProgramExercises_ExerciseId");
    }
}
