// Id (int) - primary key
// ProgramId (int) - foreign key to Program
// ExerciseId (int) - foreign key to Exercise
// Order (int) - sequence in the program (1st, 2nd, 3rd exercise)
// PlannedSets (int) - how many sets planned
// PlannedReps (int) - target reps per set
// PlannedWeight (float) - target weight
// PlannedRestTimeSeconds (int) - target rest time between sets in seconds
namespace WorkoutTracker.Core.Models
{
    public class ProgramExercise
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }
        public int ExerciseId { get; set; }
        public int Order { get; set; }
        public int PlannedSets { get; set; }
        public int PlannedReps { get; set; }
        public float PlannedWeight { get; set; }
        public int PlannedRestTimeSeconds { get; set; }
        public Program Program { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
    }
}