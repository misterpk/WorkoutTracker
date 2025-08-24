using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class WorkoutExerciseRepository : IWorkoutExerciseRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public WorkoutExerciseRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkoutExercise>> GetAllAsync()
    {
        return await _context.WorkoutExercises.ToListAsync();
    }

    public async Task<WorkoutExercise?> GetByIdAsync(int id)
    {
        return await _context.WorkoutExercises
            .FirstOrDefaultAsync(we => we.Id == id);
    }

    public async Task<WorkoutExercise> AddAsync(WorkoutExercise workoutExercise)
    {
        _context.WorkoutExercises.Add(workoutExercise);
        await _context.SaveChangesAsync();
        return workoutExercise;
    }

    public async Task UpdateAsync(WorkoutExercise workoutExercise)
    {
        _context.WorkoutExercises.Update(workoutExercise);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workoutExercise = await GetByIdAsync(id);
        if (workoutExercise != null)
        {
            _context.WorkoutExercises.Remove(workoutExercise);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.WorkoutExercises
          .AnyAsync(we => we.Id == id);
    }

    public async Task<IEnumerable<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId)
    {
        return await _context.WorkoutExercises
          .Where(we => we.WorkoutId == workoutId)
          .OrderBy(we => we.Order)
          .Include(we => we.Exercise)
          .ToListAsync();
    }

    public async Task<IEnumerable<WorkoutExercise>> GetByExerciseIdAsync(int exerciseId)
    {
        return await _context.WorkoutExercises
          .Where(we => we.ExerciseId == exerciseId)
          .Include(we => we.Workout)
          .OrderBy(we => we.WorkoutId)
          .ToListAsync();
    }

    public async Task<WorkoutExercise?> GetByWorkoutAndExerciseIdAsync(int workoutId, int exerciseId)
    {
        return await _context.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId && we.ExerciseId == exerciseId)
            .Include(we => we.Exercise)
            .Include(we => we.Workout)
            .Include(we => we.Sets)  // Include Sets since you'll probably need them
            .FirstOrDefaultAsync();
    }
}
