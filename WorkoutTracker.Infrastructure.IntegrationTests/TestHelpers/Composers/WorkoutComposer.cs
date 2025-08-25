using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Composers
{
    public class WorkoutComposer
    {
        private readonly WorkoutTrackerDbContext _context;

        public WorkoutComposer(WorkoutTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<WorkoutExercise> CreateWorkoutExerciseAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            Program? program = null,
            DateTime? workoutDate = null,
            int order = 1)
        {
            var exercise = await new ExerciseBuilder(_context, exerciseName, exerciseMuscle).SaveAsync();
            var workout = await new WorkoutBuilder(_context)
                .WithProgram(program)
                .WithDate(workoutDate ?? DateTime.UtcNow)
                .SaveAsync();

            var workoutExercise = new WorkoutExercise
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = order
            };

            _context.WorkoutExercises.Add(workoutExercise);
            await _context.SaveChangesAsync();
            return workoutExercise;
        }

        public async Task<(Workout workout, WorkoutExercise workoutExercise)> CreateWorkoutWithExerciseAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            Program? program = null,
            DateTime? workoutDate = null,
            int order = 1)
        {
            var exercise = await new ExerciseBuilder(_context, exerciseName, exerciseMuscle).SaveAsync();
            var workout = await new WorkoutBuilder(_context)
                .WithProgram(program)
                .WithDate(workoutDate ?? DateTime.UtcNow)
                .SaveAsync();

            var workoutExercise = new WorkoutExercise
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = order
            };

            _context.WorkoutExercises.Add(workoutExercise);
            await _context.SaveChangesAsync();
            
            return (workout, workoutExercise);
        }

        public async Task<Set> CreateCompleteSetAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            Program? program = null,
            int reps = 10,
            decimal weight = 100.0m,
            int rpe = 8,
            int order = 1,
            int restTimeSeconds = 120,
            string notes = "Test set notes")
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var set = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = reps,
                Weight = weight,
                Order = order,
                RPE = rpe,
                RestTimeSeconds = restTimeSeconds,
                Notes = notes
            };

            _context.Sets.Add(set);
            await _context.SaveChangesAsync();
            return set;
        }

        public async Task<(Workout workout, WorkoutExercise workoutExercise, Set set)> CreateCompleteWorkoutAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            Program? program = null,
            int reps = 10,
            decimal weight = 100.0m,
            int rpe = 8)
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var set = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = reps,
                Weight = weight,
                Order = 1,
                RPE = rpe,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };

            _context.Sets.Add(set);
            await _context.SaveChangesAsync();

            return (workout, workoutExercise, set);
        }

        public async Task<List<Set>> CreateMultipleSetsAsync(
            string exerciseName = "Test Exercise",
            string exerciseMuscle = "Test Muscle",
            Program? program = null,
            params (int reps, decimal weight, int order)[] setData)
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var sets = new List<Set>();
            foreach (var (reps, weight, order) in setData)
            {
                var set = new Set
                {
                    WorkoutExercise = workoutExercise,
                    WorkoutExerciseId = workoutExercise.Id,
                    Reps = reps,
                    Weight = weight,
                    Order = order,
                    RPE = 8,
                    RestTimeSeconds = 120,
                    Notes = "Test set notes"
                };
                sets.Add(set);
            }

            _context.Sets.AddRange(sets);
            await _context.SaveChangesAsync();
            return sets;
        }

        public class CompleteWorkoutData
        {
            public Workout Workout { get; set; } = null!;
            public List<WorkoutExercise> WorkoutExercises { get; set; } = new();
            public List<Set> Sets { get; set; } = new();
        }

        public async Task<CompleteWorkoutData> CreateMultiExerciseWorkoutAsync(
            Program? program = null,
            DateTime? workoutDate = null,
            params (string exerciseName, string muscle, int reps, decimal weight)[] exerciseData)
        {
            var workout = await new WorkoutBuilder(_context)
                .WithProgram(program)
                .WithDate(workoutDate ?? DateTime.UtcNow)
                .SaveAsync();

            var result = new CompleteWorkoutData { Workout = workout };

            for (int i = 0; i < exerciseData.Length; i++)
            {
                var (exerciseName, muscle, reps, weight) = exerciseData[i];
                
                var exercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();
                var workoutExercise = await new WorkoutExerciseBuilder(_context, workout, exercise)
                    .WithOrder(i + 1)
                    .SaveAsync();
                
                var set = await new SetBuilder(_context, workoutExercise)
                    .WithReps(reps)
                    .WithWeight(weight)
                    .SaveAsync();

                result.WorkoutExercises.Add(workoutExercise);
                result.Sets.Add(set);
            }

            return result;
        }

        public async Task<List<Workout>> CreateFreestyleWorkoutsAsync(
            params (string exerciseName, string muscle, int reps, decimal weight)[] workoutData)
        {
            var workouts = new List<Workout>();

            foreach (var (exerciseName, muscle, reps, weight) in workoutData)
            {
                var exercise = await new ExerciseBuilder(_context, exerciseName, muscle).SaveAsync();
                var workout = await new WorkoutBuilder(_context).SaveAsync();
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