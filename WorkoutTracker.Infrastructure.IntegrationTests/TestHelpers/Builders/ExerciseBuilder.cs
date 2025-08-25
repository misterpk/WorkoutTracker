using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class ExerciseBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly Exercise _exercise;

        public ExerciseBuilder(WorkoutTrackerDbContext context, string name = TestConstants.DefaultExerciseName, string primaryMuscle = TestConstants.DefaultMuscleGroup)
        {
            _context = context;
            _exercise = new Exercise
            {
                Name = name,
                PrimaryMuscle = primaryMuscle,
                Description = "Test Description"
            };
        }

        public ExerciseBuilder WithName(string name)
        {
            _exercise.Name = name;
            return this;
        }

        public ExerciseBuilder WithPrimaryMuscle(string muscle)
        {
            _exercise.PrimaryMuscle = muscle;
            return this;
        }

        public ExerciseBuilder WithDescription(string description)
        {
            _exercise.Description = description;
            return this;
        }

        public async Task<Exercise> SaveAsync()
        {
            _context.Exercises.Add(_exercise);
            await _context.SaveChangesAsync();
            return _exercise;
        }

        public Exercise Build() => _exercise;
    }
}