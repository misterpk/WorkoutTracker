using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        // Table name
        builder.ToTable("Exercises");
        
        // Primary key (explicit even though EF infers it)
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Description)
            .HasMaxLength(500);
            
        builder.Property(e => e.PrimaryMuscle)
            .IsRequired()
            .HasMaxLength(100);
            
        // Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.UpdatedAt)
            .IsRequired();
            
        // Indexes for performance
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Exercises_Name");
            
        builder.HasIndex(e => e.PrimaryMuscle)
            .HasDatabaseName("IX_Exercises_PrimaryMuscle");
    }
}