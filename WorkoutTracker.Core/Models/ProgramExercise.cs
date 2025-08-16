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