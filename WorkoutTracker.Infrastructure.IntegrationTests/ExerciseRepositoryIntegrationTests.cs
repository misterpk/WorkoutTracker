using FluentAssertions;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class ExerciseRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly ExerciseRepository _repository;

        public ExerciseRepositoryIntegrationTests()
        {
            _repository = new ExerciseRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoExercises_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithExercises_ReturnsAllExercises()
        {
            var exercise1 = await Builder.Exercise("Bench Press", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Squat", "Legs").SaveAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Name == "Bench Press");
            result.Should().Contain(e => e.Name == "Squat");
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsExercise()
        {
            var exercise = await Builder.Exercise("Deadlift", "Back").SaveAsync();

            var result = await _repository.GetByIdAsync(exercise.Id);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Deadlift");
            result.PrimaryMuscle.Should().Be("Back");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidExercise_SavesAndReturnsExercise()
        {
            var exercise = Builder.Exercise("Pull-ups", "Back").Build();

            var result = await _repository.AddAsync(exercise);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Pull-ups");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedExercise = await Context.Exercises.FindAsync(result.Id);
            savedExercise.Should().NotBeNull();
            savedExercise!.Name.Should().Be("Pull-ups");
        }

        [Fact]
        public async Task UpdateAsync_WithExistingExercise_UpdatesSuccessfully()
        {
            var exercise = await Builder.Exercise("Overhead Press", "Shoulders").SaveAsync();

            exercise.Description = "Updated description";
            exercise.PrimaryMuscle = "Deltoids";

            await _repository.UpdateAsync(exercise);

            var updatedExercise = await Context.Exercises.FindAsync(exercise.Id);
            updatedExercise.Should().NotBeNull();
            updatedExercise!.Description.Should().Be("Updated description");
            updatedExercise.PrimaryMuscle.Should().Be("Deltoids");
            updatedExercise.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesExercise()
        {
            var exercise = await Builder.Exercise("Dips", "Triceps").SaveAsync();

            await _repository.DeleteAsync(exercise.Id);

            var deletedExercise = await Context.Exercises.FindAsync(exercise.Id);
            deletedExercise.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var exercise = await Builder.Exercise("Rows", "Back").SaveAsync();

            var result = await _repository.ExistsAsync(exercise.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsExercise()
        {
            var exercise = await Builder.Exercise("Lunges", "Legs").SaveAsync();

            var result = await _repository.GetByNameAsync("Lunges");

            result.Should().NotBeNull();
            result!.Name.Should().Be("Lunges");
            result.PrimaryMuscle.Should().Be("Legs");
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ReturnsNull()
        {
            var result = await _repository.GetByNameAsync("NonExistent Exercise");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByPrimaryMuscleAsync_WithValidMuscle_ReturnsMatchingExercises()
        {
            var exercise1 = await Builder.Exercise("Bench Press", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Flyes", "Chest").SaveAsync();
            var exercise3 = await Builder.Exercise("Squat", "Legs").SaveAsync();

            var result = await _repository.GetByPrimaryMuscleAsync("Chest");

            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Name == "Bench Press");
            result.Should().Contain(e => e.Name == "Flyes");
            result.Should().NotContain(e => e.Name == "Squat");
        }

        [Fact]
        public async Task GetByPrimaryMuscleAsync_WithNonMatchingMuscle_ReturnsEmpty()
        {
            var exercise = await Builder.Exercise("Push-ups", "Chest").SaveAsync();

            var result = await _repository.GetByPrimaryMuscleAsync("Calves");

            result.Should().BeEmpty();
        }
    }
}