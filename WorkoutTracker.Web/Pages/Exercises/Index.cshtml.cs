using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;

namespace WorkoutTracker.Web.Pages.Exercises
{
    public class IndexModel : PageModel
    {
        private readonly IExerciseRepository _exerciseRepository;

        public IndexModel(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public IEnumerable<Exercise> Exercises { get; set; } = new List<Exercise>();
        public string? SearchTerm { get; set; }
        public string? FilterMuscle { get; set; }

        public async Task OnGetAsync(string? searchTerm, string? filterMuscle)
        {
            SearchTerm = searchTerm;
            FilterMuscle = filterMuscle;

            var exercises = await _exerciseRepository.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm?.Trim()))
            {
                exercises = exercises.Where(e =>
                    e.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(e.Description) && e.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Apply muscle group filter
            if (!string.IsNullOrEmpty(filterMuscle))
            {
                exercises = exercises.Where(e => e.PrimaryMuscle == filterMuscle);
            }

            Exercises = exercises;
        }

        public IEnumerable<string> GetUniqueMuscleGroups()
        {
            return Exercises.Select(e => e.PrimaryMuscle)
                           .Distinct()
                           .OrderBy(m => m);
        }
    }
}
