using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Exercises
{
    public class EditModel : PageModel
    {
        private readonly IExerciseRepository _exerciseRepository;

        public EditModel(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        [BindProperty]
        public ExerciseEditDto Exercise { get; set; } = new ExerciseEditDto();

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

            Exercise = new ExerciseEditDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                PrimaryMuscle = exercise.PrimaryMuscle
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var exercise = await _exerciseRepository.GetByIdAsync(Exercise.Id);
            if (exercise == null)
            {
                return NotFound();
            }

            exercise.Name = Exercise.Name;
            exercise.Description = Exercise.Description ?? string.Empty;
            exercise.PrimaryMuscle = Exercise.PrimaryMuscle;

            await _exerciseRepository.UpdateAsync(exercise);

            TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' has been updated successfully!";
            return RedirectToPage("Details", new { id = exercise.Id });
        }
    }

    public class ExerciseEditDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Exercise name is required")]
        [StringLength(100, ErrorMessage = "Exercise name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Primary muscle group is required")]
        [StringLength(100, ErrorMessage = "Muscle group cannot exceed 100 characters")]
        public string PrimaryMuscle { get; set; } = string.Empty;
    }
}
