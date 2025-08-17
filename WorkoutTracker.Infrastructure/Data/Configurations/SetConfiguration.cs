using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class SetConfiguration : IEntityTypeConfiguration<Set>
{
    public void Configure(EntityTypeBuilder<Set> builder)
    {
        // Table name
        builder.ToTable("Sets");
        
        // Primary key (explicit even though EF infers it)
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Reps)
            .IsRequired();
            
        builder.Property(s => s.Weight)
            .IsRequired()
            .HasColumnType("decimal(5,2)"); // Precision for weight values
            
        builder.Property(s => s.Order)
            .IsRequired();
            
        builder.Property(s => s.Notes)
            .HasMaxLength(500);
            
        builder.Property(s => s.RestTimeSeconds);
            
        builder.Property(s => s.RPE);
            
        // Audit fields
        builder.Property(s => s.CreatedAt)
            .IsRequired();
            
        builder.Property(s => s.UpdatedAt)
            .IsRequired();
            
        // Relationships
        builder.HasOne(s => s.WorkoutExercise)
            .WithMany(we => we.Sets)
            .HasForeignKey(s => s.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes for performance
        builder.HasIndex(s => s.WorkoutExerciseId)
            .HasDatabaseName("IX_Sets_WorkoutExerciseId");
            
        builder.HasIndex(s => new { s.WorkoutExerciseId, s.Order })
            .HasDatabaseName("IX_Sets_WorkoutExerciseId_Order")
            .IsUnique();
    }
}
