# CLAUDE.md - Workout Tracker Project Context

## Project Overview

Building a workout tracking web application as a learning project. Primary goal is understanding web development fundamentals and learning the .NET ecosystem for work applicability.

## User Background

- **Experience:** 10 years full-stack software engineer
- **Strong in:** Ruby, Ruby on Rails, JavaScript (intermediate to advanced)
- **Learning:** .NET Core, C#, Entity Framework, SQL Server (beginner to intermediate)
- **Familiar with:** Web development concepts, MVC patterns, REST APIs
- **Work context:** Team uses .NET Core 8 backend, migrating to modern .NET stack

## Learning Preferences

- **Methodology:** Build fundamentals first, understand problems frameworks solve
- **Approach:** Build with one technology, then rebuild with another to compare
- **Code guidance:** Guide toward solutions rather than providing complete code
- **Focus:** Understanding real-world problems and enterprise patterns

## Technology Stack (Current)

- **Backend:** .NET 8 (ASP.NET Core Web API + Razor Pages)
- **Frontend Phase 1:** Razor Pages (server-side rendering)
- **Frontend Phase 2:** Blazor (rebuild same functionality)
- **Database:** SQL Server in Docker (with EF Core Code First)
- **ORM:** Entity Framework Core with Repository pattern
- **Architecture:** Clean Architecture (layered approach)
- **Configuration:** Fluent API with separate entity configuration classes

## Development Environment

- **OS:** macOS with Docker via Colima
- **IDE:** VS Code with C# Dev Kit extension
- **Database:** SQL Server in Docker container
- **Container name:** `sqlserver`
- **Connection string:** `Server=localhost,1433;Database=WorkoutTrackerDb;User Id=SA;Password=YourStrong@Passw0rd;TrustServerCertificate=true;`
- **.NET Version:** 8.0.401
- **Database tools:** sqlcmd CLI + SQL Server extension for VS Code
- **Project location:** `~/dev/dotnet/WorkoutTracker`

**Dependencies:**

- Web → Infrastructure (repositories, data access)
- Infrastructure → Core (domain models, interfaces)
- Core → (no dependencies - pure domain logic)
- Tests → Respective projects they're testing

## Application Requirements

### MVP Scope

- **Focus:** Resistance training with program templates AND freestyle workouts
- **Core entities:**
  - **Exercise** (name, description, primary_muscle)
  - **Program** (reusable workout template)
  - **ProgramExercise** (planned exercise with order, sets/reps/weight/rest)
  - **Workout** (actual session, optionally references program, can be modified)
  - **WorkoutExercise** (exercise in actual workout)
  - **Set** (actual reps/weight/RPE performed)
- **User flow:** Create programs → Build workouts from programs OR create freestyle workouts → Log actual performance
- **Key features:** Program templates, freestyle workouts, planned vs actual tracking, RPE monitoring

## Code Style Preferences

- **Explicit types** - Use strong typing, avoid `var` when learning
- **Entity Framework logging** - Always show generated SQL for learning
- **Repository interfaces** - Abstract data access like at work
- **Layered architecture** - Web → Infrastructure → Core dependencies
- **Nullable reference types** - Enabled for better null safety
- **ICollection<T>** - Preferred over List<T> for navigation properties
- **Fluent API over Data Annotations** - More explicit, better for learning
- **Separate configuration classes** - Better organization than OnModelCreating
- **Git Commit Messages** - Use Conventional Commits. Rules located in ./cc.md

## Docker & Database Context

```bash
# SQL Server container management
docker ps                           # Check if sqlserver container running
docker start sqlserver             # Start if stopped
docker exec -it sqlserver sqlcmd   # Connect to SQL Server

```

## Learning Focus Areas

1. **Clean Architecture** ✅ (completed project structure and references)
2. **Domain Modeling** ✅ (completed all 6 entities with relationships)
3. **Repository Pattern** ✅ (defined all interfaces with async patterns)
4. **BaseModel Pattern** ✅ (automatic audit fields)
5. **Entity Framework Core** ✅ (DbContext, Fluent API, separate configurations)
6. **Fluent API Configuration** ✅ (all entities configured with relationships and constraints)
7. **Navigation Properties** ✅ (bidirectional relationships with proper types)
8. **Code First Migrations** 📄 (next - generate SQL schema from models)
9. **Dependency Injection** 📄 (next - configure DbContext in Program.cs)
10. **Repository Implementation** 📋 (implement pattern with EF Core)
11. **SQL understanding** 📋 (through EF Core query logging)
12. **Automated Testing** ✅ (unit, integration, end-to-end tests - COMPLETED with key learnings)
13. **Hybrid Controllers** 📋 (serving both web and API responses)
14. **Razor Pages** 📋 (server-side rendering)
15. **Blazor** 📋 (future phase 5)
16. **.NET MAUI** 📋 (future phase 6 - cross-platform mobile)
17. **iOS Native** 📋 (future phase 7 - comparison study)

## Quick Reference

