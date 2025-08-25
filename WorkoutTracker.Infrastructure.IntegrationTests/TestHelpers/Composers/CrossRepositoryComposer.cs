using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Composers
{
    public class CrossRepositoryComposer
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly ExerciseRepository _exerciseRepository;
        private readonly ProgramRepository _programRepository;
        private readonly ProgramExerciseRepository _programExerciseRepository;
        private readonly WorkoutRepository _workoutRepository;
        private readonly WorkoutExerciseRepository _workoutExerciseRepository;
        private readonly SetRepository _setRepository;

        public CrossRepositoryComposer(WorkoutTrackerDbContext context)
        {
            _context = context;
            _exerciseRepository = new ExerciseRepository(context);
            _programRepository = new ProgramRepository(context);
            _programExerciseRepository = new ProgramExerciseRepository(context);
            _workoutRepository = new WorkoutRepository(context);
            _workoutExerciseRepository = new WorkoutExerciseRepository(context);
            _setRepository = new SetRepository(context);
        }

        public class CompleteWorkoutFlow
        {
            public Program Program { get; set; } = null!;
            public List<Exercise> Exercises { get; set; } = new();
            public List<ProgramExercise> ProgramExercises { get; set; } = new();
            public Workout Workout { get; set; } = null!;
            public List<WorkoutExercise> WorkoutExercises { get; set; } = new();
            public List<Set> Sets { get; set; } = new();
        }

        public async Task<CompleteWorkoutFlow> CreateCompleteWorkoutFlowAsync(
            string programName,
            params (string exerciseName, string muscle, int order, int reps, decimal weight)[] exerciseData)
        {
            var result = new CompleteWorkoutFlow();

            // Create program
            result.Program = await new ProgramBuilder(_context, programName).SaveAsync();

            // Create exercises
            foreach (var (exerciseName, muscle, _, _, _) in exerciseData)
            {
                var exercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();
                result.Exercises.Add(exercise);
            }

            // Create program exercises using repository
            for (int i = 0; i < exerciseData.Length; i++)
            {
                var (_, _, order, _, _) = exerciseData[i];
                var programExercise = await new ProgramExerciseBuilder(_context, result.Program, result.Exercises[i])
                    .WithOrder(order)
                    .SaveAsync();
                result.ProgramExercises.Add(programExercise);
            }

            // Create workout
            result.Workout = await new WorkoutBuilder(_context).WithProgram(result.Program).SaveAsync();

            // Create workout exercises using repository
            for (int i = 0; i < result.Exercises.Count; i++)
            {
                var workoutExercise = await new WorkoutExerciseBuilder(_context, result.Workout, result.Exercises[i])
                    .WithOrder(exerciseData[i].order)
                    .SaveAsync();
                result.WorkoutExercises.Add(workoutExercise);
            }

            // Create sets using repository
            for (int i = 0; i < result.WorkoutExercises.Count; i++)
            {
                var (_, _, _, reps, weight) = exerciseData[i];
                var set = await new SetBuilder(_context, result.WorkoutExercises[i])
                    .WithReps(reps)
                    .WithWeight(weight)
                    .SaveAsync();
                result.Sets.Add(set);
            }

            return result;
        }

        public async Task<List<Workout>> CreateProgressiveWorkoutsAsync(
            string exerciseName,
            string muscle,
            string programName,
            params (DateTime date, int reps, decimal weight)[] progressionData)
        {
            // Create program and exercise
            var (program, exercise, _) = await new ProgramExerciseComposer(_context).CreateCompleteAsync(
                exerciseName, muscle, programName);

            var workouts = new List<Workout>();

            foreach (var (date, reps, weight) in progressionData)
            {
                // Create workout
                var workout = await new WorkoutBuilder(_context)
                    .WithProgram(program)
                    .WithDate(date)
                    .SaveAsync();
                
                // Create workout exercise
                var workoutExercise = await new WorkoutExerciseBuilder(_context, workout, exercise).SaveAsync();
                
                // Create set
                await new SetBuilder(_context, workoutExercise)
                    .WithReps(reps)
                    .WithWeight(weight)
                    .SaveAsync();

                workouts.Add(workout);
            }

            return workouts;
        }

        public async Task<(Exercise sharedExercise, List<Program> programs, List<ProgramExercise> programExercises)> 
            CreateSharedExerciseScenarioAsync(string exerciseName, string muscle, params string[] programNames)
        {
            // Create shared exercise
            var sharedExercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();

            var programs = new List<Program>();
            var programExercises = new List<ProgramExercise>();

            // Create programs and program exercises that share the same exercise
            foreach (var programName in programNames)
            {
                var program = await new ProgramBuilder(_context, programName).SaveAsync();
                programs.Add(program);

                var programExercise = await new ProgramExerciseBuilder(_context, program, sharedExercise)
                    .WithOrder(1)
                    .SaveAsync();
                programExercises.Add(programExercise);
            }

            return (sharedExercise, programs, programExercises);
        }

        public async Task<List<Workout>> CreateFreestyleWorkoutsAsync(
            params (string exerciseName, string muscle, int reps, decimal weight)[] workoutData)
        {
            var workouts = new List<Workout>();

            foreach (var (exerciseName, muscle, reps, weight) in workoutData)
            {
                var exercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();
                var workout = await new WorkoutBuilder(_context).SaveAsync(); // No program - freestyle
                var workoutExercise = await new WorkoutExerciseBuilder(_context, workout, exercise).SaveAsync();
                await new SetBuilder(_context, workoutExercise)
                    .WithReps(reps)
                    .WithWeight(weight)
                    .SaveAsync();

                workouts.Add(workout);
            }

            return workouts;
        }
    }
}