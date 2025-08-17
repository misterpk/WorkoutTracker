namespace WorkoutTracker.Core.Models
{
    public class WorkoutExercise : BaseModel
    {
        public int WorkoutId { get; set; }
        public int ExerciseId { get; set; }
        public int Order { get; set; }
        public Workout Workout { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public List<Set> Sets { get; set; } = [];
    }
}