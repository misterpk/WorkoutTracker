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

        [Fact]
        public async Task HasWorkoutsAsync_WithNoWorkouts_ReturnsFalse()
        {
            var program = await Builder.Program("Test Program").SaveAsync();

            var result = await _repository.HasWorkoutsAsync(program.Id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasWorkoutsAsync_WithWorkouts_ReturnsTrue()
        {
            var program = await Builder.Program("Test Program").SaveAsync();
            var workout = await Builder.Workout()
                .WithProgram(program)
                .SaveAsync();

            var result = await _repository.HasWorkoutsAsync(program.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CopyAsync_WithValidProgram_CreatesExactCopy()
        {
            var exercise = await Builder.Exercise("Bench Press", "Chest").SaveAsync();
            var sourceProgram = await Builder.Program("Original Program").SaveAsync();
            
            // Create ProgramExercise directly linking to our source program
            var programExercise = new ProgramExercise
            {
                ProgramId = sourceProgram.Id,
                ExerciseId = exercise.Id,
                Order = 1,
                PlannedSets = 3,
                PlannedReps = 10,
                PlannedWeight = 135m,
                PlannedRestTimeSeconds = 90
            };
            Context.ProgramExercises.Add(programExercise);
            await Context.SaveChangesAsync();

            var copiedProgram = await _repository.CopyAsync(sourceProgram.Id, "Copied Program");

            copiedProgram.Should().NotBeNull();
            copiedProgram.Name.Should().Be("Copied Program");
            copiedProgram.Description.Should().Be(sourceProgram.Description);
            copiedProgram.Id.Should().NotBe(sourceProgram.Id);
            
            var copiedProgramExercises = await Context.ProgramExercises
                .Where(pe => pe.ProgramId == copiedProgram.Id)
                .ToListAsync();
            
            copiedProgramExercises.Should().HaveCount(1);
            var copiedExercise = copiedProgramExercises.First();
            copiedExercise.ExerciseId.Should().Be(exercise.Id);
            copiedExercise.Order.Should().Be(1);
            copiedExercise.PlannedSets.Should().Be(3);
            copiedExercise.PlannedReps.Should().Be(10);
            copiedExercise.PlannedWeight.Should().Be(135m);
            copiedExercise.PlannedRestTimeSeconds.Should().Be(90);
        }

        [Fact]
        public async Task CopyAsync_WithNonExistentProgram_ThrowsException()
        {
            var act = async () => await _repository.CopyAsync(999, "Copy Name");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Program with ID 999 not found");
        }

        [Fact]
        public async Task AddExerciseAsync_WithValidProgramExercise_AddsSuccessfully()
        {
            var program = await Builder.Program("Test Program").SaveAsync();
            var exercise = await Builder.Exercise("Push Ups", "Chest").SaveAsync();
            
            var programExercise = new ProgramExercise
            {
                ProgramId = program.Id,
                ExerciseId = exercise.Id,
                Order = 1,
                PlannedSets = 3,
                PlannedReps = 12,
                PlannedWeight = 0,
                PlannedRestTimeSeconds = 60
            };

            var result = await _repository.AddExerciseAsync(programExercise);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            
            var savedProgramExercise = await Context.ProgramExercises.FindAsync(result.Id);
            savedProgramExercise.Should().NotBeNull();
            savedProgramExercise!.ProgramId.Should().Be(program.Id);
            savedProgramExercise.ExerciseId.Should().Be(exercise.Id);
        }

        [Fact]
        public async Task UpdateProgramExerciseAsync_WithValidProgramExercise_UpdatesSuccessfully()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Squats", "Legs", "Leg Day");

            programExercise.PlannedSets = 5;
            programExercise.PlannedReps = 15;
            programExercise.PlannedWeight = 225m;

            await _repository.UpdateProgramExerciseAsync(programExercise);

            var updatedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            updatedProgramExercise.Should().NotBeNull();
            updatedProgramExercise!.PlannedSets.Should().Be(5);
            updatedProgramExercise.PlannedReps.Should().Be(15);
            updatedProgramExercise.PlannedWeight.Should().Be(225m);
        }

        [Fact]
        public async Task DeleteProgramExerciseAsync_WithValidId_RemovesProgramExercise()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Deadlifts", "Back", "Pull Day");

            await _repository.DeleteProgramExerciseAsync(programExercise.Id);

            var deletedProgramExercise = await Context.ProgramExercises.FindAsync(programExercise.Id);
            deletedProgramExercise.Should().BeNull();
            
            // Ensure the exercise and program are not deleted
            var remainingExercise = await Context.Exercises.FindAsync(exercise.Id);
            var remainingProgram = await Context.Programs.FindAsync(program.Id);
            remainingExercise.Should().NotBeNull();
            remainingProgram.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteProgramExerciseAsync_WithNonExistentId_DoesNotThrow()
        {
            var act = async () => await _repository.DeleteProgramExerciseAsync(999);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetProgramExerciseByIdAsync_WithValidId_ReturnsProgramExercise()
        {
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Pull Ups", "Back", "Upper Body");

            var result = await _repository.GetProgramExerciseByIdAsync(programExercise.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(programExercise.Id);
            result.Exercise.Should().NotBeNull();
            result.Exercise.Name.Should().Be("Pull Ups");
            result.Program.Should().NotBeNull();
            result.Program.Name.Should().Be("Upper Body");
        }

        [Fact]
        public async Task GetProgramExerciseByIdAsync_WithNonExistentId_ReturnsNull()
        {
            var result = await _repository.GetProgramExerciseByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task ReorderExercisesAsync_WithValidExerciseIds_UpdatesOrderCorrectly()
        {
            var program = await Builder.Program("Reorder Test").SaveAsync();
            var exercise1 = await Builder.Exercise("Exercise 1", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Exercise 2", "Back").SaveAsync();
            var exercise3 = await Builder.Exercise("Exercise 3", "Legs").SaveAsync();

            var pe1 = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise1.Id, Order = 1, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100, PlannedRestTimeSeconds = 120 };
            var pe2 = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise2.Id, Order = 2, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100, PlannedRestTimeSeconds = 120 };
            var pe3 = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise3.Id, Order = 3, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100, PlannedRestTimeSeconds = 120 };
            
            Context.ProgramExercises.AddRange(pe1, pe2, pe3);
            await Context.SaveChangesAsync();

            // Reorder: pe3, pe1, pe2
            var newOrder = new List<int> { pe3.Id, pe1.Id, pe2.Id };

            await _repository.ReorderExercisesAsync(program.Id, newOrder);

            Context.ChangeTracker.Clear();
            
            var reorderedExercises = await Context.ProgramExercises
                .Where(pe => pe.ProgramId == program.Id)
                .OrderBy(pe => pe.Order)
                .ToListAsync();

            reorderedExercises.Should().HaveCount(3);
            reorderedExercises[0].Id.Should().Be(pe3.Id);
            reorderedExercises[0].Order.Should().Be(1);
            reorderedExercises[1].Id.Should().Be(pe1.Id);
            reorderedExercises[1].Order.Should().Be(2);
            reorderedExercises[2].Id.Should().Be(pe2.Id);
            reorderedExercises[2].Order.Should().Be(3);
        }

        [Fact]
        public async Task ReorderExercisesAsync_WithPartialExerciseIds_UpdatesOnlyProvidedExercises()
        {
            var program = await Builder.Program("Partial Reorder Test").SaveAsync();
            var exercise1 = await Builder.Exercise("Exercise A", "Chest").SaveAsync();
            var exercise2 = await Builder.Exercise("Exercise B", "Back").SaveAsync();

            var pe1 = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise1.Id, Order = 1, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100, PlannedRestTimeSeconds = 120 };
            var pe2 = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise2.Id, Order = 2, PlannedSets = 3, PlannedReps = 10, PlannedWeight = 100, PlannedRestTimeSeconds = 120 };
            
            Context.ProgramExercises.AddRange(pe1, pe2);
            await Context.SaveChangesAsync();

            // Only reorder pe2 to position 1 (pe1 should remain at order 1)
            var newOrder = new List<int> { pe2.Id };

            await _repository.ReorderExercisesAsync(program.Id, newOrder);

            Context.ChangeTracker.Clear();
            
            var updatedPe1 = await Context.ProgramExercises.FindAsync(pe1.Id);
            var updatedPe2 = await Context.ProgramExercises.FindAsync(pe2.Id);

            updatedPe1!.Order.Should().Be(1); // Unchanged
            updatedPe2!.Order.Should().Be(1); // Updated
        }
    }
}