using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        // Table name
        builder.ToTable("Workouts");

        // Primary key (explicit even though EF infers it)
        builder.HasKey(w => w.Id);

        // Properties
        builder.Property(w => w.Date)
            .IsRequired();

        builder.Property(w => w.IsModifiedFromProgram)
            .IsRequired();

        // Audit fields
        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();

        // Associations
        builder.HasOne(w => w.Program)
            .WithMany(p => p.Workouts)
            .HasForeignKey(w => w.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for performance
        builder.HasIndex(w => w.Date)
            .HasDatabaseName("IX_Workouts_Date");
    }
}
