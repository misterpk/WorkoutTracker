// WorkoutTracker.Infrastructure/Repositories/ProgramExerciseRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class ProgramExerciseRepository : IProgramExerciseRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public ProgramExerciseRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProgramExercise>> GetAllAsync()
    {
        return await _context.ProgramExercises.ToListAsync();
    }

    public async Task<ProgramExercise?> GetByIdAsync(int id)
    {
        return await _context.ProgramExercises
            .FirstOrDefaultAsync(pe => pe.Id == id);
    }

    public async Task<ProgramExercise> AddAsync(ProgramExercise programExercise)
    {
        _context.ProgramExercises.Add(programExercise);
        await _context.SaveChangesAsync();
        return programExercise;
    }

    public async Task UpdateAsync(ProgramExercise programExercise)
    {
        _context.ProgramExercises.Update(programExercise);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var programExercise = await GetByIdAsync(id);
        if (programExercise != null)
        {
            _context.ProgramExercises.Remove(programExercise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProgramExercises
            .AnyAsync(pe => pe.Id == id);
    }

    public async Task<IEnumerable<ProgramExercise>> GetByProgramIdAsync(int programId)
    {
        return await _context.ProgramExercises
            .Where(pe => pe.ProgramId == programId)
            .OrderBy(pe => pe.Order)  // Important: order by the Order field!
            .Include(pe => pe.Exercise)  // Load Exercise info for display
            .ToListAsync();
    }

    public async Task<IEnumerable<ProgramExercise>> GetByExerciseIdAsync(int exerciseId)
    {
        return await _context.ProgramExercises
            .Where(pe => pe.ExerciseId == exerciseId)
            .Include(pe => pe.Program)  // Load Program info for display
            .OrderBy(pe => pe.Program.Name)  // Order by program name
            .ToListAsync();
    }

    public async Task<ProgramExercise?> GetByProgramAndExerciseIdAsync(int programId, int exerciseId)
    {
        return await _context.ProgramExercises
            .Where(pe => pe.ProgramId == programId && pe.ExerciseId == exerciseId)
            .Include(pe => pe.Exercise)
            .Include(pe => pe.Program)
            .FirstOrDefaultAsync();
    }
}

