using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers;
using WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Composers;

namespace WorkoutTracker.Infrastructure.IntegrationTests
{
    public abstract class BaseRepositoryIntegrationTest : IDisposable
    {
        protected readonly WorkoutTrackerDbContext Context;
        protected readonly TestEntityBuilder Builder;
        protected readonly ProgramExerciseComposer ProgramExerciseComposer;
        protected readonly WorkoutComposer WorkoutComposer;
        protected readonly CrossRepositoryComposer CrossRepositoryComposer;
        private bool _disposed = false;

        protected BaseRepositoryIntegrationTest()
        {
            var options = new DbContextOptionsBuilder<WorkoutTrackerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            Context = new WorkoutTrackerDbContext(options);
            Context.Database.EnsureCreated();

            // Initialize composition-based builders and composers
            Builder = new TestEntityBuilder(Context);
            ProgramExerciseComposer = new ProgramExerciseComposer(Context);
            WorkoutComposer = new WorkoutComposer(Context);
            CrossRepositoryComposer = new CrossRepositoryComposer(Context);
        }

        protected async Task SeedDatabaseAsync()
        {
            var (workout, workoutExercise, set) = await WorkoutComposer.CreateCompleteWorkoutAsync(
                "Bench Press", "Chest");
            
            var (program, exercise, programExercise) = await ProgramExerciseComposer.CreateCompleteAsync(
                "Bench Press", "Chest", "Push Day");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                Context?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}