```bash
# Project setup
cd ~/dev/dotnet/WorkoutTracker
dotnet build                    # Build entire solution (3 projects)
dotnet watch run --project WorkoutTracker.Web # Run web application in watch mode
dotnet test                     # Run all automated tests

# Project structure
dotnet sln list                 # Shows Core, Infrastructure, Web

# EF Core commands (ready to use)
dotnet ef migrations add InitialCreate --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web
dotnet ef database update --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web

# Testing commands (validated and working)
dotnet test WorkoutTracker.Web.UnitTests/ --verbosity minimal        # Unit tests only (11 tests)
dotnet test WorkoutTracker.Web.IntegrationTests/ --verbosity minimal # Integration tests only (11 tests)
dotnet test WorkoutTracker.Infrastructure.IntegrationTests/ --verbosity minimal # Repository tests (87 tests)
dotnet build WorkoutTracker.Web.E2ETests/ --verbosity minimal        # E2E compilation check
dotnet test WorkoutTracker.Web.E2ETests/ --verbosity minimal         # E2E tests (5 tests, requires app running)
dotnet test --verbosity minimal                                      # All tests (109 total)

# Individual test commands
dotnet test --filter "FullyQualifiedName~TestName"                   # Run specific test by name
dotnet test --filter "Name~Exercise"                                 # Run tests containing "Exercise"
dotnet test --filter "ClassName=ExerciseCriticalJourneyTests"        # Run tests from specific class
dotnet test --verbosity normal                                       # Show test names in output
dotnet test --verbosity detailed                                     # Show detailed test info and timing

# E2E testing workflow (automatic server management)
dotnet test WorkoutTracker.Web.E2ETests/ --verbosity normal           # Server starts/stops automatically
```

## Guidance Approach

- Explain .NET concepts and relate to Ruby/Rails when applicable
- Show EF Core generated SQL for every query
- Guide toward writing repository interfaces first, then implementations
- Compare .NET patterns to work patterns when relevant
- Build incrementally, test each layer as we add it
- Focus on enterprise patterns and real-world applicability

## Testing Strategy & Lessons Learned ✅

### Current Test Status
- **Unit Tests**: 11/11 passing (100%) ✅
- **Integration Tests**: 11/11 passing (100%) ✅  
- **Infrastructure Tests**: 87/87 passing (100%) ✅
- **E2E Tests**: 5/5 compiled and functional ✅
- **Total**: 109 tests across Testing Trophy layers

### Testing Philosophy & Framework Alignment ✅

**🏆 Testing Trophy + Microsoft .NET: Perfect Synergy**

Our testing approach combines Testing Trophy principles with Microsoft's official .NET recommendations. These methodologies enhance each other rather than conflict:

**✅ Shared Core Principles**:
- **Integration-First**: Both favor integration tests over excessive unit testing
- **Real Database Testing**: Testing Trophy's "test against real systems" aligns with Microsoft's "test against production database system" 
- **ROI-Focused**: Both prioritize tests that provide maximum confidence with reasonable effort
- **Simplicity**: Both emphasize maintainable, understandable test code

**🎯 Hybrid Approach Benefits**:
- **Testing Trophy Structure**: More integration tests, strategic unit tests, essential E2E tests
- **Microsoft Reliability**: Proven .NET patterns (SQLite in-memory, WebApplicationFactory)
- **Faster Execution**: Simple test data creation over complex builders
- **Better Maintainability**: Direct object construction following Microsoft's "minimally passing tests" principle

### Test Data Creation Strategy

**✅ Microsoft's Recommended Approach** (Applied):
```csharp
// ✅ PREFERRED - Direct object creation (Microsoft best practice)
var program = new Program { Name = "Test Program", Description = "Test Description" };
var exercise = new Exercise { Name = "Push Ups", PrimaryMuscle = "Chest" };
var programExercise = new ProgramExercise 
{ 
    ProgramId = program.Id, 
    ExerciseId = exercise.Id,
    Order = 1,
    PlannedSets = 3,
    PlannedReps = 10
};

// ✅ USE - Theory/InlineData for parameterized tests
[Theory]
[InlineData("Beginner Program", "For beginners")]
[InlineData("Advanced Program", "For experienced users")]
public async Task CreateProgram_WithValidData_Success(string name, string description)
```

```csharp
// ❌ AVOID - Complex builders that create unintended entity relationships
var programExercise = await Builder.ProgramExercise.CreateAsync(); // Creates separate program!

// ✅ PREFERRED - Use builders for single entities, direct creation for relationships
var program = await Builder.Program("Test Program").SaveAsync();
var exercise = await Builder.Exercise("Push Ups", "Chest").SaveAsync();
var programExercise = new ProgramExercise { ProgramId = program.Id, ExerciseId = exercise.Id };
```

**Decision Framework**:
- **Simple entities**: Use builders (Program, Exercise, Workout)
- **Entity relationships**: Use direct object creation  
- **Multiple test scenarios**: Use Theory/InlineData
- **Complex setup**: Keep it simple, avoid over-engineering

