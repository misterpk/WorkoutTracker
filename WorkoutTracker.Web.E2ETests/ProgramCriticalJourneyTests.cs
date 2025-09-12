using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;
using WorkoutTracker.Web.E2ETests.PageObjects;

namespace WorkoutTracker.Web.E2ETests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ProgramCriticalJourneyTests : E2ETestBase
{
    private ProgramsPage _programsPage = null!;
    private ExercisesPage _exercisesPage = null!;

    [SetUp]
    public void SetUp()
    {
        _programsPage = new ProgramsPage(Page, BaseUrl);
        _exercisesPage = new ExercisesPage(Page, BaseUrl);
    }

    [Test]
    public async Task User_CompletesFullProgramLifecycle_HappyPath()
    {
        // This test covers the complete user journey for program management:
        // - Creating a new program
        // - Adding exercises to the program
        // - Editing program details
        // - Viewing program details with exercises
        
        var uniqueProgramName = $"E2E Test Program {Guid.NewGuid().ToString()[..8]}";
        
        // Step 1: Navigate to Programs page
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Step 2: Create a new program
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.ShouldBeOnCreatePageAsync();

        await _programsPage.FillProgramFormAsync(
            name: uniqueProgramName,
            description: "E2E test program for automated testing"
        );

        await _programsPage.SubmitProgramFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Step 3: Verify program was created and appears in details
        await Expect(Page.Locator("h1")).ToContainTextAsync(uniqueProgramName);
        await Expect(Page.Locator(".lead")).ToContainTextAsync("E2E test program for automated testing");

        // Step 4: Add an exercise to the program (first create the exercise)
        await _exercisesPage.NavigateToIndexAsync();
        await _exercisesPage.ShouldBeOnIndexPageAsync();

        var uniqueExerciseName = $"E2E Test Exercise {Guid.NewGuid().ToString()[..8]}";
        await _exercisesPage.ClickCreateExerciseAsync();
        await _exercisesPage.ShouldBeOnCreatePageAsync();

        await _exercisesPage.FillExerciseFormAsync(
            name: uniqueExerciseName,
            primaryMuscle: "Chest",
            description: "E2E test exercise"
        );

        await _exercisesPage.SubmitFormAsync();

        // Step 5: Go back to program and add the exercise
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ClickProgramCardAsync(uniqueProgramName);
        await _programsPage.ShouldBeOnDetailsPageAsync();

        await _programsPage.ClickAddExerciseAsync();
        await _programsPage.ShouldBeOnAddExercisePageAsync();

        await _programsPage.FillAddExerciseFormAsync(
            exerciseName: uniqueExerciseName,
            sets: "3",
            reps: "10",
            weight: "135",
            restTime: "90"
        );

        await _programsPage.SubmitAddExerciseFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Step 6: Verify exercise was added to program
        await Expect(Page.Locator("table")).ToContainTextAsync(uniqueExerciseName);
        await Expect(Page.Locator("table")).ToContainTextAsync("3 × 10");
        await Expect(Page.Locator("table")).ToContainTextAsync("135");

        // Step 7: Edit the program details
        await _programsPage.ClickEditProgramAsync();
        await _programsPage.ShouldBeOnEditPageAsync();

        var updatedDescription = "Updated E2E test program description";
        await _programsPage.FillEditProgramFormAsync(
            name: uniqueProgramName,
            description: updatedDescription
        );

        await _programsPage.SubmitEditProgramFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Step 8: Verify updated description
        await Expect(Page.Locator(".lead")).ToContainTextAsync(updatedDescription);

        // Step 9: Navigate back to index and verify program appears in list
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();
        await Expect(Page.Locator(".card-title")).ToContainTextAsync(uniqueProgramName);
    }

