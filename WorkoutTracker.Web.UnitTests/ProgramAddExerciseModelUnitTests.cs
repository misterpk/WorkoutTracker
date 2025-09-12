using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Web.Pages.Programs;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.UnitTests;

public class ProgramAddExerciseModelUnitTests
{
    private readonly Mock<IProgramRepository> _mockProgramRepository;
    private readonly Mock<IExerciseRepository> _mockExerciseRepository;
    private readonly AddExerciseModel _addExerciseModel;

    public ProgramAddExerciseModelUnitTests()
    {
        _mockProgramRepository = new Mock<IProgramRepository>();
        _mockExerciseRepository = new Mock<IExerciseRepository>();
        _addExerciseModel = new AddExerciseModel(_mockProgramRepository.Object, _mockExerciseRepository.Object);
    }

    [Fact]
    public async Task OnGetAsync_WithValidProgramId_LoadsProgramAndExercises()
    {
        // Arrange
        var program = new ProgramModel
        {
            Id = 1,
            Name = "Test Program",
            ProgramExercises = new List<ProgramExercise>
            {
                new ProgramExercise { Order = 1 },
                new ProgramExercise { Order = 2 }
            }
        };

        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Name = "Push-ups", PrimaryMuscle = "Chest" },
            new Exercise { Id = 2, Name = "Squats", PrimaryMuscle = "Quadriceps" }
        };

        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(program);
        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        var result = await _addExerciseModel.OnGetAsync(1);

        // Assert
        result.Should().BeOfType<PageResult>();
        _addExerciseModel.Program.Should().NotBeNull();
        _addExerciseModel.Program.Name.Should().Be("Test Program");
        _addExerciseModel.ProgramExercise.ProgramId.Should().Be(1);
        _addExerciseModel.ProgramExercise.Order.Should().Be(3); // Next in sequence
        
        _addExerciseModel.ExerciseOptions.Should().HaveCount(3); // 2 exercises + 1 empty option
        _addExerciseModel.ExerciseOptions[0].Value.Should().Be("");
        _addExerciseModel.ExerciseOptions[0].Text.Should().Be("Select an exercise...");
        _addExerciseModel.ExerciseOptions[1].Text.Should().Be("Push-ups (Chest)");
        _addExerciseModel.ExerciseOptions[2].Text.Should().Be("Squats (Quadriceps)");
    }

    [Fact]
    public async Task OnGetAsync_WithNonExistentProgram_ReturnsNotFound()
    {
        // Arrange
        _mockProgramRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ProgramModel?)null);

        // Act
        var result = await _addExerciseModel.OnGetAsync(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnGetAsync_WithEmptyProgram_SetsOrderToOne()
    {
        // Arrange
        var program = new ProgramModel
        {
            Id = 1,
            Name = "Empty Program",
            ProgramExercises = new List<ProgramExercise>()
        };

        var exercises = new List<Exercise>();

        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(program);
        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        var result = await _addExerciseModel.OnGetAsync(1);

        // Assert
        result.Should().BeOfType<PageResult>();
        _addExerciseModel.ProgramExercise.Order.Should().Be(1);
    }

    [Fact]
    public async Task OnPostAsync_WithValidData_AddsExerciseToProgram()
    {
        // Arrange
        var program = new ProgramModel { Id = 1, Name = "Test Program" };
        var createdProgramExercise = new ProgramExercise
        {
            Id = 1,
            ProgramId = 1,
            ExerciseId = 2,
            Order = 1,
            PlannedSets = 3,
            PlannedReps = 10,
            PlannedWeight = 135m,
            PlannedRestTimeSeconds = 90
        };

        _addExerciseModel.ProgramExercise = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1,
            ExerciseId = 2,
            Order = 1,
            PlannedSets = 3,
            PlannedReps = 10,
            PlannedWeight = 135m,
            PlannedRestTimeSeconds = 90
        };

        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(program);
        _mockProgramRepository.Setup(r => r.AddExerciseAsync(It.IsAny<ProgramExercise>()))
            .ReturnsAsync(createdProgramExercise);

        // Act
        var result = await _addExerciseModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("Details");
        redirectResult.RouteValues["id"].Should().Be(1);

        _mockProgramRepository.Verify(r => r.AddExerciseAsync(It.Is<ProgramExercise>(pe =>
            pe.ProgramId == 1 &&
            pe.ExerciseId == 2 &&
            pe.Order == 1 &&
            pe.PlannedSets == 3 &&
            pe.PlannedReps == 10 &&
            pe.PlannedWeight == 135m &&
            pe.PlannedRestTimeSeconds == 90)), Times.Once);

        _addExerciseModel.TempData["SuccessMessage"].Should().Be("Exercise has been added to the program successfully!");
    }

    [Fact]
    public async Task OnPostAsync_WithNonExistentProgram_ReturnsNotFound()
    {
        // Arrange
        _addExerciseModel.ProgramExercise = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 999
        };

        _mockProgramRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ProgramModel?)null);

        // Act
        var result = await _addExerciseModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelState_ReturnsPage()
    {
        // Arrange
        var program = new ProgramModel { Id = 1, Name = "Test Program" };
        
        _addExerciseModel.ProgramExercise = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1
        };
        _addExerciseModel.ModelState.AddModelError("ProgramExercise.ExerciseId", "Exercise is required");

        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(program);

        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Name = "Push-ups", PrimaryMuscle = "Chest" }
        };
        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        var result = await _addExerciseModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _addExerciseModel.ExerciseOptions.Should().NotBeEmpty();
        _mockProgramRepository.Verify(r => r.AddExerciseAsync(It.IsAny<ProgramExercise>()), Times.Never);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(20, true)]
    [InlineData(21, false)]
    public void ProgramExerciseDto_PlannedSets_ValidatesRange(int sets, bool shouldBeValid)
    {
        // Arrange
        var dto = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1,
            ExerciseId = 1,
            Order = 1,
            PlannedSets = sets,
            PlannedReps = 10,
            PlannedWeight = 0,
            PlannedRestTimeSeconds = 90
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        
        if (shouldBeValid)
        {
            validationResults.Should().NotContain(vr => vr.MemberNames.Contains("PlannedSets"));
        }
        else
        {
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("PlannedSets"));
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void ProgramExerciseDto_PlannedReps_ValidatesRange(int reps, bool shouldBeValid)
    {
        // Arrange
        var dto = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1,
            ExerciseId = 1,
            Order = 1,
            PlannedSets = 3,
            PlannedReps = reps,
            PlannedWeight = 0,
            PlannedRestTimeSeconds = 90
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        
        if (shouldBeValid)
        {
            validationResults.Should().NotContain(vr => vr.MemberNames.Contains("PlannedReps"));
        }
        else
        {
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("PlannedReps"));
        }
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(500, true)]
    [InlineData(1000, true)]
    [InlineData(1001, false)]
    public void ProgramExerciseDto_PlannedWeight_ValidatesRange(decimal weight, bool shouldBeValid)
    {
        // Arrange
        var dto = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1,
            ExerciseId = 1,
            Order = 1,
            PlannedSets = 3,
            PlannedReps = 10,
            PlannedWeight = weight,
            PlannedRestTimeSeconds = 90
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        
        if (shouldBeValid)
        {
            validationResults.Should().NotContain(vr => vr.MemberNames.Contains("PlannedWeight"));
        }
        else
        {
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("PlannedWeight"));
        }
    }

    [Fact]
    public void ProgramExerciseDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new AddExerciseModel.ProgramExerciseDto
        {
            ProgramId = 1,
            ExerciseId = 1,
            Order = 1,
            PlannedSets = 3,
            PlannedReps = 10,
            PlannedWeight = 135m,
            PlannedRestTimeSeconds = 90
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        validationResults.Should().BeEmpty();
    }

    private static IList<System.ComponentModel.DataAnnotations.ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var ctx = new System.ComponentModel.DataAnnotations.ValidationContext(model, null, null);
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}