// WorkoutTracker.Infrastructure/Repositories/ExerciseRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public ExerciseRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        return await _context.Exercises
            .ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Exercise?> GetByNameAsync(string name)
    {
        return await _context.Exercises
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<Exercise> AddAsync(Exercise exercise)
    {
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return exercise; // EF Core will have set the Id
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        _context.Exercises.Update(exercise);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var exercise = await GetByIdAsync(id);
        if (exercise != null)
        {
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Exercise>> GetByPrimaryMuscleAsync(string primaryMuscle)
    {
        return await _context.Exercises
            .Where(e => e.PrimaryMuscle == primaryMuscle)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Exercises
            .AnyAsync(e => e.Id == id);
    }
}
