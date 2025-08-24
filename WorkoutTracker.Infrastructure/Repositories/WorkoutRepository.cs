// WorkoutTracker.Infrastructure/Repositories/WorkoutRepository.cs
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public WorkoutRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Workout>> GetAllAsync()
    {
        return await _context.Workouts.ToListAsync();
    }

    public async Task<Workout?> GetByIdAsync(int id)
    {
        return await _context.Workouts
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workout> AddAsync(Workout workout)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();
        return workout;
    }

    public async Task UpdateAsync(Workout workout)
    {
        _context.Workouts.Update(workout);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workout = await GetByIdAsync(id);
        if (workout != null)
        {
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Workouts
            .AnyAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Workout>> GetByProgramIdAsync(int? programId)
    {
        return await _context.Workouts
            .Where(w => w.ProgramId == programId)
            .OrderByDescending(w => w.Date)  // Most recent workouts first
            .ToListAsync();
    }

    public async Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Workouts
            .Where(w => w.Date >= startDate && w.Date <= endDate)
            .OrderByDescending(w => w.Date)  // Most recent first
            .ToListAsync();
    }
}

