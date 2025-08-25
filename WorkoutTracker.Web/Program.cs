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
builder.Services.AddScoped<IProgramRepository, ProgramRepository>();
builder.Services.AddScoped<IProgramExerciseRepository, ProgramExerciseRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
builder.Services.AddScoped<ISetRepository, SetRepository>();

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

    // Replace the existing repository test section with this expanded version:

    // Test all repositories!
    var exerciseRepo = scope.ServiceProvider.GetRequiredService<IExerciseRepository>();
    var programRepo = scope.ServiceProvider.GetRequiredService<IProgramRepository>();
    var programExerciseRepo = scope.ServiceProvider.GetRequiredService<IProgramExerciseRepository>();
    var workoutRepo = scope.ServiceProvider.GetRequiredService<IWorkoutRepository>();
    var workoutExerciseRepo = scope.ServiceProvider.GetRequiredService<IWorkoutExerciseRepository>();
    var setRepo = scope.ServiceProvider.GetRequiredService<ISetRepository>();

    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("TESTING ALL REPOSITORIES");
    Console.WriteLine(new string('=', 60));

    // Test ExerciseRepository
    Console.WriteLine("\n--- ExerciseRepository ---");
    var allExercises = await exerciseRepo.GetAllAsync();
    Console.WriteLine($"Total exercises: {allExercises.Count()}");
    var benchPress = await exerciseRepo.GetByNameAsync("Bench Press");
    Console.WriteLine($"Bench Press found: {benchPress?.Name ?? "Not found"}");

    // Test ProgramRepository  
    Console.WriteLine("\n--- ProgramRepository ---");
    var allPrograms = await programRepo.GetAllAsync();
    Console.WriteLine($"Total programs: {allPrograms.Count()}");
    var strengthProgram = await programRepo.GetByNameAsync("Beginner Strength");
    Console.WriteLine($"Strength program found: {strengthProgram?.Name ?? "Not found"}");

    // Test ProgramExerciseRepository
    Console.WriteLine("\n--- ProgramExerciseRepository ---");
    if (strengthProgram != null)
    {
        var programExercises = await programExerciseRepo.GetByProgramIdAsync(strengthProgram.Id);
        Console.WriteLine($"Exercises in '{strengthProgram.Name}': {programExercises.Count()}");
        foreach (var pe in programExercises)
        {
            Console.WriteLine($"  {pe.Order}. {pe.Exercise.Name} - {pe.PlannedSets}x{pe.PlannedReps} @ {pe.PlannedWeight}lbs");
        }

        // Test the compound lookup
        if (benchPress != null)
        {
            var specificPE = await programExerciseRepo.GetByProgramAndExerciseIdAsync(strengthProgram.Id, benchPress.Id);
            Console.WriteLine($"Bench Press in program: {specificPE?.PlannedSets}x{specificPE?.PlannedReps} @ {specificPE?.PlannedWeight}lbs");
        }
    }

    // Test creating a workout to test WorkoutExercise and Set repos
    Console.WriteLine("\n--- Creating Test Workout ---");
    var testWorkout = new Workout
    {
        ProgramId = strengthProgram?.Id,
        Date = DateTime.Today,
        IsModifiedFromProgram = false
    };
    await workoutRepo.AddAsync(testWorkout);
    Console.WriteLine($"Created workout on {testWorkout.Date:yyyy-MM-dd}");

    // Test WorkoutRepository
    Console.WriteLine("\n--- WorkoutRepository ---");
    var todaysWorkouts = await workoutRepo.GetByDateRangeAsync(DateTime.Today, DateTime.Today);
    Console.WriteLine($"Today's workouts: {todaysWorkouts.Count()}");

    if (strengthProgram != null)
    {
        var programWorkouts = await workoutRepo.GetByProgramIdAsync(strengthProgram.Id);
        Console.WriteLine($"Workouts using '{strengthProgram.Name}': {programWorkouts.Count()}");
    }

    // Test WorkoutExerciseRepository & SetRepository
    Console.WriteLine("\n--- WorkoutExercise & Set Testing ---");
    if (benchPress != null)
    {
        // Add bench press to the workout
        var workoutExercise = new WorkoutExercise
        {
            WorkoutId = testWorkout.Id,
            ExerciseId = benchPress.Id,
            Order = 1
        };
        await workoutExerciseRepo.AddAsync(workoutExercise);

        // Add some sets
        var sets = new[]
        {
            new Set { WorkoutExerciseId = workoutExercise.Id, Order = 1, Reps = 10, Weight = 135m, RPE = 7 },
            new Set { WorkoutExerciseId = workoutExercise.Id, Order = 2, Reps = 8, Weight = 145m, RPE = 8 },
            new Set { WorkoutExerciseId = workoutExercise.Id, Order = 3, Reps = 6, Weight = 155m, RPE = 9 }
        };
        foreach (var set in sets)
        {
            await setRepo.AddAsync(set);
        }

        // Test queries
        var workoutExercises = await workoutExerciseRepo.GetByWorkoutIdAsync(testWorkout.Id);
        Console.WriteLine($"Exercises in workout: {workoutExercises.Count()}");
        var exerciseSets = await setRepo.GetByWorkoutExerciseIdAsync(workoutExercise.Id);
        Console.WriteLine($"Sets performed:");
        foreach (var set in exerciseSets)
        {
            Console.WriteLine($"  Set {set.Order}: {set.Reps} reps @ {set.Weight}lbs (RPE {set.RPE})");
        }

        // Test compound lookup
        var specificWE = await workoutExerciseRepo.GetByWorkoutAndExerciseIdAsync(testWorkout.Id, benchPress.Id);
        Console.WriteLine($"Found specific workout exercise with {specificWE?.Sets.Count ?? 0} sets");
    }

    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("ALL REPOSITORY TESTS COMPLETE!");
    Console.WriteLine(new string('=', 60) + "\n");
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
