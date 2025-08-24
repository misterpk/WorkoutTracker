using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IProgramExerciseRepository
    {
        Task<IEnumerable<ProgramExercise>> GetAllAsync();
        Task<ProgramExercise?> GetByIdAsync(int id);
        Task<ProgramExercise> AddAsync(ProgramExercise programExercise);
        Task UpdateAsync(ProgramExercise programExercise);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ProgramExercise>> GetByProgramIdAsync(int programId);
        Task<IEnumerable<ProgramExercise>> GetByExerciseIdAsync(int exerciseId);
        Task<ProgramExercise?> GetByProgramAndExerciseIdAsync(int programId, int exerciseId);
    }
}

