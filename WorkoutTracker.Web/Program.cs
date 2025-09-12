using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Core.Models;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// Configure Entity Framework (skip for Testing environment - handled by test factory)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<WorkoutTrackerDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);

        // Enable sensitive data logging and detailed errors in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            // Log SQL queries to console for learning
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }
    });
}

// Register repositories (always needed, regardless of environment)
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IProgramRepository, ProgramRepository>();
builder.Services.AddScoped<IProgramExerciseRepository, ProgramExerciseRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
builder.Services.AddScoped<ISetRepository, SetRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
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

// Ensure database is created in development (until we set up proper migrations)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();

    // This will create the database if it doesn't exist
    // Later we'll replace this with proper migrations
    context.Database.EnsureCreated();
}

app.Run();
