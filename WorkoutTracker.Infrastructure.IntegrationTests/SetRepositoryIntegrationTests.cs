using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class SetRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly SetRepository _repository;

        public SetRepositoryIntegrationTests()
        {
            _repository = new SetRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoSets_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithSets_ReturnsAllSets()
        {
            await WorkoutComposer.CreateMultipleSetsAsync(
                "Bench Press", "Chest", null,
                (10, 100.0m, 1),
                (8, 105.0m, 2));

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsSet()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Deadlift", "Back", null, 5, 200.0m);

            var result = await _repository.GetByIdAsync(set.Id);

            result.Should().NotBeNull();
            result!.Reps.Should().Be(5);
            result.Weight.Should().Be(200.0m);
            result.RPE.Should().Be(8);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidSet_SavesAndReturnsSet()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Squat", "Legs");
            
            var set = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 12,
                Weight = 150.0m,
                Order = 1,
                RPE = 8,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };

            var result = await _repository.AddAsync(set);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Reps.Should().Be(12);
            result.Weight.Should().Be(150.0m);
            result.RPE.Should().Be(8);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedSet = await Context.Sets.FindAsync(result.Id);
            savedSet.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithExistingSet_UpdatesSuccessfully()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Overhead Press", "Shoulders", null, 10, 80.0m);

            set.Reps = 8;
            set.Weight = 85.0m;
            set.RPE = 9;

            await _repository.UpdateAsync(set);

            var updatedSet = await Context.Sets.FindAsync(set.Id);
            updatedSet.Should().NotBeNull();
            updatedSet!.Reps.Should().Be(8);
            updatedSet.Weight.Should().Be(85.0m);
            updatedSet.RPE.Should().Be(9);
            updatedSet.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesSet()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Pull-ups", "Back", null, 8, 0m);

            await _repository.DeleteAsync(set.Id);

            var deletedSet = await Context.Sets.FindAsync(set.Id);
            deletedSet.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Dips", "Triceps", null, 15, 0m);

            var result = await _repository.ExistsAsync(set.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RelationshipIntegrity_WithWorkoutExercise_MaintainsCorrectRelationship()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Rows", "Back", null, 10, 120.0m);

            var result = await Context.Sets
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Exercise)
                .Include(s => s.WorkoutExercise)
                    .ThenInclude(we => we.Workout)
                .FirstOrDefaultAsync(s => s.Id == set.Id);

            result.Should().NotBeNull();
            result!.WorkoutExercise.Should().NotBeNull();
            result.WorkoutExercise.Exercise.Name.Should().Be("Rows");
            result.WorkoutExercise.Workout.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            result.WorkoutExerciseId.Should().Be(result.WorkoutExercise.Id);
        }

        [Fact]
        public async Task DeleteSet_DoesNotAffectWorkoutExercise()
        {
            var set = await WorkoutComposer.CreateCompleteSetAsync(
                "Curls", "Biceps", null, 12, 25.0m);

            await _repository.DeleteAsync(set.Id);

            var deletedSet = await Context.Sets.FindAsync(set.Id);
            var remainingWorkoutExercise = await Context.WorkoutExercises.FindAsync(set.WorkoutExerciseId);

            deletedSet.Should().BeNull();
            remainingWorkoutExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task DecimalPrecision_ForWeight_HandledCorrectly()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Bench Press", "Chest");
            
            var set = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 10,
                Weight = 135.25m,
                Order = 1,
                RPE = 8,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };

            var result = await _repository.AddAsync(set);

            result.Should().NotBeNull();
            result.Weight.Should().Be(135.25m);

            var savedSet = await Context.Sets.FindAsync(result.Id);
            savedSet.Should().NotBeNull();
            savedSet!.Weight.Should().Be(135.25m);
        }

        [Fact]
        public async Task RPE_RangeValidation_WorksCorrectly()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Deadlift", "Back");
            
            var setWithMinRPE = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 5,
                Weight = 300.0m,
                Order = 1,
                RPE = 1,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };

            var setWithMaxRPE = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 1,
                Weight = 350.0m,
                Order = 2,
                RPE = 10,
                RestTimeSeconds = 120,
                Notes = "Test set notes"
            };

            var result1 = await _repository.AddAsync(setWithMinRPE);
            var result2 = await _repository.AddAsync(setWithMaxRPE);

            result1.Should().NotBeNull();
            result1.RPE.Should().Be(1);
            
            result2.Should().NotBeNull();
            result2.RPE.Should().Be(10);
        }

        [Fact]
        public async Task OptionalFields_HandledCorrectly()
        {
            var (workout, workoutExercise) = await WorkoutComposer.CreateWorkoutWithExerciseAsync(
                "Planks", "Core");

            var setWithoutRPE = new Set
            {
                WorkoutExercise = workoutExercise,
                WorkoutExerciseId = workoutExercise.Id,
                Reps = 30,
                Weight = 0m,
                RPE = 0
            };

            var result = await _repository.AddAsync(setWithoutRPE);

            result.Should().NotBeNull();
            result.RPE.Should().Be(0);
            result.Reps.Should().Be(30);
            result.Weight.Should().Be(0m);

            var savedSet = await Context.Sets.FindAsync(result.Id);
            savedSet.Should().NotBeNull();
            savedSet!.RPE.Should().Be(0);
        }
    }
}