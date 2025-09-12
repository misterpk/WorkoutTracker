using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Web.Pages.Programs;
using ProgramModel = WorkoutTracker.Core.Models.Program;

namespace WorkoutTracker.Web.UnitTests;

public class ProgramCreateModelUnitTests
{
    private readonly Mock<IProgramRepository> _mockProgramRepository;
    private readonly CreateModel _createModel;

    public ProgramCreateModelUnitTests()
    {
        _mockProgramRepository = new Mock<IProgramRepository>();
        _createModel = new CreateModel(_mockProgramRepository.Object);
    }

    [Fact]
    public async Task OnGetAsync_WithNoCopyFromId_InitializesEmptyForm()
    {
        // Act
        await _createModel.OnGetAsync();

        // Assert
        _createModel.Program.Name.Should().BeEmpty();
        _createModel.Program.Description.Should().BeNull();
        _createModel.CopyFromProgram.Should().BeNull();
    }

    [Fact]
    public async Task OnGetAsync_WithValidCopyFromId_LoadsSourceProgram()
    {
        // Arrange
        var sourceProgram = new ProgramModel
        {
            Id = 1,
            Name = "Original Program",
            Description = "Original description"
        };
        
        _createModel.CopyFromId = 1;
        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(sourceProgram);

        // Act
        await _createModel.OnGetAsync();

        // Assert
        _createModel.CopyFromProgram.Should().NotBeNull();
        _createModel.CopyFromProgram.Name.Should().Be("Original Program");
        _createModel.Program.Name.Should().Be("Copy of Original Program");
        _createModel.Program.Description.Should().Be("Original description");
    }

    [Fact]
    public async Task OnGetAsync_WithNonExistentCopyFromId_DoesNotSetCopyFromProgram()
    {
        // Arrange
        _createModel.CopyFromId = 999;
        _mockProgramRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ProgramModel?)null);

        // Act
        await _createModel.OnGetAsync();

        // Assert
        _createModel.CopyFromProgram.Should().BeNull();
        _createModel.Program.Name.Should().BeEmpty();
        _createModel.Program.Description.Should().BeNull();
    }

    [Fact]
    public async Task OnPostAsync_WithValidProgram_CreatesProgram()
    {
        // Arrange
        var createdProgram = new ProgramModel
        {
            Id = 1,
            Name = "Test Program",
            Description = "Test description"
        };

        _createModel.Program = new CreateModel.ProgramDto
        {
            Name = "Test Program",
            Description = "Test description"
        };

        _mockProgramRepository.Setup(r => r.AddAsync(It.IsAny<ProgramModel>()))
            .ReturnsAsync(createdProgram);

        // Act
        var result = await _createModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("Details");
        redirectResult.RouteValues.Should().ContainKey("id");
        redirectResult.RouteValues["id"].Should().Be(1);

        _mockProgramRepository.Verify(r => r.AddAsync(It.Is<ProgramModel>(p => 
            p.Name == "Test Program" && 
            p.Description == "Test description")), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithCopyFromId_UsesCopyAsyncMethod()
    {
        // Arrange
        var createdProgram = new ProgramModel
        {
            Id = 2,
            Name = "Copied Program",
            Description = "Original description"
        };

        _createModel.Program = new CreateModel.ProgramDto
        {
            Name = "Copied Program",
            Description = "Original description"
        };
        _createModel.CopyFromId = 1;

        _mockProgramRepository.Setup(r => r.AddAsync(It.IsAny<ProgramModel>()))
            .ReturnsAsync(createdProgram);
        
        _mockProgramRepository.Setup(r => r.CopyAsync(1, "Copied Program"))
            .ReturnsAsync(createdProgram);

        // Act
        var result = await _createModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<RedirectToPageResult>();
        _mockProgramRepository.Verify(r => r.CopyAsync(1, "Copied Program"), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelState_ReturnsPage()
    {
        // Arrange
        _createModel.ModelState.AddModelError("Program.Name", "Name is required");

        // Act
        var result = await _createModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _mockProgramRepository.Verify(r => r.AddAsync(It.IsAny<ProgramModel>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidModelStateAndCopyFromId_ReloadsSourceProgram()
    {
        // Arrange
        var sourceProgram = new ProgramModel
        {
            Id = 1,
            Name = "Source Program",
            Description = "Source description"
        };

        _createModel.CopyFromId = 1;
        _createModel.ModelState.AddModelError("Program.Name", "Name is required");
        
        _mockProgramRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(sourceProgram);

        // Act
        var result = await _createModel.OnPostAsync();

        // Assert
        result.Should().BeOfType<PageResult>();
        _createModel.CopyFromProgram.Should().NotBeNull();
        _createModel.CopyFromProgram.Name.Should().Be("Source Program");
    }

    [Fact]
    public async Task OnPostAsync_WithSuccessfulCreation_SetsTempDataMessage()
    {
        // Arrange
        var createdProgram = new ProgramModel
        {
            Id = 1,
            Name = "Test Program",
            Description = "Test description"
        };

        _createModel.Program = new CreateModel.ProgramDto
        {
            Name = "Test Program",
            Description = "Test description"
        };

        _mockProgramRepository.Setup(r => r.AddAsync(It.IsAny<ProgramModel>()))
            .ReturnsAsync(createdProgram);

        // Act
        await _createModel.OnPostAsync();

        // Assert
        _createModel.TempData["SuccessMessage"].Should().Be("Program 'Test Program' has been created successfully!");
    }

    [Fact]
    public async Task OnPostAsync_WithSuccessfulCopy_SetsCopyTempDataMessage()
    {
        // Arrange
        var createdProgram = new ProgramModel
        {
            Id = 2,
            Name = "Copied Program",
            Description = "Original description"
        };

        _createModel.Program = new CreateModel.ProgramDto
        {
            Name = "Copied Program",
            Description = "Original description"
        };
        _createModel.CopyFromId = 1;

        _mockProgramRepository.Setup(r => r.AddAsync(It.IsAny<ProgramModel>()))
            .ReturnsAsync(createdProgram);
        
        _mockProgramRepository.Setup(r => r.CopyAsync(1, "Copied Program"))
            .ReturnsAsync(createdProgram);

        // Act
        await _createModel.OnPostAsync();

        // Assert
        _createModel.TempData["SuccessMessage"].Should().Be("Program 'Copied Program' has been created successfully with all exercises copied!");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ProgramDto_WithInvalidName_FailsValidation(string name)
    {
        // Arrange
        var dto = new CreateModel.ProgramDto
        {
            Name = name,
            Description = "Valid description"
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
    }

    [Fact]
    public void ProgramDto_WithNameTooLong_FailsValidation()
    {
        // Arrange
        var dto = new CreateModel.ProgramDto
        {
            Name = new string('A', 201), // Exceeds 200 character limit
            Description = "Valid description"
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Name"));
    }

    [Fact]
    public void ProgramDto_WithDescriptionTooLong_FailsValidation()
    {
        // Arrange
        var dto = new CreateModel.ProgramDto
        {
            Name = "Valid Name",
            Description = new string('A', 1001) // Exceeds 1000 character limit
        };

        // Act & Assert
        var validationResults = ValidateModel(dto);
        validationResults.Should().Contain(vr => vr.MemberNames.Contains("Description"));
    }

    [Fact]
    public void ProgramDto_WithValidData_PassesValidation()
    {
        // Arrange
        var dto = new CreateModel.ProgramDto
        {
            Name = "Valid Program Name",
            Description = "Valid description"
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