using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Composers
{
    public class ProgramExerciseComposer
    {
        private readonly WorkoutTrackerDbContext _context;

        public ProgramExerciseComposer(WorkoutTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<ProgramExercise> CreateAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            string programName = "Test Program",
            int order = 1,
            int plannedSets = 3,
            int plannedReps = 10,
            decimal plannedWeight = 100.0m,
            int plannedRestTimeSeconds = 120)
        {
            var exercise = await new ExerciseBuilder(_context, exerciseName, exerciseMuscle).SaveAsync();
            var program = await new ProgramBuilder(_context, programName).SaveAsync();
            
            var programExercise = new ProgramExercise
            {
                Program = program,
                ProgramId = program.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = order,
                PlannedSets = plannedSets,
                PlannedReps = plannedReps,
                PlannedWeight = plannedWeight,
                PlannedRestTimeSeconds = plannedRestTimeSeconds
            };

            _context.ProgramExercises.Add(programExercise);
            await _context.SaveChangesAsync();
            return programExercise;
        }

        public async Task<(Program program, Exercise exercise, ProgramExercise programExercise)> CreateCompleteAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle", 
            string programName = "Test Program",
            int order = 1,
            int plannedSets = 3,
            int plannedReps = 10,
            decimal plannedWeight = 100.0m)
        {
            var exercise = await new ExerciseBuilder(_context, exerciseName, exerciseMuscle).SaveAsync();
            var program = await new ProgramBuilder(_context, programName).SaveAsync();
            
            var programExercise = new ProgramExercise
            {
                Program = program,
                ProgramId = program.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = order,
                PlannedSets = plannedSets,
                PlannedReps = plannedReps,
                PlannedWeight = plannedWeight,
                PlannedRestTimeSeconds = 120
            };

            _context.ProgramExercises.Add(programExercise);
            await _context.SaveChangesAsync();
            
            return (program, exercise, programExercise);
        }

        public async Task<List<ProgramExercise>> CreateMultipleAsync(
            string programName,
            params (string exerciseName, string muscle, int order)[] exercises)
        {
            var program = await new ProgramBuilder(_context, programName).SaveAsync();
            var programExercises = new List<ProgramExercise>();

            foreach (var (exerciseName, muscle, order) in exercises)
            {
                var exercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();
                var programExercise = new ProgramExercise
                {
                    Program = program,
                    ProgramId = program.Id,
                    Exercise = exercise,
                    ExerciseId = exercise.Id,
                    Order = order,
                    PlannedSets = 3,
                    PlannedReps = 10,
                    PlannedWeight = 100.0m,
                    PlannedRestTimeSeconds = 120
                };
                programExercises.Add(programExercise);
            }

            _context.ProgramExercises.AddRange(programExercises);
            await _context.SaveChangesAsync();
            return programExercises;
        }
    }
}