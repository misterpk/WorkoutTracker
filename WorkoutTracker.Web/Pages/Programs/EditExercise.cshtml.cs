using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class EditExerciseModel : PageModel
    {
        private readonly IProgramRepository _programRepository;

        public EditExerciseModel(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        [BindProperty]
        public ProgramExerciseEditDto ProgramExercise { get; set; } = new ProgramExerciseEditDto();

        public ProgramExercise? OriginalProgramExercise { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            OriginalProgramExercise = await _programRepository.GetProgramExerciseByIdAsync(id);
            if (OriginalProgramExercise == null)
            {
                return NotFound();
            }

            // Populate the DTO with current values
            ProgramExercise.Id = OriginalProgramExercise.Id;
            ProgramExercise.ProgramId = OriginalProgramExercise.ProgramId;
            ProgramExercise.Order = OriginalProgramExercise.Order;
            ProgramExercise.PlannedSets = OriginalProgramExercise.PlannedSets;
            ProgramExercise.PlannedReps = OriginalProgramExercise.PlannedReps;
            ProgramExercise.PlannedWeight = OriginalProgramExercise.PlannedWeight;
            ProgramExercise.PlannedRestTimeSeconds = OriginalProgramExercise.PlannedRestTimeSeconds;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            OriginalProgramExercise = await _programRepository.GetProgramExerciseByIdAsync(ProgramExercise.Id);
            if (OriginalProgramExercise == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update the original entity with new values
            OriginalProgramExercise.Order = ProgramExercise.Order;
            OriginalProgramExercise.PlannedSets = ProgramExercise.PlannedSets;
            OriginalProgramExercise.PlannedReps = ProgramExercise.PlannedReps;
            OriginalProgramExercise.PlannedWeight = ProgramExercise.PlannedWeight;
            OriginalProgramExercise.PlannedRestTimeSeconds = ProgramExercise.PlannedRestTimeSeconds;

            await _programRepository.UpdateProgramExerciseAsync(OriginalProgramExercise);

            TempData["SuccessMessage"] = $"Exercise '{OriginalProgramExercise.Exercise.Name}' has been updated successfully!";
            return RedirectToPage("Details", new { id = ProgramExercise.ProgramId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var programExercise = await _programRepository.GetProgramExerciseByIdAsync(id);
            if (programExercise == null)
            {
                return NotFound();
            }

            var programId = programExercise.ProgramId;
            await _programRepository.DeleteProgramExerciseAsync(id);

            TempData["SuccessMessage"] = $"Exercise '{programExercise.Exercise.Name}' has been removed from the program.";
            return RedirectToPage("Details", new { id = programId });
        }
    }

    public class ProgramExerciseEditDto
    {
        public int Id { get; set; }
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Order is required")]
        [Range(1, 100, ErrorMessage = "Order must be between 1 and 100")]
        public int Order { get; set; }

        [Required(ErrorMessage = "Planned sets is required")]
        [Range(1, 20, ErrorMessage = "Planned sets must be between 1 and 20")]
        public int PlannedSets { get; set; }

        [Required(ErrorMessage = "Planned reps is required")]
        [Range(1, 100, ErrorMessage = "Planned reps must be between 1 and 100")]
        public int PlannedReps { get; set; }

        [Range(0, 1000, ErrorMessage = "Planned weight must be between 0 and 1000")]
        public decimal PlannedWeight { get; set; }

        [Range(0, 3600, ErrorMessage = "Rest time must be between 0 and 3600 seconds")]
        public int PlannedRestTimeSeconds { get; set; }
    }
}