using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class DeleteModel : PageModel
    {
        private readonly IProgramRepository _programRepository;
        private readonly IWorkoutRepository _workoutRepository;

        public DeleteModel(IProgramRepository programRepository, IWorkoutRepository workoutRepository)
        {
            _programRepository = programRepository;
            _workoutRepository = workoutRepository;
        }

        public ProgramModel? Program { get; set; }
        public bool HasAssociatedWorkouts { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "You must confirm deletion by checking this box")]
        public bool ConfirmDeletion { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);

            if (Program == null)
            {
                return NotFound();
            }

            // Check if this program has been used in any workouts
            var allWorkouts = await _workoutRepository.GetAllAsync();
            HasAssociatedWorkouts = allWorkouts.Any(w => w.ProgramId == id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);

            if (Program == null)
            {
                return NotFound();
            }

            // Reload the workout check for the form validation
            var allWorkouts = await _workoutRepository.GetAllAsync();
            HasAssociatedWorkouts = allWorkouts.Any(w => w.ProgramId == id);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Additional safety check: prevent deletion if there are associated workouts
            // This is a business rule - you might want to allow deletion but set ProgramId to null
            if (HasAssociatedWorkouts)
            {
                ModelState.AddModelError(string.Empty, 
                    "Cannot delete this program because it has been used in previous workouts. " +
                    "Contact support if you need to remove this program.");
                return Page();
            }

            var programName = Program.Name;
            await _programRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = $"Program '{programName}' has been deleted successfully.";
            return RedirectToPage("Index");
        }
    }
}