using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace WorkoutTracker.Web.E2ETests.PageObjects;

public class ExercisesPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ExercisesPage(IPage page, string baseUrl = "http://localhost:5000")
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Locators
    private ILocator CreateExerciseButton => _page.Locator("a.btn.btn-primary[href='/Exercises/Create']");
    private ILocator ExerciseNameInput => _page.Locator("input[name='Exercise.Name']");
    private ILocator PrimaryMuscleInput => _page.Locator("input[name='Exercise.PrimaryMuscle']");
    private ILocator DescriptionTextarea => _page.Locator("textarea[name='Exercise.Description']");
    private ILocator SubmitButton => _page.Locator("button[type='submit']:has-text('Create'), button[type='submit']:has-text('Save')");
    private ILocator SearchInput => _page.Locator("input[name='searchTerm']");
    private ILocator MuscleFilter => _page.Locator("select[name='filterMuscle']");
    private ILocator ExerciseCards => _page.Locator(".card:has(.card-title)");
    private ILocator SuccessMessage => _page.Locator(".alert-success, .alert.alert-success");

    // Navigation
    public async Task NavigateToIndexAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/Exercises");
    }

    public async Task NavigateToCreateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/Exercises/Create");
    }

    // Actions
    public async Task ClickCreateExerciseAsync()
    {
        await CreateExerciseButton.ClickAsync();
    }

    public async Task FillExerciseFormAsync(string name, string primaryMuscle, string description = "")
    {
        await ExerciseNameInput.FillAsync(name);
        await PrimaryMuscleInput.FillAsync(primaryMuscle);
        if (!string.IsNullOrEmpty(description))
        {
            await DescriptionTextarea.FillAsync(description);
        }
    }

    public async Task SubmitFormAsync()
    {
        await SubmitButton.ClickAsync();
    }

    public async Task SearchExercisesAsync(string searchTerm)
    {
        await SearchInput.FillAsync(searchTerm);
        await _page.Keyboard.PressAsync("Enter");
    }

    public async Task FilterByMuscleGroupAsync(string muscleGroup)
    {
        await MuscleFilter.SelectOptionAsync(muscleGroup);
    }

    // Assertions
    public async Task ShouldBeOnIndexPageAsync()
    {
        await Expect(_page).ToHaveTitleAsync(new Regex("Exercises"));
        await Expect(_page.Locator("h1")).ToContainTextAsync("Exercise Library");
    }

    public async Task ShouldBeOnCreatePageAsync()
    {
        await Expect(_page).ToHaveTitleAsync(new Regex("Add New Exercise"));
        await Expect(ExerciseNameInput).ToBeVisibleAsync();
        await Expect(PrimaryMuscleInput).ToBeVisibleAsync();
        await Expect(DescriptionTextarea).ToBeVisibleAsync();
    }

    public async Task ShouldShowExerciseInListAsync(string exerciseName)
    {
        var exerciseCard = _page.Locator($".card:has(.card-title:has-text('{exerciseName}'))");
        await Expect(exerciseCard).ToBeVisibleAsync();
    }

    public async Task ShouldNotShowExerciseInListAsync(string exerciseName)
    {
        var exerciseCard = _page.Locator($".card:has(.card-title:has-text('{exerciseName}'))");
        await Expect(exerciseCard).Not.ToBeVisibleAsync();
    }

    public async Task ShouldShowSuccessMessageAsync(string? message = null)
    {
        await Expect(SuccessMessage).ToBeVisibleAsync();
        if (message != null)
        {
            await Expect(SuccessMessage).ToContainTextAsync(message);
        }
    }

    public async Task ShouldHaveExerciseCountAsync(int expectedCount)
    {
        if (expectedCount == 0)
        {
            // If no exercises expected, check for empty state message or no cards
            var noExercisesMessage = _page.Locator(":has-text('No exercises found'), :has-text('No exercises available')");
            var isEmptyCards = await ExerciseCards.CountAsync() == 0;
            var hasEmptyMessage = await noExercisesMessage.CountAsync() > 0;
            
            if (!isEmptyCards && !hasEmptyMessage)
            {
                throw new Exception($"Expected no exercises, but found {await ExerciseCards.CountAsync()} cards");
            }
        }
        else
        {
            await Expect(ExerciseCards).ToHaveCountAsync(expectedCount);
        }
    }

    // Fluent API methods for chaining
    public ExercisesPage ThenNavigateToCreate()
    {
        NavigateToCreateAsync().Wait();
        return this;
    }

    public ExercisesPage ThenFillForm(string name, string primaryMuscle, string description = "")
    {
        FillExerciseFormAsync(name, primaryMuscle, description).Wait();
        return this;
    }

    public ExercisesPage ThenSubmit()
    {
        SubmitFormAsync().Wait();
        return this;
    }

    public ExercisesPage ThenShouldShowInList(string exerciseName)
    {
        ShouldShowExerciseInListAsync(exerciseName).Wait();
        return this;
    }
}