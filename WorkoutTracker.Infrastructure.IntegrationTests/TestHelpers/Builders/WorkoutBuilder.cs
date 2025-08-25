using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class WorkoutBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly Workout _workout;

        public WorkoutBuilder(WorkoutTrackerDbContext context)
        {
            _context = context;
            _workout = new Workout
            {
                Date = DateTime.UtcNow,
                ProgramId = null,
                Program = null,
                IsModifiedFromProgram = false
            };
        }

        public WorkoutBuilder WithDate(DateTime date)
        {
            _workout.Date = date;
            return this;
        }

        public WorkoutBuilder WithProgram(Program? program)
        {
            _workout.Program = program;
            _workout.ProgramId = program?.Id;
            return this;
        }

        public WorkoutBuilder AsModifiedFromProgram(bool isModified = true)
        {
            _workout.IsModifiedFromProgram = isModified;
            return this;
        }

        public async Task<Workout> SaveAsync()
        {
            _context.Workouts.Add(_workout);
            await _context.SaveChangesAsync();
            return _workout;
        }

        public Workout Build() => _workout;
    }
}