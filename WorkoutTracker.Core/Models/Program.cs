namespace WorkoutTracker.Core.Models
{
    public class Program : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ProgramExercise> ProgramExercises { get; set; } = null!;
        public ICollection<Workout> Workouts { get; set; } = null!;
    }
}
