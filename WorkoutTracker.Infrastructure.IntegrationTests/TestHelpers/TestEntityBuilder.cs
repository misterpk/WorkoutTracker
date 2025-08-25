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

        public ExerciseBuilder Exercise(string name = TestConstants.DefaultExerciseName, string primaryMuscle = TestConstants.DefaultMuscleGroup) 
            => new ExerciseBuilder(_context, name, primaryMuscle);

        public ProgramBuilder Program(string name = TestConstants.DefaultProgramName) 
            => new ProgramBuilder(_context, name);

        public WorkoutBuilder Workout() 
            => new WorkoutBuilder(_context);

        public ProgramExerciseComposer ProgramExercise 
            => new ProgramExerciseComposer(_context);

        public WorkoutComposer WorkoutComposer 
            => new WorkoutComposer(_context);
    }
}