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

# .NET EF Core commands (ready to use)
dotnet ef migrations add InitialCreate --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web
dotnet ef database update --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web
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
12. **Automated Testing** 📋 (unit, integration, end-to-end tests)
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
dotnet run --project WorkoutTracker.Web  # Run web application

# Project structure
dotnet sln list                 # Shows Core, Infrastructure, Web

# EF Core commands (ready to use)
dotnet ef migrations add InitialCreate --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web
dotnet ef database update --project WorkoutTracker.Infrastructure --startup-project WorkoutTracker.Web
```

## Guidance Approach
- Explain .NET concepts and relate to Ruby/Rails when applicable
- Show EF Core generated SQL for every query
- Guide toward writing repository interfaces first, then implementations
- Compare .NET patterns to work patterns when relevant
- Build incrementally, test each layer as we add it
- Focus on enterprise patterns and real-world applicability
