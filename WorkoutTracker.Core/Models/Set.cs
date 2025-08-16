namespace WorkoutTracker.Core.Models
{
    public class Set
    {
        public int Id { get; set; }
        public int WorkoutExerciseId { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; } // 0 indicates bodyweight
        public int Order { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int RestTimeSeconds { get; set; }
        public int RPE { get; set; } // Rate of Perceived Exertion, scale 1-10
        public WorkoutExercise WorkoutExercise { get; set; } = null!;
    }
}