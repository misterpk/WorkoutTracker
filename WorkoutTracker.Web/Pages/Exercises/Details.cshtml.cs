using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Exercises
{
    public class DetailsModel : PageModel
    {
        private readonly IExerciseRepository _exerciseRepository;

        public DetailsModel(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public Exercise Exercise { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
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
            return Page();
        }
    }
}
