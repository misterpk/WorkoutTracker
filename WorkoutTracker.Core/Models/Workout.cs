namespace WorkoutTracker.Core.Models
{
    public class Workout : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int? ProgramId { get; set; }
        public bool IsModifiedFromProgram { get; set; } = false;
        public Program? Program { get; set; } = null!;  // Nullable since freestyle workouts exist
        public List<WorkoutExercise> WorkoutExercises { get; set; } = [];
    }
}