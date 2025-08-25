using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class ProgramRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly ProgramRepository _repository;

        public ProgramRepositoryIntegrationTests()
        {
            _repository = new ProgramRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoPrograms_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithPrograms_ReturnsAllPrograms()
        {
            var program1 = await Builder.Program("Push Day").SaveAsync();
            var program2 = await Builder.Program("Pull Day").SaveAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "Push Day");
            result.Should().Contain(p => p.Name == "Pull Day");
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsProgram()
        {
            var program = await Builder.Program("Full Body").SaveAsync();

            var result = await _repository.GetByIdAsync(program.Id);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Full Body");
            result.Description.Should().Be("Test Program Description");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidProgram_SavesAndReturnsProgram()
        {
            var program = Builder.Program("Upper Body").Build();

            var result = await _repository.AddAsync(program);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Upper Body");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedProgram = await Context.Programs.FindAsync(result.Id);
            savedProgram.Should().NotBeNull();
            savedProgram!.Name.Should().Be("Upper Body");
        }

        [Fact]
        public async Task UpdateAsync_WithExistingProgram_UpdatesSuccessfully()
        {
            var program = await Builder.Program("Leg Day").SaveAsync();

            program.Description = "Updated leg day description";
            program.Name = "Lower Body Day";

            await _repository.UpdateAsync(program);

            var updatedProgram = await Context.Programs.FindAsync(program.Id);
            updatedProgram.Should().NotBeNull();
            updatedProgram!.Description.Should().Be("Updated leg day description");
            updatedProgram.Name.Should().Be("Lower Body Day");
            updatedProgram.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesProgram()
        {
            var program = await Builder.Program("Cardio Day").SaveAsync();

            await _repository.DeleteAsync(program.Id);

            var deletedProgram = await Context.Programs.FindAsync(program.Id);
            deletedProgram.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var program = await Builder.Program("Strength Training").SaveAsync();

            var result = await _repository.ExistsAsync(program.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsProgram()
        {
            var program = await Builder.Program("HIIT Workout").SaveAsync();

            var result = await _repository.GetByNameAsync("HIIT Workout");

            result.Should().NotBeNull();
            result!.Name.Should().Be("HIIT Workout");
            result.Description.Should().Be("Test Program Description");
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ReturnsNull()
        {
            var result = await _repository.GetByNameAsync("NonExistent Program");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithProgramExercises_LoadsNavigationProperties()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Bench Press", "Chest", "Push Workout");

            var result = await Context.Programs
                .Include(p => p.ProgramExercises)
                    .ThenInclude(pe => pe.Exercise)
                .FirstOrDefaultAsync(p => p.Id == program.Id);

            result.Should().NotBeNull();
            result!.ProgramExercises.Should().HaveCount(1);
            result.ProgramExercises.First().Exercise.Name.Should().Be("Bench Press");
        }

        [Fact]
        public async Task DeleteAsync_WithProgramExercises_HandlesCascadeCorrectly()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Squat", "Legs", "Leg Workout");

            await _repository.DeleteAsync(program.Id);

            var deletedProgram = await Context.Programs.FindAsync(program.Id);
            var orphanedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            var remainingExercise = await Context.Exercises.FindAsync(exercise.Id);

            deletedProgram.Should().BeNull();
            orphanedProgramExercise.Should().BeNull();
            remainingExercise.Should().NotBeNull();
        }
    }
}