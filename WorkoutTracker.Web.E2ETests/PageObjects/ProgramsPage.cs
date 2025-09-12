using Microsoft.Playwright;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace WorkoutTracker.Web.E2ETests.PageObjects;

public class ProgramsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ProgramsPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Navigation methods
    public async Task NavigateToIndexAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/Programs");
    }

    // Page verification methods
    public async Task ShouldBeOnIndexPageAsync()
    {
        await Expect(_page).ToHaveURLAsync(new Regex(@".*\/Programs(\?.*)?$"));
        await Expect(_page.Locator("h1")).ToContainTextAsync("Workout Programs");
    }

    public async Task ShouldBeOnCreatePageAsync()
    {
        await Expect(_page).ToHaveURLAsync(new Regex(@".*\/Programs\/Create"));
        await Expect(_page.Locator("h1")).ToContainTextAsync("Create New Program");
    }

    public async Task ShouldBeOnDetailsPageAsync()
    {
        await Expect(_page).ToHaveURLAsync(new Regex(@".*\/Programs\/Details\/\d+"));
    }

    public async Task ShouldBeOnEditPageAsync()
    {
        await Expect(_page).ToHaveURLAsync(new Regex(@".*\/Programs\/Edit\/\d+"));
        await Expect(_page.Locator("h1")).ToContainTextAsync("Edit Program");
    }

    public async Task ShouldBeOnAddExercisePageAsync()
    {
        await Expect(_page).ToHaveURLAsync(new Regex(@".*\/Programs\/AddExercise\/\d+"));
        await Expect(_page.Locator("h1")).ToContainTextAsync("Add Exercise");
    }

    // Action methods for Index page
    public async Task ClickCreateProgramAsync()
    {
        await _page.ClickAsync("text=Create New Program");
    }

    public async Task SearchProgramsAsync(string searchTerm)
    {
        await _page.FillAsync("input[name='searchTerm']", searchTerm);
        await _page.ClickAsync("button[type='submit']");
    }

    public async Task ClickProgramCardAsync(string programName)
    {
        // Look for the View button in the card containing the program name
        var programCard = _page.Locator(".card").Filter(new() { HasText = programName });
        await programCard.Locator("text=View").ClickAsync();
    }

    public async Task ClickCopyProgramAsync(string programName)
    {
        // Look for the copy button in the card containing the program name
        var programCard = _page.Locator(".card").Filter(new() { HasText = programName });
        
        // Check if there's a dropdown for more actions
        var moreButton = programCard.Locator("text=More");
        if (await moreButton.CountAsync() > 0)
        {
            await moreButton.ClickAsync();
            await _page.ClickAsync("text=Copy Program");
        }
        else
        {
            // If no dropdown, look for direct copy button
            await programCard.Locator("text=Copy").ClickAsync();
        }
    }

    // Action methods for Create/Edit forms
    public async Task FillProgramFormAsync(string name, string description)
    {
        await _page.FillAsync("input[name='Program.Name']", name);
        await _page.FillAsync("textarea[name='Program.Description']", description);
    }

    public async Task FillEditProgramFormAsync(string name, string description)
    {
        await _page.FillAsync("input[name='ProgramDto.Name']", name);
        await _page.FillAsync("textarea[name='ProgramDto.Description']", description);
    }

    public async Task SubmitProgramFormAsync()
    {
        await _page.ClickAsync("button[type='submit']");
    }

    public async Task SubmitEditProgramFormAsync()
    {
        await _page.ClickAsync("button[type='submit']");
    }

    // Action methods for Details page
    public async Task ClickEditProgramAsync()
    {
        await _page.ClickAsync("text=Edit Program");
    }

    public async Task ClickAddExerciseAsync()
    {
        await _page.ClickAsync("text=Add Exercise");
    }

    public async Task ClickBackToProgramsAsync()
    {
        await _page.ClickAsync("text=Back to Programs");
    }

    // Action methods for Add Exercise page
    public async Task FillAddExerciseFormAsync(string exerciseName, string sets, string reps, string weight, string restTime)
    {
        // Select exercise from dropdown
        await _page.SelectOptionAsync("select[name='ProgramExercise.ExerciseId']", new SelectOptionValue() { Label = exerciseName });
        
        // Fill in the exercise parameters
        await _page.FillAsync("input[name='ProgramExercise.PlannedSets']", sets);
        await _page.FillAsync("input[name='ProgramExercise.PlannedReps']", reps);
        await _page.FillAsync("input[name='ProgramExercise.PlannedWeight']", weight);
        await _page.SelectOptionAsync("select[name='ProgramExercise.PlannedRestTimeSeconds']", restTime);
    }

    public async Task SubmitAddExerciseFormAsync()
    {
        await _page.ClickAsync("text=Add Exercise");
    }

    // Helper methods for verification
    public async Task<int> GetProgramCountAsync()
    {
        return await _page.Locator(".card-title").CountAsync();
    }

    public async Task<bool> HasProgramWithNameAsync(string programName)
    {
        var programCards = await _page.Locator(".card-title").AllTextContentsAsync();
        return programCards.Any(title => title.Contains(programName));
    }

    public async Task<bool> HasExerciseInProgramAsync(string exerciseName)
    {
        var tableContent = await _page.Locator("table").TextContentAsync();
        return tableContent?.Contains(exerciseName) ?? false;
    }

    // Verification methods for program statistics
    public async Task<string> GetEstimatedDurationAsync()
    {
        var durationCard = _page.Locator(".card").Filter(new() { HasText = "Est. Duration" });
        var durationText = await durationCard.Locator(".text-info").TextContentAsync();
        return durationText ?? "";
    }

    public async Task<string> GetTotalSetsAsync()
    {
        var setsCard = _page.Locator(".card").Filter(new() { HasText = "Total Sets" });
        var setsText = await setsCard.Locator(".text-success").TextContentAsync();
        return setsText ?? "";
    }

    public async Task<string> GetExerciseCountAsync()
    {
        var exercisesCard = _page.Locator(".card").Filter(new() { HasText = "Exercises" });
        var countText = await exercisesCard.Locator(".text-primary").TextContentAsync();
        return countText ?? "";
    }

    // Methods for handling success/error messages
    public async Task<bool> HasSuccessMessageAsync()
    {
        return await _page.Locator(".alert-success").IsVisibleAsync();
    }

    public async Task<string> GetSuccessMessageAsync()
    {
        var successAlert = await _page.Locator(".alert-success").TextContentAsync();
        return successAlert ?? "";
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        return await _page.Locator(".alert-danger").IsVisibleAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        var errorAlert = await _page.Locator(".alert-danger").TextContentAsync();
        return errorAlert ?? "";
    }

    // Methods for form validation
    public async Task<bool> HasValidationErrorAsync(string fieldName)
    {
        var validationSpan = _page.Locator($"span[data-valmsg-for='{fieldName}']");
        return await validationSpan.IsVisibleAsync();
    }

    public async Task<string> GetValidationErrorAsync(string fieldName)
    {
        var validationSpan = _page.Locator($"span[data-valmsg-for='{fieldName}']");
        var errorText = await validationSpan.TextContentAsync();
        return errorText ?? "";
    }
}