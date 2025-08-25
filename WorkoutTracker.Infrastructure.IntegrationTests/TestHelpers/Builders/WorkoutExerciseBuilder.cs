using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class WorkoutExerciseBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly WorkoutExercise _workoutExercise;

        public WorkoutExerciseBuilder(WorkoutTrackerDbContext context, Workout workout, Exercise exercise)
        {
            _context = context;
            _workoutExercise = new WorkoutExercise
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = TestConstants.DefaultOrder
            };
        }

        public WorkoutExerciseBuilder WithOrder(int order)
        {
            _workoutExercise.Order = order;
            return this;
        }

        public async Task<WorkoutExercise> SaveAsync()
        {
            _context.WorkoutExercises.Add(_workoutExercise);
            await _context.SaveChangesAsync();
            return _workoutExercise;
        }

        public WorkoutExercise Build() => _workoutExercise;
    }
}