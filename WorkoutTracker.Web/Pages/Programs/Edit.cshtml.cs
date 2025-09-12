using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class EditModel : PageModel
    {
        private readonly IProgramRepository _programRepository;

        public EditModel(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        [BindProperty]
        public ProgramEditDto ProgramDto { get; set; } = new ProgramEditDto();

        public ProgramModel? Program { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);

            if (Program == null)
            {
                return NotFound();
            }

            // Pre-populate the form with existing data
            ProgramDto.Name = Program.Name;
            ProgramDto.Description = Program.Description;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Program = await _programRepository.GetByIdAsync(id);

            if (Program == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update the program properties
            Program.Name = ProgramDto.Name;
            Program.Description = ProgramDto.Description ?? string.Empty;

            await _programRepository.UpdateAsync(Program);

            TempData["SuccessMessage"] = $"Program '{Program.Name}' has been updated successfully!";
            return RedirectToPage("Details", new { id = Program.Id });
        }
    }

    public class ProgramEditDto
    {
        [Required(ErrorMessage = "Program name is required")]
        [StringLength(200, ErrorMessage = "Program name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

    }
}