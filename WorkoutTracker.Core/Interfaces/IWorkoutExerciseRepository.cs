using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IWorkoutExerciseRepository
    {
        Task<IEnumerable<WorkoutExercise>> GetAllAsync();
        Task<WorkoutExercise?> GetByIdAsync(int id);
        Task<WorkoutExercise> AddAsync(WorkoutExercise workoutExercise);
        Task UpdateAsync(WorkoutExercise workoutExercise);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId);
        Task<IEnumerable<WorkoutExercise>> GetByExerciseIdAsync(int exerciseId);
        Task<WorkoutExercise?> GetByWorkoutAndExerciseIdAsync(int workoutId, int exerciseId);
    }
}
