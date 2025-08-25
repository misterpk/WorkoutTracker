using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Composers;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers
{
    public class TestEntityBuilder
    {
        private readonly WorkoutTrackerDbContext _context;

        public TestEntityBuilder(WorkoutTrackerDbContext context)
        {
            _context = context;
        }

        public ExerciseBuilder Exercise(string name = "Test Exercise", string primaryMuscle = "Test Muscle") 
            => new ExerciseBuilder(_context, name, primaryMuscle);

        public ProgramBuilder Program(string name = "Test Program") 
            => new ProgramBuilder(_context, name);

        public WorkoutBuilder Workout() 
            => new WorkoutBuilder(_context);

        public ProgramExerciseComposer ProgramExercise 
            => new ProgramExerciseComposer(_context);

        public WorkoutComposer WorkoutExercise 
            => new WorkoutComposer(_context);
    }
}