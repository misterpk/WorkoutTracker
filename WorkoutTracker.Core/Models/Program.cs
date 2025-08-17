namespace WorkoutTracker.Core.Models
{
    public class Program : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ProgramExercise> ProgramExercises { get; set; } = [];
    }
}