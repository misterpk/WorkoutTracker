namespace WorkoutTracker.Core.Models
{
    public class Program
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public List<ProgramExercise> ProgramExercises { get; set; } = [];
    }
}