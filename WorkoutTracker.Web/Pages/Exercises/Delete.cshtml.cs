using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Exercises
{
    public class DeleteModel : PageModel
    {
        private readonly IExerciseRepository _exerciseRepository;

        public DeleteModel(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        [BindProperty]
        public Exercise Exercise { get; set; } = null!;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exercise = await _exerciseRepository.GetByIdAsync(id.Value);
            if (exercise == null)
            {
                return NotFound();
            }

            Exercise = exercise;

            if (saveChangesError.GetValueOrDefault())
            {
                ErrorMessage = "Delete failed. This exercise may be used in existing programs or workouts.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exercise = await _exerciseRepository.GetByIdAsync(id.Value);
            if (exercise == null)
            {
                return NotFound();
            }

            try
            {
                await _exerciseRepository.DeleteAsync(id.Value);
                TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' has been deleted successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception)
            {
                // If delete fails (likely due to foreign key constraints), 
                // redirect back to the delete page with an error
                return RedirectToPage("Delete", new { id = id, saveChangesError = true });
            }
        }
    }
}
