using FluentAssertions;
using AngleSharp.Html.Dom;
using WorkoutTracker.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;

namespace WorkoutTracker.Web.IntegrationTests;

public class ProgramWorkflowIntegrationTests : WebIntegrationTestBase
{
    public ProgramWorkflowIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ProgramIndex_Get_ReturnsProgramListPage()
    {
        var response = await Client.GetAsync("/Programs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task ProgramIndex_WithPrograms_DisplaysProgramsInCards()
    {
        // Arrange - Clear database first to ensure clean state
        await ClearDatabaseAsync();
        
        // Seed database with test programs
        await SeedDatabaseAsync(context =>
        {
            context.Programs.AddRange(
                new Program { Name = "Push Day", Description = "Upper body push exercises" },
                new Program { Name = "Pull Day", Description = "Upper body pull exercises" }
            );
        });

        // Act
        var document = await GetDocumentAsync("/Programs");

        // Assert
        document.Title.Should().Contain("Programs");
        
        // Look for program cards
        var programCards = document.QuerySelectorAll(".card-title");
        programCards.Should().HaveCountGreaterOrEqualTo(2);
        
        var cardTitles = programCards.Select(card => card.TextContent.Trim()).ToList();
        cardTitles.Should().Contain("Push Day");
        cardTitles.Should().Contain("Pull Day");
    }

    [Fact]
    public async Task ProgramIndex_WithSearchTerm_FiltersPrograms()
    {
        // Arrange
        await ClearDatabaseAsync();
        await SeedDatabaseAsync(context =>
        {
            context.Programs.AddRange(
                new Program { Name = "Push Day", Description = "Upper body push exercises" },
                new Program { Name = "Pull Day", Description = "Upper body pull exercises" },
                new Program { Name = "Leg Day", Description = "Lower body exercises" }
            );
        });

        // Act
        var document = await GetDocumentAsync("/Programs?searchTerm=upper");

        // Assert
        var programCards = document.QuerySelectorAll(".card-title");
        var cardTitles = programCards.Select(card => card.TextContent.Trim()).ToList();
        
        cardTitles.Should().Contain("Push Day");
        cardTitles.Should().Contain("Pull Day");
        cardTitles.Should().NotContain("Leg Day");
    }

    [Fact]
    public async Task ProgramCreate_Get_ReturnsCreateForm()
    {
        var response = await Client.GetAsync("/Programs/Create");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var document = await GetDocumentAsync("/Programs/Create");
        document.QuerySelector("form").Should().NotBeNull();
        document.QuerySelector("input[name='Program.Name']").Should().NotBeNull();
        document.QuerySelector("textarea[name='Program.Description']").Should().NotBeNull();
    }

    [Fact]
    public async Task ProgramCreate_Post_WithValidData_CreatesProgram()
    {
        // Arrange
        await ClearDatabaseAsync();
        var createPage = await GetDocumentAsync("/Programs/Create");
        var form = createPage.QuerySelector("form") as IHtmlFormElement;
        form.Should().NotBeNull();

        var formData = ExtractFormData(form!);
        formData["Program.Name"] = "Test Program";
        formData["Program.Description"] = "Test Description";

        // Act
        var response = await SubmitFormAsync(form!, formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().StartWith("/Programs/Details/");

        // Verify program was created in database
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var createdProgram = context.Programs.FirstOrDefault(p => p.Name == "Test Program");
        
        createdProgram.Should().NotBeNull();
        createdProgram!.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task ProgramCreate_Post_WithInvalidData_ReturnsValidationErrors()
    {
        // Arrange
        var createPage = await GetDocumentAsync("/Programs/Create");
        var form = createPage.QuerySelector("form") as IHtmlFormElement;
        form.Should().NotBeNull();

        var formData = ExtractFormData(form!);
        formData["Program.Name"] = ""; // Invalid: empty name
        formData["Program.Description"] = "Valid description";

        // Act
        var response = await SubmitFormAsync(form!, formData, followRedirects: false);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Program name is required");
    }

    [Fact]
    public async Task ProgramDetails_Get_WithValidId_ShowsProgramDetails()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var programId = 0;
        await SeedDatabaseAsync(context =>
        {
            var program = new Program 
            { 
                Name = "Test Program", 
                Description = "Test Description" 
            };
            context.Programs.Add(program);
            context.SaveChanges();
            programId = program.Id;
        });

        // Act
        var document = await GetDocumentAsync($"/Programs/Details/{programId}");

        // Assert
        document.QuerySelector("h1").Should().NotBeNull();
        document.QuerySelector("h1")!.TextContent.Should().Contain("Test Program");
        
        var description = document.QuerySelector(".lead");
        description.Should().NotBeNull();
        description!.TextContent.Should().Contain("Test Description");
    }

    [Fact]
    public async Task ProgramDetails_Get_WithNonExistentId_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/Programs/Details/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProgramEdit_Get_WithValidId_ReturnsEditForm()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var programId = 0;
        await SeedDatabaseAsync(context =>
        {
            var program = new Program 
            { 
                Name = "Original Program", 
                Description = "Original Description" 
            };
            context.Programs.Add(program);
            context.SaveChanges();
            programId = program.Id;
        });

        // Act
        var document = await GetDocumentAsync($"/Programs/Edit/{programId}");

        // Assert
        var nameInput = document.QuerySelector("input[name='ProgramDto.Name']") as IHtmlInputElement;
        nameInput.Should().NotBeNull();
        nameInput!.Value.Should().Be("Original Program");

        var descriptionTextarea = document.QuerySelector("textarea[name='ProgramDto.Description']") as IHtmlTextAreaElement;
        descriptionTextarea.Should().NotBeNull();
        descriptionTextarea!.Value.Should().Be("Original Description");
    }

    [Fact]
    public async Task ProgramEdit_Post_WithValidData_UpdatesProgram()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var programId = 0;
        await SeedDatabaseAsync(context =>
        {
            var program = new Program 
            { 
                Name = "Original Program", 
                Description = "Original Description" 
            };
            context.Programs.Add(program);
            context.SaveChanges();
            programId = program.Id;
        });

        var editPage = await GetDocumentAsync($"/Programs/Edit/{programId}");
        var form = editPage.QuerySelector("form") as IHtmlFormElement;
        form.Should().NotBeNull();

        var formData = ExtractFormData(form!);
        formData["ProgramDto.Name"] = "Updated Program";
        formData["ProgramDto.Description"] = "Updated Description";

        // Act
        var response = await SubmitFormAsync(form!, formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be($"/Programs/Details/{programId}");

        // Verify program was updated in database
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var updatedProgram = context.Programs.Find(programId);
        
        updatedProgram.Should().NotBeNull();
        updatedProgram!.Name.Should().Be("Updated Program");
        updatedProgram.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task ProgramAddExercise_Get_WithValidProgramId_ReturnsAddExerciseForm()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var programId = 0;
        await SeedDatabaseAsync(context =>
        {
            var program = new Program 
            { 
                Name = "Test Program", 
                Description = "Test Description" 
            };
            var exercise = new Exercise
            {
                Name = "Test Exercise",
                PrimaryMuscle = "Chest",
                Description = "Test exercise description"
            };
            context.Programs.Add(program);
            context.Exercises.Add(exercise);
            context.SaveChanges();
            programId = program.Id;
        });

        // Act
        var document = await GetDocumentAsync($"/Programs/AddExercise/{programId}");

        // Assert
        document.QuerySelector("form").Should().NotBeNull();
        document.QuerySelector("select[name='ProgramExercise.ExerciseId']").Should().NotBeNull();
        document.QuerySelector("input[name='ProgramExercise.PlannedSets']").Should().NotBeNull();
        document.QuerySelector("input[name='ProgramExercise.PlannedReps']").Should().NotBeNull();
        
        // Check that exercise appears in dropdown
        var exerciseSelect = document.QuerySelector("select[name='ProgramExercise.ExerciseId']");
        exerciseSelect!.TextContent.Should().Contain("Test Exercise (Chest)");
    }

    [Fact]
    public async Task ProgramAddExercise_Post_WithValidData_AddsExerciseToProgram()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var programId = 0;
        var exerciseId = 0;
        await SeedDatabaseAsync(context =>
        {
            var program = new Program 
            { 
                Name = "Test Program", 
                Description = "Test Description" 
            };
            var exercise = new Exercise
            {
                Name = "Test Exercise",
                PrimaryMuscle = "Chest",
                Description = "Test exercise description"
            };
            context.Programs.Add(program);
            context.Exercises.Add(exercise);
            context.SaveChanges();
            programId = program.Id;
            exerciseId = exercise.Id;
        });

        var addExercisePage = await GetDocumentAsync($"/Programs/AddExercise/{programId}");
        var form = addExercisePage.QuerySelector("form") as IHtmlFormElement;
        form.Should().NotBeNull();

        var formData = ExtractFormData(form!);
        formData["ProgramExercise.ExerciseId"] = exerciseId.ToString();
        formData["ProgramExercise.Order"] = "1";
        formData["ProgramExercise.PlannedSets"] = "3";
        formData["ProgramExercise.PlannedReps"] = "10";
        formData["ProgramExercise.PlannedWeight"] = "135";
        formData["ProgramExercise.PlannedRestTimeSeconds"] = "90";

        // Act
        var response = await SubmitFormAsync(form!, formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be($"/Programs/Details/{programId}");

        // Verify program exercise was created in database
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        var programExercise = context.ProgramExercises.FirstOrDefault(pe => 
            pe.ProgramId == programId && pe.ExerciseId == exerciseId);
        
        programExercise.Should().NotBeNull();
        programExercise!.PlannedSets.Should().Be(3);
        programExercise.PlannedReps.Should().Be(10);
        programExercise.PlannedWeight.Should().Be(135m);
        programExercise.PlannedRestTimeSeconds.Should().Be(90);
    }

    [Fact]
    public async Task CompleteWorkflow_CreateProgramAndAddExercise_WorksEndToEnd()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        // First create an exercise to use
        await SeedDatabaseAsync(context =>
        {
            var exercise = new Exercise
            {
                Name = "Bench Press",
                PrimaryMuscle = "Chest",
                Description = "Chest exercise"
            };
            context.Exercises.Add(exercise);
        });

        // Step 1: Create a program
        var createPage = await GetDocumentAsync("/Programs/Create");
        var createForm = createPage.QuerySelector("form") as IHtmlFormElement;
        var createFormData = ExtractFormData(createForm!);
        createFormData["Program.Name"] = "My Workout Program";
        createFormData["Program.Description"] = "Complete workout routine";

        var createResponse = await SubmitFormAsync(createForm!, createFormData);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        
        // Extract program ID from redirect URL
        var location = createResponse.Headers.Location?.ToString();
        location.Should().StartWith("/Programs/Details/");
        var programId = int.Parse(location!.Split('/').Last());

        // Step 2: Add exercise to the program
        var addExercisePage = await GetDocumentAsync($"/Programs/AddExercise/{programId}");
        var addExerciseForm = addExercisePage.QuerySelector("form") as IHtmlFormElement;
        var addExerciseFormData = ExtractFormData(addExerciseForm!);
        
        // Get the exercise ID from the dropdown
        var exerciseSelect = addExercisePage.QuerySelector("select[name='ProgramExercise.ExerciseId'] option[value]:not([value=''])");
        var exerciseId = exerciseSelect!.GetAttribute("value");
        
        addExerciseFormData["ProgramExercise.ExerciseId"] = exerciseId!;
        addExerciseFormData["ProgramExercise.Order"] = "1";
        addExerciseFormData["ProgramExercise.PlannedSets"] = "4";
        addExerciseFormData["ProgramExercise.PlannedReps"] = "8";
        addExerciseFormData["ProgramExercise.PlannedWeight"] = "185";
        addExerciseFormData["ProgramExercise.PlannedRestTimeSeconds"] = "120";

        var addExerciseResponse = await SubmitFormAsync(addExerciseForm!, addExerciseFormData);
        addExerciseResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        // Step 3: Verify the complete program details
        var detailsPage = await GetDocumentAsync($"/Programs/Details/{programId}");
        
        // Check program name and description
        detailsPage.QuerySelector("h1")!.TextContent.Should().Contain("My Workout Program");
        detailsPage.QuerySelector(".lead")!.TextContent.Should().Contain("Complete workout routine");
        
        // Check that the exercise is listed in the program
        detailsPage.TextContent.Should().Contain("Bench Press");
        detailsPage.TextContent.Should().Contain("4 × 8"); // Sets × Reps
        detailsPage.TextContent.Should().Contain("185"); // Weight
    }

    private async Task<int> GetProgramCountAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTracker.Infrastructure.Data.WorkoutTrackerDbContext>();
        return context.Programs.Count();
    }
}