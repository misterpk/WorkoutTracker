using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IProgramRepository
    {
        Task<IEnumerable<Program>> GetAllAsync();
        Task<Program?> GetByIdAsync(int id);
        Task<Program> AddAsync(Program program);
        Task UpdateAsync(Program program);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}