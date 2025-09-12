using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class IndexModel : PageModel
    {
        private readonly IProgramRepository _programRepository;

        public IndexModel(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        public IEnumerable<ProgramModel> Programs { get; set; } = new List<ProgramModel>();
        public string? SearchTerm { get; set; }
        public async Task OnGetAsync(string? searchTerm)
        {
            SearchTerm = searchTerm;

            var programs = await _programRepository.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm?.Trim()))
            {
                programs = programs.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            Programs = programs.OrderBy(p => p.Name);
        }

        public int CalculateEstimatedDuration(ProgramModel program)
        {
            if (!program.ProgramExercises.Any())
                return 0;

            // Estimate duration based on sets and rest periods
            // Assumptions: 
            // - Average of 45 seconds per set
            // - Rest period from ProgramExercise.PlannedRestTimeSeconds (default to 90 seconds if not set)
            // - 5 minutes warm-up and cool-down
            
            int totalSets = program.ProgramExercises.Sum(pe => pe.PlannedSets);
            int totalWorkTime = totalSets * 45; // 45 seconds per set
            
            // Calculate rest time (number of rest periods = total sets - number of exercises)
            int restPeriods = Math.Max(0, totalSets - program.ProgramExercises.Count);
            int averageRestTime = program.ProgramExercises.Any(pe => pe.PlannedRestTimeSeconds > 0) 
                ? (int)program.ProgramExercises.Where(pe => pe.PlannedRestTimeSeconds > 0).Average(pe => pe.PlannedRestTimeSeconds)
                : 90; // Default 90 seconds
            
            int totalRestTime = restPeriods * averageRestTime;
            int warmupCooldown = 5 * 60; // 5 minutes total
            
            return (totalWorkTime + totalRestTime + warmupCooldown) / 60; // Convert to minutes
        }
    }
}