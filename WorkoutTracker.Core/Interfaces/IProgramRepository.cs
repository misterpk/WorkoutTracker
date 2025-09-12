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
        Task<Program?> GetByNameAsync(string name);
        Task<bool> HasWorkoutsAsync(int programId);
        Task<Program> CopyAsync(int sourceProgramId, string newName);
        Task<ProgramExercise> AddExerciseAsync(ProgramExercise programExercise);
        Task UpdateProgramExerciseAsync(ProgramExercise programExercise);
        Task DeleteProgramExerciseAsync(int programExerciseId);
        Task<ProgramExercise?> GetProgramExerciseByIdAsync(int programExerciseId);
        Task ReorderExercisesAsync(int programId, List<int> exerciseIds);
    }
}

