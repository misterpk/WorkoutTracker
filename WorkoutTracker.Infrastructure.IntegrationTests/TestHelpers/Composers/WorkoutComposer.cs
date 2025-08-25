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

        private async Task<(Exercise exercise, Workout workout)> CreateExerciseAndWorkoutAsync(
            string exerciseName, 
            string exerciseMuscle, 
            Program? program = null, 
            DateTime? workoutDate = null)
        {
            var exercise = await new ExerciseBuilder(_context, exerciseName, exerciseMuscle).SaveAsync();
            var workout = await new WorkoutBuilder(_context)
                .WithProgram(program)
                .WithDate(workoutDate ?? DateTime.UtcNow)
                .SaveAsync();
            
            return (exercise, workout);
        }

        public async Task<WorkoutExercise> CreateWorkoutExerciseAsync(
            string exerciseName = TestConstants.DefaultExerciseName,
            string exerciseMuscle = TestConstants.DefaultMuscleGroup,
            Program? program = null,
            DateTime? workoutDate = null,
            int order = TestConstants.DefaultOrder)
        {
            var (exercise, workout) = await CreateExerciseAndWorkoutAsync(exerciseName, exerciseMuscle, program, workoutDate);

            var workoutExercise = await new WorkoutExerciseBuilder(_context, workout, exercise)
                .WithOrder(order)
                .SaveAsync();

            return workoutExercise;
        }

        public async Task<(Workout workout, WorkoutExercise workoutExercise)> CreateWorkoutWithExerciseAsync(
            string exerciseName = TestConstants.DefaultExerciseName,
            string exerciseMuscle = TestConstants.DefaultMuscleGroup,
            Program? program = null,
            DateTime? workoutDate = null,
            int order = TestConstants.DefaultOrder)
        {
            var (exercise, workout) = await CreateExerciseAndWorkoutAsync(exerciseName, exerciseMuscle, program, workoutDate);

            var workoutExercise = await new WorkoutExerciseBuilder(_context, workout, exercise)
                .WithOrder(order)
                .SaveAsync();
            
            return (workout, workoutExercise);
        }

        public async Task<Set> CreateCompleteSetAsync(
            string exerciseName = TestConstants.DefaultExerciseName,
            string exerciseMuscle = TestConstants.DefaultMuscleGroup,
            Program? program = null,
            int reps = TestConstants.DefaultReps,
            decimal weight = TestConstants.DefaultWeight,
            int rpe = TestConstants.DefaultRpe,
            int order = TestConstants.DefaultOrder,
            int restTimeSeconds = TestConstants.DefaultRestTimeSeconds,
            string notes = TestConstants.DefaultNotes)
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var set = await new SetBuilder(_context, workoutExercise)
                .WithReps(reps)
                .WithWeight(weight)
                .WithOrder(order)
                .WithRPE(rpe)
                .WithRestTime(restTimeSeconds)
                .WithNotes(notes)
                .SaveAsync();

            return set;
        }

        public async Task<(Workout workout, WorkoutExercise workoutExercise, Set set)> CreateCompleteWorkoutAsync(
            string exerciseName = TestConstants.DefaultExerciseName,
            string exerciseMuscle = TestConstants.DefaultMuscleGroup,
            Program? program = null,
            int reps = TestConstants.DefaultReps,
            decimal weight = TestConstants.DefaultWeight,
            int rpe = TestConstants.DefaultRpe)
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var set = await new SetBuilder(_context, workoutExercise)
                .WithReps(reps)
                .WithWeight(weight)
                .WithOrder(TestConstants.DefaultOrder)
                .WithRPE(rpe)
                .WithRestTime(TestConstants.DefaultRestTimeSeconds)
                .WithNotes(TestConstants.DefaultNotes)
                .SaveAsync();

            return (workout, workoutExercise, set);
        }

        public async Task<List<Set>> CreateMultipleSetsAsync(
            string exerciseName = TestConstants.DefaultExerciseName,
            string exerciseMuscle = TestConstants.DefaultMuscleGroup,
            Program? program = null,
            params (int reps, decimal weight, int order)[] setData)
        {
            var (workout, workoutExercise) = await CreateWorkoutWithExerciseAsync(
                exerciseName, exerciseMuscle, program);

            var sets = new List<Set>();
            foreach (var (reps, weight, order) in setData)
            {
                var set = await new SetBuilder(_context, workoutExercise)
                    .WithReps(reps)
                    .WithWeight(weight)
                    .WithOrder(order)
                    .WithRPE(TestConstants.DefaultRpe)
                    .WithRestTime(TestConstants.DefaultRestTimeSeconds)
                    .WithNotes(TestConstants.DefaultNotes)
                    .SaveAsync();
                sets.Add(set);
            }

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