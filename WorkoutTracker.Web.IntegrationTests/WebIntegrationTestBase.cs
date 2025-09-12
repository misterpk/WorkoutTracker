using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Data;
using AngleSharp;
using AngleSharp.Html.Dom;
using Microsoft.Data.Sqlite;

namespace WorkoutTracker.Web.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        // Create SQLite in-memory connection that stays open for the lifetime of the factory
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL Entity Framework and database provider services
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<WorkoutTrackerDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(WorkoutTrackerDbContext) ||
                d.ServiceType.Name.Contains("EntityFramework") ||
                d.ServiceType.Name.Contains("SqlServer") ||
                d.ImplementationType?.Name.Contains("EntityFramework") == true ||
                d.ImplementationType?.Name.Contains("SqlServer") == true
            ).ToArray();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add SQLite in-memory database for testing - shared connection ensures data persistence
            services.AddDbContext<WorkoutTrackerDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging();
            });

            // Build service provider to ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
            context.Database.EnsureCreated();
        });

        // Use test environment to prevent Program.cs database setup
        builder.UseEnvironment("Testing");

        // Reduce logging noise during tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class WebIntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly AngleSharp.IConfiguration AngleSharpConfig;

    public WebIntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // Ensure we can test redirect responses
        });
        AngleSharpConfig = Configuration.Default;
    }

    protected async Task<IHtmlDocument> GetDocumentAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        return (IHtmlDocument)await context.OpenAsync(req => req.Content(content));
    }

    protected async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData)
    {
        // Get the form page first to extract anti-forgery token
        var getResponse = await Client.GetAsync(url);
        getResponse.EnsureSuccessStatusCode();
        
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = (IHtmlDocument)await context.OpenAsync(req => req.Content(getContent));
        
        // Extract anti-forgery token
        var tokenInput = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement;
        if (tokenInput != null)
        {
            formData["__RequestVerificationToken"] = tokenInput.Value ?? "";
        }
        
        var formContent = new FormUrlEncodedContent(formData);
        return await Client.PostAsync(url, formContent);
    }

    protected WorkoutTrackerDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
    }

    protected async Task ClearDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
        
        // Clear all exercises to ensure test isolation
        var exercises = context.Exercises.ToList();
        context.Exercises.RemoveRange(exercises);
        await context.SaveChangesAsync();
        
        // Force the context to detach all entities to ensure fresh reads
        context.ChangeTracker.Clear();
    }

    protected async Task SeedDatabaseAsync(Action<WorkoutTrackerDbContext> seedAction)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
        seedAction(context);
        await context.SaveChangesAsync();
        
        // Force the context to detach all entities to ensure fresh reads
        context.ChangeTracker.Clear();
    }

    protected async Task<T> SeedEntityAsync<T>(T entity) where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
        var entry = context.Set<T>().Add(entity);
        await context.SaveChangesAsync();
        
        // Force the context to detach all entities to ensure fresh reads
        context.ChangeTracker.Clear();
        return entry.Entity;
    }

    protected async Task<int> GetExerciseCountAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutTrackerDbContext>();
        return await context.Exercises.CountAsync();
    }

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}