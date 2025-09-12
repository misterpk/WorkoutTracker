using FluentAssertions;
using AngleSharp.Html.Dom;
using WorkoutTracker.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace WorkoutTracker.Web.IntegrationTests;

public class ExerciseWorkflowIntegrationTests : WebIntegrationTestBase
{
    public ExerciseWorkflowIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ExerciseIndex_Get_ReturnsExerciseListPage()
    {
        var response = await Client.GetAsync("/Exercises");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task ExerciseIndex_WithExercises_DisplaysExercisesInList()
    {
        // Arrange - Clear database first to ensure clean state
        await ClearDatabaseAsync();
        
        // Seed database with test exercises
        await SeedDatabaseAsync(context =>
        {
            context.Exercises.AddRange(
                new Exercise { Name = "Bench Press", PrimaryMuscle = "Chest", Description = "Upper body exercise" },
                new Exercise { Name = "Squat", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
            );
        });

        // Debug - Check if data was actually seeded
        var countAfterSeed = await GetExerciseCountAsync();
        countAfterSeed.Should().Be(2, "Exercises should be seeded in database");

        // Act
        var document = await GetDocumentAsync("/Exercises");

        // Assert
        document.Title.Should().Contain("Exercises");
        
        // Look for exercise cards instead of table rows (the page uses card layout)
        var exerciseCards = document.QuerySelectorAll(".card-title");
        var exerciseNames = exerciseCards.Where(card => 
            card.TextContent?.Contains("Bench Press") == true || card.TextContent?.Contains("Squat") == true);
        
        exerciseNames.Should().HaveCount(2, "Expected both seeded exercises to appear in the card layout");
    }

    [Fact]
    public async Task ExerciseCreate_Get_ReturnsCreateForm()
    {
        // Act
        var document = await GetDocumentAsync("/Exercises/Create");

        // Assert
        document.Title.Should().Contain("Add New Exercise");
        
        var nameInput = document.QuerySelector("input[name='Exercise.Name']") as IHtmlInputElement;
        var muscleInput = document.QuerySelector("input[name='Exercise.PrimaryMuscle']") as IHtmlInputElement;
        var descriptionTextarea = document.QuerySelector("textarea[name='Exercise.Description']") as IHtmlTextAreaElement;
        
        nameInput.Should().NotBeNull();
        muscleInput.Should().NotBeNull();
        descriptionTextarea.Should().NotBeNull();
    }

    [Fact]
    public async Task ExerciseCreate_ValidData_RedirectsToDetails()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["Exercise.Name"] = "Deadlift",
            ["Exercise.PrimaryMuscle"] = "Back",
            ["Exercise.Description"] = "Compound pulling exercise"
        };

        // Act
        var response = await PostFormAsync("/Exercises/Create", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().StartWith("/Exercises/Details/");

        // Verify the exercise was created in the database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var createdExercise = context.Exercises.FirstOrDefault(e => e.Name == "Deadlift");
        
        createdExercise.Should().NotBeNull();
        createdExercise!.Name.Should().Be("Deadlift");
        createdExercise.PrimaryMuscle.Should().Be("Back");
        createdExercise.Description.Should().Be("Compound pulling exercise");
    }

    [Fact]
    public async Task ExerciseCreate_InvalidData_ReturnsValidationErrors()
    {
        // Arrange - Submit form with missing required fields
        var formData = new Dictionary<string, string>
        {
            ["Exercise.Name"] = "", // Empty name should trigger validation error
            ["Exercise.PrimaryMuscle"] = "",
            ["Exercise.Description"] = "Some description"
        };

        // Act
        var response = await PostFormAsync("/Exercises/Create", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Returns form with validation errors
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Exercise name is required");
        content.Should().Contain("Primary muscle group is required");
    }

    [Fact]
    public async Task ExerciseDetails_ValidId_DisplaysExerciseInformation()
    {
        // Arrange - Create exercise in database
        var createdExercise = await SeedEntityAsync(new Exercise 
        { 
            Name = "Pull-ups", 
            PrimaryMuscle = "Back", 
            Description = "Upper body pulling exercise" 
        });

        // Debug - Verify the exercise has a valid ID
        createdExercise.Id.Should().BeGreaterThan(0, "Seeded exercise should have a valid ID");

        // Act
        var document = await GetDocumentAsync($"/Exercises/Details/{createdExercise.Id}");

        // Assert
        document.Title.Should().Contain("Pull-ups");
        document.Body!.TextContent.Should().Contain("Pull-ups");
        document.Body.TextContent.Should().Contain("Back");
        document.Body.TextContent.Should().Contain("Upper body pulling exercise");
    }

    [Fact]
    public async Task ExerciseDetails_InvalidId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync("/Exercises/Details/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExerciseEdit_ValidId_PrePopulatesForm()
    {
        // Arrange - Create exercise in database
        var createdExercise = await SeedEntityAsync(new Exercise 
        { 
            Name = "Push-ups", 
            PrimaryMuscle = "Chest", 
            Description = "Bodyweight exercise" 
        });

        // Act
        var document = await GetDocumentAsync($"/Exercises/Edit/{createdExercise.Id}");

        // Assert
        document.Title.Should().Contain("Edit");
        
        var nameInput = document.QuerySelector("input[name='Exercise.Name']") as IHtmlInputElement;
        var muscleInput = document.QuerySelector("input[name='Exercise.PrimaryMuscle']") as IHtmlInputElement;
        var descriptionTextarea = document.QuerySelector("textarea[name='Exercise.Description']") as IHtmlTextAreaElement;
        
        nameInput?.Value.Should().Be("Push-ups");
        muscleInput?.Value.Should().Be("Chest");
        descriptionTextarea?.Value.Should().Be("Bodyweight exercise");
    }

    [Fact]
    public async Task ExerciseEdit_ValidData_UpdatesAndRedirects()
    {
        // Arrange - Create exercise in database
        var createdExercise = await SeedEntityAsync(new Exercise 
        { 
            Name = "Original Name", 
            PrimaryMuscle = "Original Muscle", 
            Description = "Original description" 
        });

        var formData = new Dictionary<string, string>
        {
            ["Exercise.Id"] = createdExercise.Id.ToString(),
            ["Exercise.Name"] = "Updated Name",
            ["Exercise.PrimaryMuscle"] = "Updated Muscle",
            ["Exercise.Description"] = "Updated description"
        };

        // Act
        var response = await PostFormAsync($"/Exercises/Edit/{createdExercise.Id}", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be($"/Exercises/Details/{createdExercise.Id}");

        // Verify the exercise was updated in the database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var updatedExercise = context.Exercises.Find(createdExercise.Id);
        
        updatedExercise.Should().NotBeNull();
        updatedExercise!.Name.Should().Be("Updated Name");
        updatedExercise.PrimaryMuscle.Should().Be("Updated Muscle");
        updatedExercise.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task ExerciseDelete_ValidId_ShowsConfirmationPage()
    {
        // Arrange - Create exercise in database
        var createdExercise = await SeedEntityAsync(new Exercise 
        { 
            Name = "To Be Deleted", 
            PrimaryMuscle = "Test Muscle", 
            Description = "This exercise will be deleted" 
        });

        // Act
        var document = await GetDocumentAsync($"/Exercises/Delete/{createdExercise.Id}");

        // Assert
        document.Title.Should().Contain("Delete");
        document.Body!.TextContent.Should().Contain("To Be Deleted");
        document.Body.TextContent.Should().Contain("Are you sure you want to delete");
        
        var deleteForm = document.QuerySelector("form[method='post']");
        deleteForm.Should().NotBeNull();
    }

    [Fact]
    public async Task ExerciseDelete_Post_DeletesExerciseAndRedirects()
    {
        // Arrange - Create exercise in database
        var createdExercise = await SeedEntityAsync(new Exercise 
        { 
            Name = "To Be Deleted", 
            PrimaryMuscle = "Test Muscle", 
            Description = "This exercise will be deleted" 
        });

        var formData = new Dictionary<string, string>
        {
            ["Exercise.Id"] = createdExercise.Id.ToString()
        };

        // Act
        var response = await PostFormAsync($"/Exercises/Delete/{createdExercise.Id}", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be("/Exercises");

        // Verify the exercise was deleted from the database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var deletedExercise = context.Exercises.Find(createdExercise.Id);
        
        deletedExercise.Should().BeNull();
    }
}