### Critical Learnings Applied

**✅ Database Testing Best Practice**:
```csharp
// ❌ AVOID - EF Core InMemory (Microsoft officially discourages for integration tests)
options.UseInMemoryDatabase("TestDb");

// ✅ USE - SQLite in-memory (Microsoft's recommended approach)
options.UseSqlite("DataSource=:memory:");
```

**✅ Integration Test Configuration**:
```csharp
// Critical for testing redirects and form submissions
var client = factory.CreateClient(new WebApplicationFactoryClientOptions
{
    AllowAutoRedirect = false  // Allows testing 302 responses
});
```

**✅ CSRF Token Handling**:
```csharp
// All form submissions must include anti-forgery tokens
// Extract from form page and include in POST data
formData["__RequestVerificationToken"] = extractedToken;
```

### Project-Specific Testing Insights

1. **Razor Pages Testing**: 
   - Route parameters must match exactly: `@page "{id:int}"`
   - Page titles and content need exact matches in assertions
   - Card-based layouts require different selectors than table-based

2. **Repository Pattern Testing**:
   - SQLite in-memory works perfectly with Clean Architecture
   - Database context isolation requires `ChangeTracker.Clear()`
   - Entity IDs are properly generated with SQLite (unlike InMemory)

3. **E2E Test Setup**:
   - Playwright requires global CLI tool: `dotnet tool install --global Microsoft.Playwright.CLI`
   - Browser installation: `playwright install`
   - Use absolute URLs with base URL pattern for navigation

4. **Common Pitfalls Avoided**:
   - Don't mix EF Core InMemory with integration testing
   - Always check Microsoft's current testing recommendations
   - Validate fundamental test setup before writing many tests
   - Use current Playwright API syntax, not outdated blog examples

### Future Testing Approach

**🏆 Testing Trophy + Microsoft .NET Hybrid Strategy**:

**Phase 1 - Integration-First (Testing Trophy Core)**:
1. **Start with 1 integration test** (HTTP pipeline validation)
   - Use WebApplicationFactory with real database flow
   - Test controller → repository → database roundtrip
   - Follow Microsoft's "test against production database system" principle

**Phase 2 - Strategic Unit Tests (Microsoft Simplicity)**:
2. **Add unit tests for pure business logic only**
   - Use Theory/InlineData for multiple scenarios (Microsoft recommended)
   - Direct object creation over complex builders
   - Focus on functions with no external dependencies

**Phase 3 - Critical User Journeys (Testing Trophy ROI)**:
3. **Add E2E test for critical workflows only**
   - Use Playwright with automatic server management
   - Test complete user scenarios that generate business value

**Phase 4 - Validation & Iteration**:
4. **Validate test strategy before scaling up**
   - Ensure tests run fast (Microsoft: milliseconds for unit tests)
   - Verify good isolation between tests
   - Maintain Testing Trophy distribution (more integration than unit)

**Decision Matrix for Test Type**:
- **Repository Methods**: Integration tests with SQLite in-memory
- **Business Logic**: Unit tests with Theory/InlineData  
- **Web Endpoints**: Integration tests with WebApplicationFactory
- **User Workflows**: E2E tests with Playwright
- **Entity Validation**: Unit tests with direct object creation

**Reference Documentation**:
- Complete analysis: `./automation_testing_lessons_learned.md`
- Testing strategy: `./automation_test_strategy.md`
- All fixes applied: `./test_fix_TODO.md`

### Testing Context for Future Work

**🏆 Testing Trophy + Microsoft .NET Principles** (Always Apply):

**Core Standards**:
- SQLite in-memory is the established standard (not EF Core InMemory)
- Integration tests use TestWebApplicationFactory with AllowAutoRedirect = false
- E2E tests have automatic server management (no manual startup required)
- E2E tests require Playwright global CLI tool and browser installation
- Form submissions need CSRF token extraction and inclusion
- All 109 tests are currently passing - validate changes don't break existing tests

**Microsoft .NET Best Practices**:
- **Test Data Creation**: Use direct object construction over complex builders for entity relationships
- **Parameterized Tests**: Prefer Theory/InlineData over creating multiple similar test methods
- **Entity Framework**: Test repository methods with SQLite in-memory (Microsoft approved)
- **Test Structure**: Follow Arrange-Act-Assert pattern with minimal setup
- **Test Scope**: One Act per test, focus on behavior not implementation

**Testing Trophy Principles**:
- **Integration-First**: Start with integration tests, add unit tests strategically
- **ROI Focus**: More integration tests than unit tests, essential E2E tests only
- **Real Systems**: Test against real database behavior when possible
- **User Value**: Prioritize tests for critical user workflows

**Development Workflow**:
- Make sure to build the corresponding projects after adding code. Especially when adding Razor pages. You must build the web project.
- Make sure to write automated tests for all feature requests following the hybrid approach
- Validate new tests don't break existing 109 passing tests
- Use the Decision Matrix to choose appropriate test type for each scenario