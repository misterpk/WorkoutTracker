using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class CrossRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly ProgramRepository _programRepository;
        private readonly ProgramExerciseRepository _programExerciseRepository;
        private readonly WorkoutRepository _workoutRepository;
        private readonly WorkoutExerciseRepository _workoutExerciseRepository;
        private readonly SetRepository _setRepository;

        public CrossRepositoryIntegrationTests()
        {
            _exerciseRepository = new ExerciseRepository(Context);
            _programRepository = new ProgramRepository(Context);
            _programExerciseRepository = new ProgramExerciseRepository(Context);
            _workoutRepository = new WorkoutRepository(Context);
            _workoutExerciseRepository = new WorkoutExerciseRepository(Context);
            _setRepository = new SetRepository(Context);
        }

        [Fact]
        public async Task CompleteWorkoutFlow_ProgramToWorkoutToSets_AllDataPersisted()
        {
            // Create exercise and program first
            var exercise1 = await Builder.Exercise("Bench Press", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Squat", "Legs").SaveAsync();
            var program = await Builder.Program("Push Pull Program").SaveAsync();
            
            // Create program exercises using repository
            var programExercise1 = new ProgramExercise
            {
                Program = program, ProgramId = program.Id, Exercise = exercise1, ExerciseId = exercise1.Id,
                Order = 1, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100.0m, PlannedRestTimeSeconds = 120
            };
            var programExercise2 = new ProgramExercise
            {
                Program = program, ProgramId = program.Id, Exercise = exercise2, ExerciseId = exercise2.Id,
                Order = 2, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100.0m, PlannedRestTimeSeconds = 120
            };
            await _programExerciseRepository.AddAsync(programExercise1);
            await _programExerciseRepository.AddAsync(programExercise2);

            // Create workout and workout exercises
            var workout = await Builder.Workout().WithProgram(program).SaveAsync();
            var workoutExercise1 = new WorkoutExercise
            {
                Workout = workout, WorkoutId = workout.Id, Exercise = exercise1, ExerciseId = exercise1.Id, Order = 1
            };
            var workoutExercise2 = new WorkoutExercise
            {
                Workout = workout, WorkoutId = workout.Id, Exercise = exercise2, ExerciseId = exercise2.Id, Order = 2
            };
            await _workoutExerciseRepository.AddAsync(workoutExercise1);
            await _workoutExerciseRepository.AddAsync(workoutExercise2);

            // Create sets
            var set1 = new Set { WorkoutExercise = workoutExercise1, WorkoutExerciseId = workoutExercise1.Id, Reps = 10, Weight = 135.0m, Order = 1, RPE = 8, RestTimeSeconds = 120, Notes = "Test set notes" };
            var set2 = new Set { WorkoutExercise = workoutExercise1, WorkoutExerciseId = workoutExercise1.Id, Reps = 8, Weight = 140.0m, Order = 2, RPE = 8, RestTimeSeconds = 120, Notes = "Test set notes" };
            var set3 = new Set { WorkoutExercise = workoutExercise2, WorkoutExerciseId = workoutExercise2.Id, Reps = 12, Weight = 185.0m, Order = 1, RPE = 8, RestTimeSeconds = 120, Notes = "Test set notes" };
            await _setRepository.AddAsync(set1);
            await _setRepository.AddAsync(set2);
            await _setRepository.AddAsync(set3);

            var completeWorkout = await Context.Workouts
                .Include(w => w.Program)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Sets)
                .FirstOrDefaultAsync(w => w.Id == workout.Id);

            completeWorkout.Should().NotBeNull();
            completeWorkout!.Program.Should().NotBeNull();
            completeWorkout.Program!.Name.Should().Be("Push Pull Program");
            completeWorkout.WorkoutExercises.Should().HaveCount(2);

            var benchPressWorkoutExercise = completeWorkout.WorkoutExercises
                .First(we => we.Exercise.Name == "Bench Press");
            benchPressWorkoutExercise.Sets.Should().HaveCount(2);
            benchPressWorkoutExercise.Sets.Should().Contain(s => s.Weight == 135.0m);
            benchPressWorkoutExercise.Sets.Should().Contain(s => s.Weight == 140.0m);

            var squatWorkoutExercise = completeWorkout.WorkoutExercises
                .First(we => we.Exercise.Name == "Squat");
            squatWorkoutExercise.Sets.Should().HaveCount(1);
            squatWorkoutExercise.Sets.First().Weight.Should().Be(185.0m);
        }

        [Fact]
        public async Task FreestyleWorkout_WithoutProgram_SavesAndRetrievesCorrectly()
        {
            var exercise1 = await Builder.Exercise("Push-ups", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Plank", "Core").SaveAsync();
            
            var freestyleWorkout1 = await Builder.Workout().SaveAsync();
            var freestyleWorkout2 = await Builder.Workout().SaveAsync();
            
            // Create workout exercises and sets
            var workoutExercise1 = new WorkoutExercise
            {
                Workout = freestyleWorkout1, WorkoutId = freestyleWorkout1.Id,
                Exercise = exercise1, ExerciseId = exercise1.Id, Order = 1
            };
            var workoutExercise2 = new WorkoutExercise
            {
                Workout = freestyleWorkout2, WorkoutId = freestyleWorkout2.Id,
                Exercise = exercise2, ExerciseId = exercise2.Id, Order = 1
            };
            await _workoutExerciseRepository.AddAsync(workoutExercise1);
            await _workoutExerciseRepository.AddAsync(workoutExercise2);
            
            var set1 = new Set { WorkoutExercise = workoutExercise1, WorkoutExerciseId = workoutExercise1.Id, Reps = 15, Weight = 0m, Order = 1, RPE = 8, RestTimeSeconds = 120, Notes = "Test set notes" };
            var set2 = new Set { WorkoutExercise = workoutExercise2, WorkoutExerciseId = workoutExercise2.Id, Reps = 60, Weight = 0m, Order = 1, RPE = 8, RestTimeSeconds = 120, Notes = "Test set notes" };
            await _setRepository.AddAsync(set1);
            await _setRepository.AddAsync(set2);

            var freestyleWorkouts = await _workoutRepository.GetByProgramIdAsync(null);
            freestyleWorkouts.Should().HaveCount(2);
            freestyleWorkouts.All(w => w.ProgramId == null).Should().BeTrue();

            var retrievedWorkout1 = await Context.Workouts
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Sets)
                .FirstOrDefaultAsync(w => w.Id == freestyleWorkout1.Id);

            retrievedWorkout1.Should().NotBeNull();
            retrievedWorkout1!.Program.Should().BeNull();
            retrievedWorkout1.WorkoutExercises.Should().HaveCount(1);
        }

        [Fact]
        public async Task CascadeDelete_Program_RemovesProgramExercisesButKeepsExercises()
        {
            var programExercises = await ProgramExerciseComposer.CreateMultipleAsync(
                "Test Program for Deletion",
                ("Dumbbell Press", "Chest", 1),
                ("Leg Press", "Legs", 2));
            var program = programExercises.First().Program;
            var exercise1 = programExercises.First(pe => pe.Exercise.Name == "Dumbbell Press").Exercise;
            var exercise2 = programExercises.First(pe => pe.Exercise.Name == "Leg Press").Exercise;
            var programExercise1 = programExercises.First(pe => pe.Exercise.Name == "Dumbbell Press");
            var programExercise2 = programExercises.First(pe => pe.Exercise.Name == "Leg Press");

            await _programRepository.DeleteAsync(program.Id);

            var deletedProgram = await _programRepository.GetByIdAsync(program.Id);
            var orphanedProgramExercise1 = await _programExerciseRepository.GetByIdAsync(programExercise1.Id);
            var orphanedProgramExercise2 = await _programExerciseRepository.GetByIdAsync(programExercise2.Id);
            var remainingExercise1 = await _exerciseRepository.GetByIdAsync(exercise1.Id);
            var remainingExercise2 = await _exerciseRepository.GetByIdAsync(exercise2.Id);

            deletedProgram.Should().BeNull();
            orphanedProgramExercise1.Should().BeNull();
            orphanedProgramExercise2.Should().BeNull();
            remainingExercise1.Should().NotBeNull();
            remainingExercise2.Should().NotBeNull();
        }

        [Fact]
        public async Task CascadeDelete_Workout_RemovesWorkoutExercisesAndSets()
        {
            var sets = await WorkoutComposer.CreateMultipleSetsAsync(
                "Test Exercise", "Back", null,
                (10, 100.0m, 1),
                (8, 105.0m, 2));
            var workout = sets.First().WorkoutExercise.Workout;
            var workoutExercise = sets.First().WorkoutExercise;
            var exercise = sets.First().WorkoutExercise.Exercise;
            var set1 = sets.First(s => s.Reps == 10);
            var set2 = sets.First(s => s.Reps == 8);

            await _workoutRepository.DeleteAsync(workout.Id);

            var deletedWorkout = await _workoutRepository.GetByIdAsync(workout.Id);
            var orphanedWorkoutExercise = await _workoutExerciseRepository.GetByIdAsync(workoutExercise.Id);
            var orphanedSet1 = await _setRepository.GetByIdAsync(set1.Id);
            var orphanedSet2 = await _setRepository.GetByIdAsync(set2.Id);
            var remainingExercise = await _exerciseRepository.GetByIdAsync(exercise.Id);

            deletedWorkout.Should().BeNull();
            orphanedWorkoutExercise.Should().BeNull();
            orphanedSet1.Should().BeNull();
            orphanedSet2.Should().BeNull();
            remainingExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task GetWorkoutWithAllRelatedData_LoadsEfficientlyWithInclude()
        {
            // Create program first
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Compound Exercise", "Full Body", "Complex Program");
            
            // Create workout with sets
            var sets = await WorkoutComposer.CreateMultipleSetsAsync(
                "Compound Exercise", "Full Body", program,
                (5, 225.0m, 1),
                (5, 235.0m, 2),
                (3, 245.0m, 3));
            var workout = sets.First().WorkoutExercise.Workout;
            var workoutExercise = sets.First().WorkoutExercise;

            var fullWorkout = await Context.Workouts
                .Include(w => w.Program)
                    .ThenInclude(p => p!.ProgramExercises)
                        .ThenInclude(pe => pe.Exercise)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Sets)
                .FirstOrDefaultAsync(w => w.Id == workout.Id);

            fullWorkout.Should().NotBeNull();
            fullWorkout!.Program.Should().NotBeNull();
            fullWorkout.Program!.ProgramExercises.Should().HaveCount(1);
            fullWorkout.WorkoutExercises.Should().HaveCount(1);
            fullWorkout.WorkoutExercises.First().Sets.Should().HaveCount(3);

            var progressionSets = fullWorkout.WorkoutExercises.First().Sets.OrderBy(s => s.Weight).ToList();
            progressionSets[0].Weight.Should().Be(225.0m);
            progressionSets[1].Weight.Should().Be(235.0m);
            progressionSets[2].Weight.Should().Be(245.0m);
        }

        [Fact]
        public async Task MultipleWorkoutsFromSameProgram_TrackProgressOverTime()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Bench Press Progression", "Chest", "Linear Progression Program");

            // Create three workouts with progressive weight increases
            var set1 = await WorkoutComposer.CreateCompleteSetAsync(
                "Bench Press Progression", "Chest", program, 10, 135.0m);
            var set2 = await WorkoutComposer.CreateCompleteSetAsync(
                "Bench Press Progression", "Chest", program, 10, 140.0m);
            var set3 = await WorkoutComposer.CreateCompleteSetAsync(
                "Bench Press Progression", "Chest", program, 10, 145.0m);
                
            // Update workout dates for progression
            var workout1 = set1.WorkoutExercise.Workout;
            var workout2 = set2.WorkoutExercise.Workout;
            var workout3 = set3.WorkoutExercise.Workout;
            
            workout1.Date = DateTime.UtcNow.AddDays(-7);
            workout2.Date = DateTime.UtcNow.AddDays(-3);
            workout3.Date = DateTime.UtcNow;
            
            Context.Workouts.UpdateRange(workout1, workout2, workout3);
            await Context.SaveChangesAsync();

            var programWorkouts = await _workoutRepository.GetByProgramIdAsync(program.Id);
            var orderedWorkouts = programWorkouts.OrderBy(w => w.Date).ToList();

            orderedWorkouts.Should().HaveCount(3);

            var workoutsWithData = await Context.Workouts
                .Where(w => w.ProgramId == program.Id)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Sets)
                .OrderBy(w => w.Date)
                .ToListAsync();

            workoutsWithData[0].WorkoutExercises.First().Sets.First().Weight.Should().Be(135.0m);
            workoutsWithData[1].WorkoutExercises.First().Sets.First().Weight.Should().Be(140.0m);
            workoutsWithData[2].WorkoutExercises.First().Sets.First().Weight.Should().Be(145.0m);
        }

        [Fact]
        public async Task ExerciseUsageAcrossMultiplePrograms_DoesNotCreateDuplication()
        {
            var commonExercise = await Builder.Exercise("Common Exercise", "Multi").SaveAsync();
            var program1 = await Builder.Program("Program A").SaveAsync();
            var program2 = await Builder.Program("Program B").SaveAsync();

            // Create program exercises that share the same exercise
            var programExercise1 = new ProgramExercise
            {
                Program = program1, ProgramId = program1.Id, Exercise = commonExercise, ExerciseId = commonExercise.Id,
                Order = 1, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100.0m, PlannedRestTimeSeconds = 120
            };
            var programExercise2 = new ProgramExercise
            {
                Program = program2, ProgramId = program2.Id, Exercise = commonExercise, ExerciseId = commonExercise.Id,
                Order = 1, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100.0m, PlannedRestTimeSeconds = 120
            };
            await _programExerciseRepository.AddAsync(programExercise1);
            await _programExerciseRepository.AddAsync(programExercise2);

            var allExercises = await _exerciseRepository.GetAllAsync();
            var exercisesByName = await _exerciseRepository.GetByNameAsync("Common Exercise");

            allExercises.Count(e => e.Name == "Common Exercise").Should().Be(1);
            exercisesByName.Should().NotBeNull();

            var program1Exercises = await Context.ProgramExercises
                .Where(pe => pe.ProgramId == program1.Id)
                .Include(pe => pe.Exercise)
                .ToListAsync();

            var program2Exercises = await Context.ProgramExercises
                .Where(pe => pe.ProgramId == program2.Id)
                .Include(pe => pe.Exercise)
                .ToListAsync();

            program1Exercises.Should().HaveCount(1);
            program2Exercises.Should().HaveCount(1);
            program1Exercises.First().Exercise.Id.Should().Be(program2Exercises.First().Exercise.Id);
        }
    }
}