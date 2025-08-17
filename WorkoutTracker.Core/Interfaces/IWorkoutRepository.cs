using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IWorkoutRepository
    {
        Task<IEnumerable<Workout>> GetAllAsync();
        Task<Workout?> GetByIdAsync(int id);
        Task<Workout> AddAsync(Workout workout);
        Task UpdateAsync(Workout workout);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Workout>> GetByProgramIdAsync(int? programId);
    }
}
