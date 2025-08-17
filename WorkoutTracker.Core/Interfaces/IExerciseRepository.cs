using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IExerciseRepository
    {
        Task<IEnumerable<Exercise>> GetAllAsync();
        Task<Exercise?> GetByIdAsync(int id);
        Task<Exercise> AddAsync(Exercise exercise);
        Task UpdateAsync(Exercise exercise);
        Task DeleteAsync(int id);
        Task<IEnumerable<Exercise>> GetByPrimaryMuscleAsync(string primaryMuscle);
        Task<bool> ExistsAsync(int id);
    }
}