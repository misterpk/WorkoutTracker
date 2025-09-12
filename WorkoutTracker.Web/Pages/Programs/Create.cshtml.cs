using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.Pages.Programs
{
    public class CreateModel : PageModel
    {
        private readonly IProgramRepository _programRepository;

        public CreateModel(IProgramRepository programRepository)
        {
            _programRepository = programRepository;
        }

        [BindProperty]
        public ProgramDto Program { get; set; } = new ProgramDto();
        
        [BindProperty(SupportsGet = true)]
        public int? CopyFromId { get; set; }
        
        public ProgramModel? CopyFromProgram { get; set; }

        public async Task OnGetAsync()
        {
            if (CopyFromId.HasValue)
            {
                CopyFromProgram = await _programRepository.GetByIdAsync(CopyFromId.Value);
                if (CopyFromProgram != null)
                {
                    Program.Name = $"Copy of {CopyFromProgram.Name}";
                    Program.Description = CopyFromProgram.Description;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload the copy from program if needed
                if (CopyFromId.HasValue)
                {
                    CopyFromProgram = await _programRepository.GetByIdAsync(CopyFromId.Value);
                }
                return Page();
            }

            var program = new ProgramModel
            {
                Name = Program.Name,
                Description = Program.Description ?? string.Empty
            };

            var createdProgram = await _programRepository.AddAsync(program);

            // If copying from another program, use the CopyAsync method
            if (CopyFromId.HasValue)
            {
                await _programRepository.CopyAsync(CopyFromId.Value, Program.Name);
            }

            TempData["SuccessMessage"] = CopyFromId.HasValue 
                ? $"Program '{program.Name}' has been created successfully with all exercises copied!"
                : $"Program '{program.Name}' has been created successfully!";
            
            return RedirectToPage("Details", new { id = createdProgram.Id });
        }
    }

    public class ProgramDto
    {
        [Required(ErrorMessage = "Program name is required")]
        [StringLength(200, ErrorMessage = "Program name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}