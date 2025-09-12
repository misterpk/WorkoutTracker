using Microsoft.Playwright;
using NUnit.Framework;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;
using WorkoutTracker.Web.E2ETests.PageObjects;

namespace WorkoutTracker.Web.E2ETests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class ExerciseCriticalJourneyTests : E2ETestBase
{
    private ExercisesPage _exercisesPage = null!;

    [SetUp]
    public void SetUp()
    {
        _exercisesPage = new ExercisesPage(Page, BaseUrl);
        
        // Configure test to run in Chromium headless by default
        // The server is automatically started and managed by E2ETestBase
        // BaseUrl is dynamically determined from the TestWebApplicationFactory
    }

    [Test]
    public async Task User_CompletesFullExerciseLifecycle_HappyPath()
    {
        // This test covers the complete user journey that integration tests cannot:
        // - Visual feedback and UI interactions
        // - Browser-specific behavior
        // - JavaScript interactions (if any)
        // - Real form submission workflows
        
        var uniqueExerciseName = $"E2E Test Exercise {Guid.NewGuid().ToString()[..8]}";
        
        // Navigate to Exercises page
        await _exercisesPage.NavigateToIndexAsync();
        await _exercisesPage.ShouldBeOnIndexPageAsync();

        // Click Create New Exercise button (tests JavaScript/navigation)
        await _exercisesPage.ClickCreateExerciseAsync();
        await _exercisesPage.ShouldBeOnCreatePageAsync();

        // Fill out the form with visual feedback validation
        await _exercisesPage.FillExerciseFormAsync(
            name: uniqueExerciseName,
            primaryMuscle: "Chest",
            description: "E2E test exercise for automated testing"
        );

        // Submit form and verify redirect behavior
        await _exercisesPage.SubmitFormAsync();
        
        // Wait for navigation to complete and verify redirect to Details page
        await Page.WaitForURLAsync(new Regex(@".*/(Exercises/Details/\d+|Exercises).*"));
        await Expect(Page).ToHaveURLAsync(new Regex(@"/Exercises/Details/\d+"));
        await Expect(Page.Locator("h4.mb-0")).ToContainTextAsync(uniqueExerciseName);

        // Navigate back to index and verify exercise appears in list
        await _exercisesPage.NavigateToIndexAsync();
        await _exercisesPage.ShouldShowExerciseInListAsync(uniqueExerciseName);
    }

    [Test]
    public async Task User_SearchesAndFiltersExercises_InteractiveFeatures()
    {
        // This test specifically covers interactive search/filter features
        // that may have JavaScript behavior not covered by integration tests
        
        await _exercisesPage.NavigateToIndexAsync();
        
        // Test real-time search (if implemented with JavaScript)
        await _exercisesPage.SearchExercisesAsync("Push");
        
        // Verify search results update
        await _exercisesPage.ShouldHaveExerciseCountAsync(0); // Assuming no "Push" exercises exist
        
        // Clear search and test muscle filter dropdown
        await _exercisesPage.SearchExercisesAsync("");
        
        // Test muscle group filter dropdown interaction
        await _exercisesPage.FilterByMuscleGroupAsync("Chest");
        
        // This tests the actual browser dropdown behavior and form submission
    }

    [Test]
    public async Task User_EncountersFormValidation_VisualFeedback()
    {
        // This test covers visual validation feedback that integration tests miss
        
        await _exercisesPage.NavigateToCreateAsync();
        await _exercisesPage.ShouldBeOnCreatePageAsync();

        // Submit empty form to trigger validation
        await _exercisesPage.SubmitFormAsync();
        
        // Verify validation messages appear visually
        await Expect(Page.Locator(".text-danger, .invalid-feedback")).ToBeVisibleAsync();
        
        // Verify form doesn't redirect on validation failure
        await _exercisesPage.ShouldBeOnCreatePageAsync();
        
        // Test that filling required fields removes validation errors
        await _exercisesPage.FillExerciseFormAsync("Valid Exercise Name", "Chest");
        
        // Submit valid form
        await _exercisesPage.SubmitFormAsync();
        
        // Should successfully redirect (proving validation cleared)
        await Expect(Page).ToHaveURLAsync(new Regex(@"/Exercises/Details/\d+"));
    }

    [Test]
    public async Task User_NavigatesWithBrowserButtons_BrowserBehavior()
    {
        // This test covers browser-specific navigation behavior
        // that integration tests cannot test (back button, history, etc.)
        
        var uniqueName = $"Navigation Test {Guid.NewGuid().ToString()[..8]}";
        
        // Create an exercise first
        await _exercisesPage.NavigateToCreateAsync();
        await _exercisesPage.FillExerciseFormAsync(uniqueName, "Back");
        await _exercisesPage.SubmitFormAsync();
        
        // Navigate to index
        await _exercisesPage.NavigateToIndexAsync();
        
        // Use browser back button
        await Page.GoBackAsync();
        
        // Should be back on details page
        await Expect(Page.Locator("h4.mb-0")).ToContainTextAsync(uniqueName);
        
        // Use browser forward button  
        await Page.GoForwardAsync();
        
        // Should be back on index
        await _exercisesPage.ShouldBeOnIndexPageAsync();
    }

    [Test]
    public async Task User_ExperiencesResponsiveDesign_MobileViewport()
    {
        // This test covers responsive design behavior at mobile viewport
        // Integration tests cannot test viewport-specific rendering
        
        // Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        
        await _exercisesPage.NavigateToIndexAsync();
        
        // Verify page is still functional at mobile size
        await _exercisesPage.ShouldBeOnIndexPageAsync();
        
        // Test that create button is still accessible
        await _exercisesPage.ClickCreateExerciseAsync();
        await _exercisesPage.ShouldBeOnCreatePageAsync();
        
        // Verify form is still usable in mobile viewport
        await Expect(Page.Locator("input[name='Exercise.Name']")).ToBeVisibleAsync();
        await Expect(Page.Locator("input[name='Exercise.PrimaryMuscle']")).ToBeVisibleAsync();
    }

    // Note: In a real setup, we would have:
    // - Database cleanup between tests
    // - Configurable base URL via environment variables  
    // - Cross-browser testing (Chrome, Firefox, Safari)
    // - Screenshot capture on test failures
    // - Video recording for failed tests
    // - Parallel test execution configuration
}