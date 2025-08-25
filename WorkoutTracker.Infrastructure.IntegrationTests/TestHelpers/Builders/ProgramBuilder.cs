using WorkoutTracker.Core.Models;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.IntegrationTests.TestHelpers.Builders
{
    public class ProgramBuilder
    {
        private readonly WorkoutTrackerDbContext _context;
        private readonly Program _program;

        public ProgramBuilder(WorkoutTrackerDbContext context, string name = TestConstants.DefaultProgramName)
        {
            _context = context;
            _program = new Program
            {
                Name = name,
                Description = "Test Program Description"
            };
        }

        public ProgramBuilder WithName(string name)
        {
            _program.Name = name;
            return this;
        }

        public ProgramBuilder WithDescription(string description)
        {
            _program.Description = description;
            return this;
        }

        public async Task<Program> SaveAsync()
        {
            _context.Programs.Add(_program);
            await _context.SaveChangesAsync();
            return _program;
        }

        public Program Build() => _program;
    }
}