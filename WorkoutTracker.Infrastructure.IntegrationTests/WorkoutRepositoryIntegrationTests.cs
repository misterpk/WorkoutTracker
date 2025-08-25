using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class WorkoutRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly WorkoutRepository _repository;

        public WorkoutRepositoryIntegrationTests()
        {
            _repository = new WorkoutRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoWorkouts_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithWorkouts_ReturnsAllWorkouts()
        {
            var program = await Builder.Program("Push Day").SaveAsync();
            var workout1 = await Builder.Workout().WithProgram(program).SaveAsync();
            var workout2 = await Builder.Workout().SaveAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsWorkout()
        {
            var workout = await Builder.Workout().SaveAsync();

            var result = await _repository.GetByIdAsync(workout.Id);

            result.Should().NotBeNull();
            result!.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidWorkout_SavesAndReturnsWorkout()
        {
            var workout = Builder.Workout().Build();

            var result = await _repository.AddAsync(workout);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedWorkout = await Context.Workouts.FindAsync(result.Id);
            savedWorkout.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithExistingWorkout_UpdatesSuccessfully()
        {
            var workout = await Builder.Workout().SaveAsync();

            workout.IsModifiedFromProgram = true;
            workout.Date = DateTime.UtcNow.AddDays(-1);

            await _repository.UpdateAsync(workout);

            var updatedWorkout = await Context.Workouts.FindAsync(workout.Id);
            updatedWorkout.Should().NotBeNull();
            updatedWorkout!.IsModifiedFromProgram.Should().BeTrue();
            updatedWorkout.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesWorkout()
        {
            var workout = await Builder.Workout().SaveAsync();

            await _repository.DeleteAsync(workout.Id);

            var deletedWorkout = await Context.Workouts.FindAsync(workout.Id);
            deletedWorkout.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var workout = await Builder.Workout().SaveAsync();

            var result = await _repository.ExistsAsync(workout.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByProgramIdAsync_WithValidProgramId_ReturnsAssociatedWorkouts()
        {
            var program = await Builder.Program("Push Program").SaveAsync();
            var workout1 = await Builder.Workout().WithProgram(program).SaveAsync();
            var workout2 = await Builder.Workout().WithProgram(program).SaveAsync();
            var freestyleWorkout = await Builder.Workout().SaveAsync();

            var result = await _repository.GetByProgramIdAsync(program.Id);

            result.Should().HaveCount(2);
            result.Should().Contain(w => w.Id == workout1.Id);
            result.Should().Contain(w => w.Id == workout2.Id);
            result.Should().NotContain(w => w.Id == freestyleWorkout.Id);
        }

        [Fact]
        public async Task GetByProgramIdAsync_WithNullProgramId_ReturnsFreestyleWorkouts()
        {
            var program = await Builder.Program("Some Program").SaveAsync();
            var programWorkout = await Builder.Workout().WithProgram(program).SaveAsync();
            var freestyleWorkout1 = await Builder.Workout().SaveAsync();
            var freestyleWorkout2 = await Builder.Workout().SaveAsync();

            var result = await _repository.GetByProgramIdAsync(null);

            result.Should().HaveCount(2);
            result.Should().Contain(w => w.Id == freestyleWorkout1.Id);
            result.Should().Contain(w => w.Id == freestyleWorkout2.Id);
            result.Should().NotContain(w => w.Id == programWorkout.Id);
        }

        [Fact]
        public async Task GetByProgramIdAsync_WithNonExistingProgramId_ReturnsEmpty()
        {
            var workout = await Builder.Workout().SaveAsync();

            var result = await _repository.GetByProgramIdAsync(999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithValidRange_ReturnsWorkoutsInRange()
        {
            var startDate = DateTime.UtcNow.AddDays(-5);
            var endDate = DateTime.UtcNow.AddDays(-1);
            var middleDate = DateTime.UtcNow.AddDays(-3);

            var workoutInRange1 = await Builder.Workout().WithDate(startDate).SaveAsync();
            var workoutInRange2 = await Builder.Workout().WithDate(middleDate).SaveAsync();
            var workoutOutOfRange = await Builder.Workout().WithDate(DateTime.UtcNow.AddDays(-10)).SaveAsync();

            var result = await _repository.GetByDateRangeAsync(startDate, endDate);

            result.Should().HaveCount(2);
            result.Should().Contain(w => w.Id == workoutInRange1.Id);
            result.Should().Contain(w => w.Id == workoutInRange2.Id);
            result.Should().NotContain(w => w.Id == workoutOutOfRange.Id);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNoWorkoutsInRange_ReturnsEmpty()
        {
            var workout = await Builder.Workout().WithDate(DateTime.UtcNow.AddDays(-10)).SaveAsync();

            var startDate = DateTime.UtcNow.AddDays(-3);
            var endDate = DateTime.UtcNow.AddDays(-1);

            var result = await _repository.GetByDateRangeAsync(startDate, endDate);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithStartDateAfterEndDate_ReturnsEmpty()
        {
            var workout = await Builder.Workout().SaveAsync();

            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-5);

            var result = await _repository.GetByDateRangeAsync(startDate, endDate);

            result.Should().BeEmpty();
        }
    }
}