using FluentAssertions;
using Moq;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Web.Pages.Programs;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.UnitTests;

public class ProgramIndexModelUnitTests
{
    private readonly Mock<IProgramRepository> _mockProgramRepository;
    private readonly IndexModel _indexModel;

    public ProgramIndexModelUnitTests()
    {
        _mockProgramRepository = new Mock<IProgramRepository>();
        _indexModel = new IndexModel(_mockProgramRepository.Object);
    }

    [Fact]
    public async Task OnGetAsync_WithNoFilters_LoadsAllPrograms()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Push Day", Description = "Upper body push exercises" },
            new ProgramModel { Name = "Pull Day", Description = "Upper body pull exercises" },
            new ProgramModel { Name = "Leg Day", Description = "Lower body exercises" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: null);

        // Assert
        _indexModel.Programs.Should().HaveCount(3);
        _indexModel.Programs.Should().Contain(p => p.Name == "Push Day");
        _indexModel.Programs.Should().Contain(p => p.Name == "Pull Day");
        _indexModel.Programs.Should().Contain(p => p.Name == "Leg Day");
        _indexModel.SearchTerm.Should().BeNull();
    }

    [Fact]
    public async Task OnGetAsync_WithSearchTerm_FiltersPrograms()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Push Day", Description = "Upper body push exercises" },
            new ProgramModel { Name = "Pull Day", Description = "Upper body pull exercises" },
            new ProgramModel { Name = "Leg Day", Description = "Lower body exercises" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "upper");

        // Assert
        _indexModel.Programs.Should().HaveCount(2);
        _indexModel.Programs.Should().Contain(p => p.Name == "Push Day");
        _indexModel.Programs.Should().Contain(p => p.Name == "Pull Day");
        _indexModel.Programs.Should().NotContain(p => p.Name == "Leg Day");
        _indexModel.SearchTerm.Should().Be("upper");
    }

    [Fact]
    public async Task OnGetAsync_WithSearchTerm_FiltersProgromsByName()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Push Day", Description = "Upper body exercises" },
            new ProgramModel { Name = "Pull Day", Description = "Back and biceps" },
            new ProgramModel { Name = "Push/Pull Split", Description = "Combined routine" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "push");

        // Assert
        _indexModel.Programs.Should().HaveCount(2);
        _indexModel.Programs.Should().Contain(p => p.Name == "Push Day");
        _indexModel.Programs.Should().Contain(p => p.Name == "Push/Pull Split");
        _indexModel.Programs.Should().NotContain(p => p.Name == "Pull Day");
    }

    [Fact]
    public async Task OnGetAsync_WithCaseInsensitiveSearch_FiltersCorrectly()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "STRENGTH TRAINING", Description = "Heavy lifting program" },
            new ProgramModel { Name = "strength building", Description = "Muscle development" },
            new ProgramModel { Name = "Cardio Day", Description = "Cardiovascular training" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "STRENGTH");

        // Assert
        _indexModel.Programs.Should().HaveCount(2);
        _indexModel.Programs.Should().Contain(p => p.Name == "STRENGTH TRAINING");
        _indexModel.Programs.Should().Contain(p => p.Name == "strength building");
    }

    [Fact]
    public async Task OnGetAsync_WithEmptySearchTerm_LoadsAllPrograms()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Program A", Description = "Description A" },
            new ProgramModel { Name = "Program B", Description = "Description B" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "");

        // Assert
        _indexModel.Programs.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnGetAsync_WithWhitespaceSearchTerm_LoadsAllPrograms()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Program A", Description = "Description A" },
            new ProgramModel { Name = "Program B", Description = "Description B" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: "   ");

        // Assert
        _indexModel.Programs.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnGetAsync_SortsPrograms_AlphabeticallyByName()
    {
        // Arrange
        var programs = new List<ProgramModel>
        {
            new ProgramModel { Name = "Zebra Program", Description = "Last alphabetically" },
            new ProgramModel { Name = "Alpha Program", Description = "First alphabetically" },
            new ProgramModel { Name = "Beta Program", Description = "Second alphabetically" }
        };

        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(programs);

        // Act
        await _indexModel.OnGetAsync(searchTerm: null);

        // Assert
        var programNames = _indexModel.Programs.Select(p => p.Name).ToList();
        programNames.Should().BeInAscendingOrder();
        programNames[0].Should().Be("Alpha Program");
        programNames[1].Should().Be("Beta Program");
        programNames[2].Should().Be("Zebra Program");
    }

    [Fact]
    public void CalculateEstimatedDuration_WithNoProgramExercises_ReturnsZero()
    {
        // Arrange
        var program = new ProgramModel 
        { 
            Name = "Empty Program",
            ProgramExercises = new List<ProgramExercise>()
        };

        // Act
        var duration = _indexModel.CalculateEstimatedDuration(program);

        // Assert
        duration.Should().Be(0);
    }

    [Fact]
    public void CalculateEstimatedDuration_WithProgramExercises_CalculatesCorrectly()
    {
        // Arrange
        var program = new ProgramModel 
        { 
            Name = "Test Program",
            ProgramExercises = new List<ProgramExercise>
            {
                new ProgramExercise 
                { 
                    PlannedSets = 3, 
                    PlannedRestTimeSeconds = 90 
                },
                new ProgramExercise 
                { 
                    PlannedSets = 4, 
                    PlannedRestTimeSeconds = 120 
                }
            }
        };

        // Act
        var duration = _indexModel.CalculateEstimatedDuration(program);

        // Assert
        // Expected calculation:
        // Total sets: 3 + 4 = 7
        // Work time: 7 * 45 seconds = 315 seconds
        // Rest periods: 7 - 2 = 5 rest periods
        // Average rest time: (90 + 120) / 2 = 105 seconds
        // Total rest time: 5 * 105 = 525 seconds
        // Warm-up/cool-down: 5 * 60 = 300 seconds
        // Total: (315 + 525 + 300) / 60 = 19 minutes
        duration.Should().Be(19);
    }

    [Fact]
    public void CalculateEstimatedDuration_WithZeroRestTime_UsesDefaultRest()
    {
        // Arrange
        var program = new ProgramModel 
        { 
            Name = "Test Program",
            ProgramExercises = new List<ProgramExercise>
            {
                new ProgramExercise 
                { 
                    PlannedSets = 3, 
                    PlannedRestTimeSeconds = 0 
                },
                new ProgramExercise 
                { 
                    PlannedSets = 2, 
                    PlannedRestTimeSeconds = 0 
                }
            }
        };

        // Act
        var duration = _indexModel.CalculateEstimatedDuration(program);

        // Assert
        // Expected calculation with default 90 second rest:
        // Total sets: 3 + 2 = 5
        // Work time: 5 * 45 = 225 seconds
        // Rest periods: 5 - 2 = 3 rest periods
        // Total rest time: 3 * 90 = 270 seconds (using default)
        // Warm-up/cool-down: 300 seconds
        // Total: (225 + 270 + 300) / 60 = 13.25 minutes ≈ 13 minutes
        duration.Should().Be(13);
    }

    [Fact]
    public async Task OnGetAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _mockProgramRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _indexModel.OnGetAsync(searchTerm: null));
        
        exception.Message.Should().Be("Database error");
    }
}