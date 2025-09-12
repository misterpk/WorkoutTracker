using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class ProgramRepository : IProgramRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public ProgramRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Program>> GetAllAsync()
    {
        return await _context.Programs
            .Include(p => p.ProgramExercises)
                .ThenInclude(pe => pe.Exercise)
            .ToListAsync();
    }

    public async Task<Program?> GetByIdAsync(int id)
    {
        return await _context.Programs
            .Include(p => p.ProgramExercises)
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Program?> GetByNameAsync(string name)
    {
        return await _context.Programs
            .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<Program> AddAsync(Program program)
    {
        _context.Programs.Add(program);
        await _context.SaveChangesAsync();
        return program;
    }

    public async Task UpdateAsync(Program program)
    {
        _context.Programs.Update(program);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var program = await GetByIdAsync(id);
        if (program != null)
        {
            _context.Programs.Remove(program);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Programs
            .AnyAsync(p => p.Id == id);
    }

    public async Task<bool> HasWorkoutsAsync(int programId)
    {
        return await _context.Workouts
            .AnyAsync(w => w.ProgramId == programId);
    }

    public async Task<Program> CopyAsync(int sourceProgramId, string newName)
    {
        var sourceProgram = await GetByIdAsync(sourceProgramId);
        if (sourceProgram == null)
            throw new InvalidOperationException($"Program with ID {sourceProgramId} not found");

        var newProgram = new Program
        {
            Name = newName,
            Description = sourceProgram.Description
        };

        var savedProgram = await AddAsync(newProgram);

        // Copy program exercises separately to ensure proper relationship setup
        foreach (var pe in sourceProgram.ProgramExercises)
        {
            var newProgramExercise = new ProgramExercise
            {
                ProgramId = savedProgram.Id,
                ExerciseId = pe.ExerciseId,
                Order = pe.Order,
                PlannedSets = pe.PlannedSets,
                PlannedReps = pe.PlannedReps,
                PlannedWeight = pe.PlannedWeight,
                PlannedRestTimeSeconds = pe.PlannedRestTimeSeconds
            };
            _context.ProgramExercises.Add(newProgramExercise);
        }

        await _context.SaveChangesAsync();
        return savedProgram;
    }

    public async Task<ProgramExercise> AddExerciseAsync(ProgramExercise programExercise)
    {
        _context.ProgramExercises.Add(programExercise);
        await _context.SaveChangesAsync();
        return programExercise;
    }

    public async Task UpdateProgramExerciseAsync(ProgramExercise programExercise)
    {
        _context.ProgramExercises.Update(programExercise);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProgramExerciseAsync(int programExerciseId)
    {
        var programExercise = await _context.ProgramExercises.FindAsync(programExerciseId);
        if (programExercise != null)
        {
            _context.ProgramExercises.Remove(programExercise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ProgramExercise?> GetProgramExerciseByIdAsync(int programExerciseId)
    {
        return await _context.ProgramExercises
            .Include(pe => pe.Exercise)
            .Include(pe => pe.Program)
            .FirstOrDefaultAsync(pe => pe.Id == programExerciseId);
    }

    public async Task ReorderExercisesAsync(int programId, List<int> exerciseIds)
    {
        var programExercises = await _context.ProgramExercises
            .Where(pe => pe.ProgramId == programId)
            .ToListAsync();

        for (int i = 0; i < exerciseIds.Count; i++)
        {
            var exercise = programExercises.FirstOrDefault(pe => pe.Id == exerciseIds[i]);
            if (exercise != null)
            {
                exercise.Order = i + 1;
            }
        }

        await _context.SaveChangesAsync();
    }
}
