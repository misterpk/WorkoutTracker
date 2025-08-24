using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories;

public class SetRepository : ISetRepository
{
    private readonly WorkoutTrackerDbContext _context;

    public SetRepository(WorkoutTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Set>> GetAllAsync()
    {
        return await _context.Sets.ToListAsync();
    }

    public async Task<Set?> GetByIdAsync(int id)
    {
        return await _context.Sets
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Set> AddAsync(Set set)
    {
        _context.Sets.Add(set);
        await _context.SaveChangesAsync();
        return set;
    }

    public async Task UpdateAsync(Set set)
    {
        _context.Sets.Update(set);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var set = await GetByIdAsync(id);
        if (set != null)
        {
            _context.Sets.Remove(set);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Sets
            .AnyAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Set>> GetByWorkoutExerciseIdAsync(int workoutExerciseId)
    {
        return await _context.Sets
            .Where(s => s.WorkoutExerciseId == workoutExerciseId)
            .OrderBy(s => s.Order)  // Set 1, Set 2, Set 3...
            .Include(s => s.WorkoutExercise)
                .ThenInclude(we => we.Exercise)  // Load Exercise info for display
            .ToListAsync();
    }
}
