using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Repositories;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public class ProgramExerciseRepositoryIntegrationTests : BaseRepositoryIntegrationTest
    {
        private readonly ProgramExerciseRepository _repository;

        public ProgramExerciseRepositoryIntegrationTests()
        {
            _repository = new ProgramExerciseRepository(Context);
        }

        [Fact]
        public async Task GetAllAsync_WithNoProgramExercises_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WithProgramExercises_ReturnsAllProgramExercises()
        {
            await ProgramExerciseComposer.CreateMultipleAsync(
                "Full Body",
                ("Bench Press", "Chest", 1),
                ("Squat", "Legs", 2));

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsProgramExercise()
        {
            var programExercise = await ProgramExerciseComposer.CreateAsync(
                "Deadlift", "Back", "Pull Day");

            var result = await _repository.GetByIdAsync(programExercise.Id);

            result.Should().NotBeNull();
            result!.PlannedSets.Should().Be(3);
            result.PlannedReps.Should().Be(10);
            result.PlannedWeight.Should().Be(100.0m);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WithValidProgramExercise_SavesAndReturnsProgramExercise()
        {
            var exercise = await Builder.Exercise("Pull-ups", "Back").SaveAsync();
            var program = await Builder.Program("Upper Body").SaveAsync();
            
            var programExercise = new ProgramExercise
            {
                Program = program,
                ProgramId = program.Id,
                Exercise = exercise,
                ExerciseId = exercise.Id,
                Order = 1,
                PlannedSets = 3,
                PlannedReps = 10,
                PlannedWeight = 100.0m,
                PlannedRestTimeSeconds = 120
            };

            var result = await _repository.AddAsync(programExercise);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Order.Should().Be(1);
            result.PlannedSets.Should().Be(3);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedProgramExercise = await Context.ProgramExercises.FindAsync(result.Id);
            savedProgramExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_WithExistingProgramExercise_UpdatesSuccessfully()
        {
            var programExercise = await ProgramExerciseComposer.CreateAsync(
                "Overhead Press", "Shoulders", "Push Day");

            programExercise.PlannedSets = 4;
            programExercise.PlannedReps = 8;
            programExercise.PlannedWeight = 120.0m;

            await _repository.UpdateAsync(programExercise);

            var updatedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            updatedProgramExercise.Should().NotBeNull();
            updatedProgramExercise!.PlannedSets.Should().Be(4);
            updatedProgramExercise.PlannedReps.Should().Be(8);
            updatedProgramExercise.PlannedWeight.Should().Be(120.0m);
            updatedProgramExercise.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesProgramExercise()
        {
            var programExercise = await ProgramExerciseComposer.CreateAsync(
                "Dips", "Triceps", "Upper Body");

            await _repository.DeleteAsync(programExercise.Id);

            var deletedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            deletedProgramExercise.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var programExercise = await ProgramExerciseComposer.CreateAsync(
                "Rows", "Back", "Pull Day");

            var result = await _repository.ExistsAsync(programExercise.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RelationshipIntegrity_WithProgramAndExercise_MaintainsCorrectRelationships()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Lunges", "Legs", "Leg Day");

            var result = await Context.ProgramExercises
                .Include(pe => pe.Program)
                .Include(pe => pe.Exercise)
                .FirstOrDefaultAsync(pe => pe.Id == programExercise.Id);

            result.Should().NotBeNull();
            result!.Program.Should().NotBeNull();
            result.Program.Name.Should().Be("Leg Day");
            result.Exercise.Should().NotBeNull();
            result.Exercise.Name.Should().Be("Lunges");
            result.ProgramId.Should().Be(program.Id);
            result.ExerciseId.Should().Be(exercise.Id);
        }

        [Fact]
        public async Task DeleteProgramExercise_DoesNotAffectProgramOrExercise()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Curls", "Biceps", "Arm Day");

            await _repository.DeleteAsync(programExercise.Id);

            var deletedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            var remainingProgram = await Context.Programs.FindAsync(program.Id);
            var remainingExercise = await Context.Exercises.FindAsync(exercise.Id);

            deletedProgramExercise.Should().BeNull();
            remainingProgram.Should().NotBeNull();
            remainingExercise.Should().NotBeNull();
        }

        [Fact]
        public async Task OrderingValidation_WithMultipleProgramExercises_MaintainsCorrectOrder()
        {
            var programExercises = await ProgramExerciseComposer.CreateMultipleAsync(
                "Full Body",
                ("Exercise 1", "Chest", 1),
                ("Exercise 2", "Back", 2),
                ("Exercise 3", "Legs", 3));

            var orderedResults = await Context.ProgramExercises
                .Where(pe => pe.ProgramId == programExercises.First().ProgramId)
                .OrderBy(pe => pe.Order)
                .Include(pe => pe.Exercise)
                .ToListAsync();

            orderedResults.Should().HaveCount(3);
            orderedResults[0].Exercise.Name.Should().Be("Exercise 1");
            orderedResults[1].Exercise.Name.Should().Be("Exercise 2");
            orderedResults[2].Exercise.Name.Should().Be("Exercise 3");
        }
    }
}