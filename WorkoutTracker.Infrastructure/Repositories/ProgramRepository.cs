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
        return await _context.Programs.ToListAsync();
    }

    public async Task<Program?> GetByIdAsync(int id)
    {
        return await _context.Programs
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
}