    [Test]
    public async Task User_SearchesForPrograms_FiltersCorrectly()
    {
        // This test verifies the search functionality works as expected
        
        var searchableProgramName = $"Searchable Program {Guid.NewGuid().ToString()[..8]}";
        var nonSearchableProgramName = $"Different Program {Guid.NewGuid().ToString()[..8]}";
        
        // Create two programs with different names
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Create first program
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.FillProgramFormAsync(searchableProgramName, "Searchable description");
        await _programsPage.SubmitProgramFormAsync();

        // Create second program
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.FillProgramFormAsync(nonSearchableProgramName, "Different description");
        await _programsPage.SubmitProgramFormAsync();

        // Go back to index and search
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Search for "Searchable"
        await _programsPage.SearchProgramsAsync("Searchable");

        // Verify only the searchable program appears
        await Expect(Page.Locator(".card-title")).ToContainTextAsync(searchableProgramName);
        
        // Verify the non-searchable program does not appear
        var allCardTitles = await Page.Locator(".card-title").AllTextContentsAsync();
        Assert.That(allCardTitles, Does.Not.Contain(nonSearchableProgramName));
    }

    [Test]
    public async Task User_ViewsEmptyProgramList_SeesCreatePrompt()
    {
        // This test verifies the empty state UI displays correctly
        
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // If there are no programs, should see empty state
        var programCards = await Page.Locator(".card-title").CountAsync();
        if (programCards == 0)
        {
            await Expect(Page.Locator("text=No programs found")).ToBeVisibleAsync();
            await Expect(Page.Locator("text=Get started by creating your first workout program!")).ToBeVisibleAsync();
            await Expect(Page.Locator("text=Create Your First Program")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task User_CopiesExistingProgram_CreatesExactDuplicate()
    {
        // This test verifies program copying functionality
        
        var originalProgramName = $"Original Program {Guid.NewGuid().ToString()[..8]}";
        var copiedProgramName = $"Copy of {originalProgramName}";
        
        // Create original program
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.FillProgramFormAsync(
            name: originalProgramName,
            description: "Original program description"
        );
        await _programsPage.SubmitProgramFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Go back to index and copy the program
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();
        
        // Click copy button (this might be in a dropdown or modal)
        await _programsPage.ClickCopyProgramAsync(originalProgramName);

        // Should navigate to create page with pre-filled data
        await _programsPage.ShouldBeOnCreatePageAsync();
        
        // Verify the form is pre-filled
        await Expect(Page.Locator("input[name='Program.Name']")).ToHaveValueAsync(copiedProgramName);
        await Expect(Page.Locator("textarea[name='Program.Description']")).ToHaveValueAsync("Original program description");

        // Submit the copied program
        await _programsPage.SubmitProgramFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Verify copied program was created
        await Expect(Page.Locator("h1")).ToContainTextAsync(copiedProgramName);
        await Expect(Page.Locator(".lead")).ToContainTextAsync("Original program description");
    }

    [Test]
    public async Task User_NavigatesWithBackButton_ReturnsCorrectly()
    {
        // This test verifies navigation flows work correctly
        
        var programName = $"Navigation Test Program {Guid.NewGuid().ToString()[..8]}";
        
        // Start from index
        await _programsPage.NavigateToIndexAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Go to create page
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.ShouldBeOnCreatePageAsync();

        // Use back button to return to index
        await _programsPage.ClickBackToProgramsAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Create a program to test details navigation
        await _programsPage.ClickCreateProgramAsync();
        await _programsPage.FillProgramFormAsync(programName, "Test navigation");
        await _programsPage.SubmitProgramFormAsync();
        await _programsPage.ShouldBeOnDetailsPageAsync();

        // Use back button from details to return to index
        await _programsPage.ClickBackToProgramsAsync();
        await _programsPage.ShouldBeOnIndexPageAsync();

        // Verify we can navigate back to the created program
        await _programsPage.ClickProgramCardAsync(programName);
        await _programsPage.ShouldBeOnDetailsPageAsync();
        await Expect(Page.Locator("h1")).ToContainTextAsync(programName);
    }
}