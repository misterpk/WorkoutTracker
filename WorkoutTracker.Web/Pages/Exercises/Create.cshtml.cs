using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Exercises
{
    public class CreateModel : PageModel
    {
        private readonly IExerciseRepository _exerciseRepository;

        public CreateModel(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        [BindProperty]
        public ExerciseDto Exercise { get; set; } = new ExerciseDto();

        public void OnGet()
        {
            // Initialize with empty form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var exercise = new Exercise
            {
                Name = Exercise.Name,
                Description = Exercise.Description ?? string.Empty,
                PrimaryMuscle = Exercise.PrimaryMuscle
            };

            var createdExercise = await _exerciseRepository.AddAsync(exercise);

            TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' has been created successfully!";
            return RedirectToPage("Details", new { id = createdExercise.Id });
        }
    }

    public class ExerciseDto
    {
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
