using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class SetBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly Set _set;

        public SetBuilder(WorkoutTrackerDbContext context, WorkoutExercise workoutExercise)
        {
            _context = context;
            _set = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 10,
                Weight = 100.0m,
                Order = 1,
                RPE = 8,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };
        }

        public SetBuilder WithReps(int reps)
        {
            _set.Reps = reps;
            return this;
        }

        public SetBuilder WithWeight(decimal weight)
        {
            _set.Weight = weight;
            return this;
        }

        public SetBuilder WithRPE(int rpe)
        {
            _set.RPE = rpe;
            return this;
        }

        public SetBuilder WithOrder(int order)
        {
            _set.Order = order;
            return this;
        }

        public SetBuilder WithRestTime(int restTimeSeconds)
        {
            _set.RestTimeSeconds = restTimeSeconds;
            return this;
        }

        public SetBuilder WithNotes(string notes)
        {
            _set.Notes = notes;
            return this;
        }

        public async Task<Set> SaveAsync()
        {
            _context.Sets.Add(_set);
            await _context.SaveChangesAsync();
            return _set;
        }

        public Set Build() => _set;
    }
}