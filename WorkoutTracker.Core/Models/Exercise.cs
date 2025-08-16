namespace WorkoutTracker.Core.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryMuscle { get; set; } = string.Empty;
    }
}