namespace WorkoutTracker.Core.Models
{
    public class Workout : BaseModel
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int? ProgramId { get; set; }
        public bool IsModifiedFromProgram { get; set; } = false;
        public Program? Program { get; set; }  // Nullable since freestyle workouts exist
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = null!;
    }
}
