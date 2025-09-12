using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class AddExerciseModel : PageModel
    {
        private readonly IProgramRepository _programRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public AddExerciseModel(IProgramRepository programRepository, IExerciseRepository exerciseRepository)
        {
            _programRepository = programRepository;
            _exerciseRepository = exerciseRepository;
        }

        [BindProperty]
        public ProgramExerciseDto ProgramExercise { get; set; } = new ProgramExerciseDto();

        public ProgramModel? Program { get; set; }
        public List<SelectListItem> ExerciseOptions { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);
            if (Program == null)
            {
                return NotFound();
            }

            ProgramExercise.ProgramId = id;
            
            // Set default order to be the next in sequence
            ProgramExercise.Order = Program.ProgramExercises.Any() 
                ? Program.ProgramExercises.Max(pe => pe.Order) + 1 
                : 1;

            await LoadExerciseOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Program = await _programRepository.GetByIdAsync(ProgramExercise.ProgramId);
            if (Program == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadExerciseOptionsAsync();
                return Page();
            }

            var programExercise = new ProgramExercise
            {
                ProgramId = ProgramExercise.ProgramId,
                ExerciseId = ProgramExercise.ExerciseId,
                Order = ProgramExercise.Order,
                PlannedSets = ProgramExercise.PlannedSets,
                PlannedReps = ProgramExercise.PlannedReps,
                PlannedWeight = ProgramExercise.PlannedWeight,
                PlannedRestTimeSeconds = ProgramExercise.PlannedRestTimeSeconds
            };

            await _programRepository.AddExerciseAsync(programExercise);

            TempData["SuccessMessage"] = "Exercise has been added to the program successfully!";
            return RedirectToPage("Details", new { id = ProgramExercise.ProgramId });
        }

        private async Task LoadExerciseOptionsAsync()
        {
            var exercises = await _exerciseRepository.GetAllAsync();
            ExerciseOptions = exercises.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} ({e.PrimaryMuscle})"
            }).ToList();

            // Add an empty option at the beginning
            ExerciseOptions.Insert(0, new SelectListItem { Value = "", Text = "Select an exercise..." });
        }
    }

    public class ProgramExerciseDto
    {
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Exercise is required")]
        public int ExerciseId { get; set; }

        [Required(ErrorMessage = "Order is required")]
        [Range(1, 100, ErrorMessage = "Order must be between 1 and 100")]
        public int Order { get; set; }

        [Required(ErrorMessage = "Planned sets is required")]
        [Range(1, 20, ErrorMessage = "Planned sets must be between 1 and 20")]
        public int PlannedSets { get; set; } = 3;

        [Required(ErrorMessage = "Planned reps is required")]
        [Range(1, 100, ErrorMessage = "Planned reps must be between 1 and 100")]
        public int PlannedReps { get; set; } = 10;

        [Range(0, 1000, ErrorMessage = "Planned weight must be between 0 and 1000")]
        public decimal PlannedWeight { get; set; } = 0;

        [Range(0, 3600, ErrorMessage = "Rest time must be between 0 and 3600 seconds")]
        public int PlannedRestTimeSeconds { get; set; } = 90;
    }
}