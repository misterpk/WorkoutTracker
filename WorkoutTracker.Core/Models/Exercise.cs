namespace WorkoutTracker.Core.Models
{
    public class Exercise : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryMuscle { get; set; } = string.Empty;
    }
}