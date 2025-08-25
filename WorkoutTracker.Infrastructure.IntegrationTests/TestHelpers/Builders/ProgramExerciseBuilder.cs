using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class ProgramExerciseBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly ProgramExercise _programExercise;

        public ProgramExerciseBuilder(WorkoutTrackerDbContext context, Program program, Exercise exercise)
        {
            _context = context;
            _programExercise = new ProgramExercise
            {
                Program = program,
                ProgramId = program.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = 1,
                PlannedSets = 3,
                PlannedReps = 10,
                PlannedWeight = 100.0m,
                PlannedRestTimeSeconds = 120
            };
        }

        public ProgramExerciseBuilder WithOrder(int order)
        {
            _programExercise.Order = order;
            return this;
        }

        public ProgramExerciseBuilder WithPlannedSets(int sets)
        {
            _programExercise.PlannedSets = sets;
            return this;
        }

        public ProgramExerciseBuilder WithPlannedReps(int reps)
        {
            _programExercise.PlannedReps = reps;
            return this;
        }

        public ProgramExerciseBuilder WithPlannedWeight(decimal weight)
        {
            _programExercise.PlannedWeight = weight;
            return this;
        }

        public ProgramExerciseBuilder WithPlannedRestTime(int restTimeSeconds)
        {
            _programExercise.PlannedRestTimeSeconds = restTimeSeconds;
            return this;
        }

        public async Task<ProgramExercise> SaveAsync()
        {
            _context.ProgramExercises.Add(_programExercise);
            await _context.SaveChangesAsync();
            return _programExercise;
        }

        public ProgramExercise Build() => _programExercise;
    }
}