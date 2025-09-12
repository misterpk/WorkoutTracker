using FluentAssertions;
using Moq;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Web.Pages.Exercises;

namespace WorkoutTracker.Web.UnitTests;

public class ExerciseIndexModelUnitTests
{
    private readonly Mock<IExerciseRepository> _mockExerciseRepository;
    private readonly IndexModel _indexModel;

    public ExerciseIndexModelUnitTests()
    {
        _mockExerciseRepository = new Mock<IExerciseRepository>();
        _indexModel = new IndexModel(_mockExerciseRepository.Object);
    }

    [Fact]
    public async Task OnGetAsync_WithNoFilters_LoadsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" },
            new Exercise { Name = "Pull-ups", PrimaryMuscle = "Back", Description = "Upper body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: null, filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(3);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Push-ups");
        _indexModel.Exercises.Should().Contain(e => e.Name == "Squats");
        _indexModel.Exercises.Should().Contain(e => e.Name == "Pull-ups");
        _indexModel.SearchTerm.Should().BeNull();
        _indexModel.FilterMuscle.Should().BeNull();
    }

    [Fact]
    public async Task OnGetAsync_WithSearchTerm_FiltersExercisesByName()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Pull-ups", PrimaryMuscle = "Back", Description = "Upper body exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "ups", filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(2);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Push-ups");
        _indexModel.Exercises.Should().Contain(e => e.Name == "Pull-ups");
        _indexModel.Exercises.Should().NotContain(e => e.Name == "Squats");
        _indexModel.SearchTerm.Should().Be("ups");
    }

    [Fact]
    public async Task OnGetAsync_WithSearchTerm_FiltersExercisesByDescription()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Dumbbell Press", PrimaryMuscle = "Chest", Description = "Upper body exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "upper", filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(1);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Dumbbell Press");
        _indexModel.SearchTerm.Should().Be("upper");
    }

    [Fact]
    public async Task OnGetAsync_WithMuscleFilter_FiltersExercisesByMuscleGroup()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Bench Press", PrimaryMuscle = "Chest", Description = "Weight exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: null, filterMuscle: "Chest");

        // Assert
        _indexModel.Exercises.Should().HaveCount(2);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Push-ups");
        _indexModel.Exercises.Should().Contain(e => e.Name == "Bench Press");
        _indexModel.Exercises.Should().NotContain(e => e.Name == "Squats");
        _indexModel.FilterMuscle.Should().Be("Chest");
    }

    [Fact]
    public async Task OnGetAsync_WithBothFilters_AppliesBothSearchAndMuscleFilter()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Chest Press", PrimaryMuscle = "Chest", Description = "Machine exercise" },
            new Exercise { Name = "Chest Fly", PrimaryMuscle = "Chest", Description = "Weight exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Compound exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "press", filterMuscle: "Chest");

        // Assert
        _indexModel.Exercises.Should().HaveCount(1);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Chest Press");
        _indexModel.SearchTerm.Should().Be("press");
        _indexModel.FilterMuscle.Should().Be("Chest");
    }

    [Fact]
    public async Task OnGetAsync_WithCaseInsensitiveSearch_FiltersCorrectly()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "PULL-UPS", PrimaryMuscle = "Back", Description = "UPPER BODY EXERCISE" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "UPS", filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(2);
        _indexModel.Exercises.Should().Contain(e => e.Name == "Push-ups");
        _indexModel.Exercises.Should().Contain(e => e.Name == "PULL-UPS");
    }

    [Fact]
    public async Task OnGetAsync_WithEmptySearchTerm_LoadsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "", filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnGetAsync_WithWhitespaceSearchTerm_LoadsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Bodyweight exercise" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Lower body exercise" }
        };

        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(exercises);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "   ", filterMuscle: null);

        // Assert
        _indexModel.Exercises.Should().HaveCount(2);
    }

    [Fact]
    public void GetUniqueMuscleGroups_WithVariousExercises_ReturnsDistinctMuscleGroups()
    {
        // Arrange
        _indexModel.Exercises = new List<Exercise>
        {
            new Exercise { Name = "Push-ups", PrimaryMuscle = "Chest", Description = "Exercise 1" },
            new Exercise { Name = "Bench Press", PrimaryMuscle = "Chest", Description = "Exercise 2" },
            new Exercise { Name = "Squats", PrimaryMuscle = "Quadriceps", Description = "Exercise 3" },
            new Exercise { Name = "Pull-ups", PrimaryMuscle = "Back", Description = "Exercise 4" },
            new Exercise { Name = "Rows", PrimaryMuscle = "Back", Description = "Exercise 5" }
        };

        // Act
        var uniqueMuscleGroups = _indexModel.GetUniqueMuscleGroups();

        // Assert
        uniqueMuscleGroups.Should().HaveCount(3);
        uniqueMuscleGroups.Should().Contain("Back");
        uniqueMuscleGroups.Should().Contain("Chest");
        uniqueMuscleGroups.Should().Contain("Quadriceps");
        
        // Should be ordered alphabetically
        uniqueMuscleGroups.Should().BeInAscendingOrder();
    }

    [Fact]
    public void GetUniqueMuscleGroups_WithNoExercises_ReturnsEmpty()
    {
        // Arrange
        _indexModel.Exercises = new List<Exercise>();

        // Act
        var uniqueMuscleGroups = _indexModel.GetUniqueMuscleGroups();

        // Assert
        uniqueMuscleGroups.Should().BeEmpty();
    }

    [Fact]
    public async Task OnGetAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _mockExerciseRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _indexModel.OnGetAsync(searchTerm: null, filterMuscle: null));
        
        exception.Message.Should().Be("Database error");
    }
}