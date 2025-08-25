using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class WorkoutExerciseRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly WorkoutExerciseRepository _repository;

        public WorkoutExerciseRepositoryIntegrationTests()
        {
            _repository = new WorkoutExerciseRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoWorkoutExercises_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithWorkoutExercises_ReturnsAllWorkoutExercises()
        {
            await WorkoutComposer.CreateWorkoutExerciseAsync("Bench Press", "Chest");
            await WorkoutComposer.CreateWorkoutExerciseAsync("Squat", "Legs");

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsWorkoutExercise()
        {
            var workoutExercise = await WorkoutComposer.CreateWorkoutExerciseAsync("Deadlift", "Back");

            var result = await _repository.GetByIdAsync(workoutExercise.Id);

            result.Should().NotBeNull();
            result!.Order.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidWorkoutExercise_SavesAndReturnsWorkoutExercise()
        {
            var exercise = await Builder.Exercise("Pull-ups", "Back").SaveAsync();
            var workout = await Builder.Workout().SaveAsync();
            
            var workoutExercise = new WorkoutExercise
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = 1
            };

            var result = await _repository.AddAsync(workoutExercise);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Order.Should().Be(1);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedWorkoutExercise = await Context.WorkoutExercises.FindAsync(result.Id);
            savedWorkoutExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithExistingWorkoutExercise_UpdatesSuccessfully()
        {
            var workoutExercise = await WorkoutComposer.CreateWorkoutExerciseAsync(
                "Overhead Press", "Shoulders");

            workoutExercise.Order = 2;

            await _repository.UpdateAsync(workoutExercise);

            var updatedWorkoutExercise = await Context.WorkoutExercises.FindAsync(workoutExercise.Id);
            updatedWorkoutExercise.Should().NotBeNull();
            updatedWorkoutExercise!.Order.Should().Be(2);
            updatedWorkoutExercise.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesWorkoutExercise()
        {
            var workoutExercise = await WorkoutComposer.CreateWorkoutExerciseAsync(
                "Dips", "Triceps");

            await _repository.DeleteAsync(workoutExercise.Id);

            var deletedWorkoutExercise = await Context.WorkoutExercises.FindAsync(workoutExercise.Id);
            deletedWorkoutExercise.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var workoutExercise = await WorkoutComposer.CreateWorkoutExerciseAsync(
                "Rows", "Back");

            var result = await _repository.ExistsAsync(workoutExercise.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RelationshipIntegrity_WithWorkoutAndExercise_MaintainsCorrectRelationships()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Lunges", "Legs");

            var result = await Context.WorkoutExercises
                .Include(we => we.Workout)
                .Include(we => we.Exercise)
                .FirstOrDefaultAsync(we => we.Id == workoutExercise.Id);

            result.Should().NotBeNull();
            result!.Workout.Should().NotBeNull();
            result.Workout.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            result.Exercise.Should().NotBeNull();
            result.Exercise.Name.Should().Be("Lunges");
            result.WorkoutId.Should().Be(workout.Id);
            result.ExerciseId.Should().Be(result.Exercise.Id);
        }

        [Fact]
        public async Task DeleteWorkoutExercise_DoesNotAffectWorkoutOrExercise()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Curls", "Biceps");

            await _repository.DeleteAsync(workoutExercise.Id);

            var deletedWorkoutExercise = await Context.WorkoutExercises.FindAsync(workoutExercise.Id);
            var remainingWorkout = await Context.Workouts.FindAsync(workout.Id);
            var remainingExercise = await Context.Exercises.FindAsync(workoutExercise.ExerciseId);

            deletedWorkoutExercise.Should().BeNull();
            remainingWorkout.Should().NotBeNull();
            remainingExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task WorkoutExerciseWithSets_LoadsNavigationPropertiesCorrectly()
        {
            var sets = await WorkoutComposer.CreateMultipleSetsAsync(
                "Tricep Extensions", "Triceps", null,
                (10, 50.0m, 1),
                (8, 55.0m, 2));
            var workoutExercise = sets.First().WorkoutExercise;

            var result = await Context.WorkoutExercises
                .Include(we => we.Sets)
                .Include(we => we.Exercise)
                .Include(we => we.Workout)
                .FirstOrDefaultAsync(we => we.Id == workoutExercise.Id);

            result.Should().NotBeNull();
            result!.Sets.Should().HaveCount(2);
            result.Sets.Should().Contain(s => s.Reps == 10 && s.Weight == 50.0m);
            result.Sets.Should().Contain(s => s.Reps == 8 && s.Weight == 55.0m);
        }

        [Fact]
        public async Task NullableFields_HandledCorrectly()
        {
            var exercise = await Builder.Exercise("Planks", "Core").SaveAsync();
            var workout = await Builder.Workout().SaveAsync();

            var workoutExercise = new WorkoutExercise
            {
                Workout = workout,
                WorkoutId = workout.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = 1
            };

            var result = await _repository.AddAsync(workoutExercise);

            result.Should().NotBeNull();
            result.Order.Should().Be(1);

            var savedWorkoutExercise = await Context.WorkoutExercises.FindAsync(result.Id);
            savedWorkoutExercise.Should().NotBeNull();
            savedWorkoutExercise!.Order.Should().Be(1);
        }
    }
}