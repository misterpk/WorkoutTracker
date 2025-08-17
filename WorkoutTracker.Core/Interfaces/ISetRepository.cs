using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface ISetRepository
    {
        Task<IEnumerable<Set>> GetAllAsync();
        Task<Set?> GetByIdAsync(int id);
        Task<Set> AddAsync(Set set);
        Task UpdateAsync(Set set);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Set>> GetByWorkoutExerciseIdAsync(int workoutExerciseId);
    }
}
