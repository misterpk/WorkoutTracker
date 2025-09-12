using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class DetailsModel : PageModel
    {
        private readonly IProgramRepository _programRepository;

        public DetailsModel(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        public ProgramModel? Program { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);

            if (Program == null)
            {
                return NotFound();
            }

            return Page();
        }

        public int CalculateEstimatedDuration()
        {
            if (Program == null || !Program.ProgramExercises.Any())
                return 0;

            // Estimate duration based on sets and rest periods
            // Assumptions: 
            // - Average of 45 seconds per set
            // - Rest period from ProgramExercise.PlannedRestTimeSeconds (default to 90 seconds if not set)
            // - 5 minutes warm-up and cool-down
            
            int totalSets = Program.ProgramExercises.Sum(pe => pe.PlannedSets);
            int totalWorkTime = totalSets * 45; // 45 seconds per set
            
            // Calculate rest time (number of rest periods = total sets - number of exercises)
            int restPeriods = Math.Max(0, totalSets - Program.ProgramExercises.Count);
            int averageRestTime = Program.ProgramExercises.Any(pe => pe.PlannedRestTimeSeconds > 0) 
                ? (int)Program.ProgramExercises.Where(pe => pe.PlannedRestTimeSeconds > 0).Average(pe => pe.PlannedRestTimeSeconds)
                : 90; // Default 90 seconds
            
            int totalRestTime = restPeriods * averageRestTime;
            int warmupCooldown = 5 * 60; // 5 minutes total
            
            return (totalWorkTime + totalRestTime + warmupCooldown) / 60; // Convert to minutes
        }
    }
}