using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.Data.Configurations;

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        // Table name
        builder.ToTable("Programs");
        
        // Primary key (explicit even though EF infers it)
        builder.HasKey(p => p.Id);
        
        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.Description)
            .HasMaxLength(500);
            
        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();
            
        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasMany(p => p.ProgramExercises)
            .WithOne(pe => pe.Program)
            .HasForeignKey(pe => pe.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Index for performance
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Programs_Name");
    }
}

