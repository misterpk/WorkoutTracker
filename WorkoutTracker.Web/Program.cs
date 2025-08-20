using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Entity Framework
builder.Services.AddDbContext<WorkoutTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();

var app = builder.Build();

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
    
    // Check if we already have data
    if (!context.Exercises.Any())
    {
        // Create some exercises
        var squats = new Exercise
        {
            Name = "Back Squat",
            Description = "Barbell back squat with proper depth",
            PrimaryMuscle = "Quadriceps"
        };

        var benchPressAdd = new Exercise
        {
            Name = "Bench Press",
            Description = "Barbell bench press with controlled tempo",
            PrimaryMuscle = "Chest"
        };

        var deadlift = new Exercise
        {
            Name = "Deadlift",
            Description = "Conventional deadlift from floor",
            PrimaryMuscle = "Hamstrings"
        };

        context.Exercises.AddRange(squats, benchPressAdd, deadlift);
        context.SaveChanges();

        Console.WriteLine("Test exercises added!");

        // Create a program
        var strengthProgramTemplate = new WorkoutTracker.Core.Models.Program
        {
            Name = "Beginner Strength",
            Description = "Basic 3x5 strength program"
        };

        context.Programs.Add(strengthProgramTemplate);
        context.SaveChanges();

        // Add exercises to the program
        var programExercises = new[]
        {
            new ProgramExercise
            {
                ProgramId = strengthProgramTemplate.Id,
                ExerciseId = squats.Id,
                Order = 1,
                PlannedSets = 3,
                PlannedReps = 5,
                PlannedWeight = 135m
            },
            new ProgramExercise
            {
                ProgramId = strengthProgramTemplate.Id,
                ExerciseId = benchPressAdd.Id,
                Order = 2,
                PlannedSets = 3,
                PlannedReps = 5,
                PlannedWeight = 115m
            },
            new ProgramExercise
            {
                ProgramId = strengthProgramTemplate.Id,
                ExerciseId = deadlift.Id,
                Order = 3,
                PlannedSets = 1,
                PlannedReps = 5,
                PlannedWeight = 185m
            }
        };

        context.ProgramExercises.AddRange(programExercises);
        context.SaveChanges();

        Console.WriteLine("Test program created with exercises!");
    }
    else
    {
        Console.WriteLine("Database already has data, skipping seeding.");
    }
    
    // MOVE THE REPOSITORY TEST OUT HERE - always runs regardless of seeding
    var exerciseRepo = scope.ServiceProvider.GetRequiredService<IExerciseRepository>();
    
    Console.WriteLine("\n" + new string('=', 50));
    Console.WriteLine("TESTING EXERCISE REPOSITORY");
    Console.WriteLine(new string('=', 50));
    
    // Test GetAllAsync
    var allExercises = await exerciseRepo.GetAllAsync();
    Console.WriteLine($"Found {allExercises.Count()} exercises:");
    foreach (var ex in allExercises)
    {
        Console.WriteLine($"  - {ex.Name} ({ex.PrimaryMuscle}) - Created: {ex.CreatedAt}");
    }
    
    // Test GetByNameAsync
    var benchPress = await exerciseRepo.GetByNameAsync("Bench Press");
    Console.WriteLine($"\nBench Press lookup: {benchPress?.Name ?? "Not found"}");
    
    // Test GetByPrimaryMuscleAsync
    var chestExercises = await exerciseRepo.GetByPrimaryMuscleAsync("Chest");
    Console.WriteLine($"\nChest exercises: {chestExercises.Count()}");
    
    // Test ExistsAsync
    var exists = await exerciseRepo.ExistsAsync(1);
    Console.WriteLine($"\nExercise ID 1 exists: {exists}");
    
    Console.WriteLine(new string('=', 50));
    Console.WriteLine("REPOSITORY TEST COMPLETE");
    Console.WriteLine(new string('=', 50) + "\n");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
