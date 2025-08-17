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

## Technology Stack (Updated)
- **Backend:** .NET 8 (ASP.NET Core Web API + Razor Pages)
- **Frontend Phase 1:** Razor Pages (server-side rendering)
- **Frontend Phase 2:** Blazor (rebuild same functionality)
- **Database:** SQL Server in Docker (with EF Core Code First)
- **ORM:** Entity Framework Core with Repository pattern
- **Architecture:** Clean Architecture (layered approach)

## Development Environment
- **OS:** macOS with Docker via Colima
- **IDE:** VS Code with C# Dev Kit extension
- **Database:** SQL Server in Docker container
- **Container name:** `sqlserver`
- **Connection string:** `Server=localhost,1433;Database=master;User Id=SA;Password=YourStrong@Passw0rd;TrustServerCertificate=true;`
- **.NET Version:** 8.0.401
- **Database tools:** sqlcmd CLI + SQL Server extension for VS Code

## Project Structure (Simplified Clean Architecture)
```
WorkoutTracker/
├── WorkoutTracker.sln              # Solution file
├── WorkoutTracker.Core/            # Domain models, business logic, interfaces
├── WorkoutTracker.Infrastructure/  # EF Core, repositories, data access
├── WorkoutTracker.Web/            # ASP.NET Core app (Razor Pages + DTOs)
└── WorkoutTracker.Tests/          # Test projects (to be added)
    ├── WorkoutTracker.Core.Tests/     # Unit tests for domain logic
    ├── WorkoutTracker.Infrastructure.Tests/  # Integration tests for data access
    └── WorkoutTracker.Web.Tests/      # End-to-end tests for web app
```

**Dependencies:**
- Web → Infrastructure (repositories, data access)
- Infrastructure → Core (domain models, interfaces)
- Core → (no dependencies - pure domain logic)
- Tests → Respective projects they're testing

## Learning Progression

### Phase 1: ASP.NET Core + Razor Pages + EF Core ✅ (Setup Complete)
- Clean Architecture with layered projects
- Entity Framework Core Code First approach
- Repository pattern on top of EF Core
- Server-side rendering with Razor Pages
- SQL visibility through EF Core logging

### Phase 2: API Endpoints + Hybrid Controllers 📋 (Next)
- Add JSON API responses alongside Razor Page views
- Same controllers serve both web browsers and API clients
- Prepare backend for mobile consumption

### Phase 3: Convert Web Frontend to Blazor 📋 (Future)
- Keep same backend/data layer and API endpoints
- Rebuild web frontend with Blazor Server
- Compare server-rendered vs SPA-like approaches

### Phase 4: .NET MAUI Mobile App 📋 (Future)
- Build cross-platform mobile app (iOS/Android)
- Consume same API endpoints as web app
- Single C# codebase across web, mobile, backend

### Phase 5: iOS Native Exploration 📋 (Future)
- Compare native iOS development to MAUI approach
- Understand trade-offs between cross-platform and native

### Completed Setup ✅
- .NET 8 SDK installed and verified (8.0.401)
- VS Code with C# Dev Kit extension
- Simplified solution with 3 projects created
- Project references configured (Web → Infrastructure → Core)
- SQL Server Docker container available
- **Complete domain model:** Exercise, Program, ProgramExercise, Workout, WorkoutExercise, Set
- **Repository interfaces:** IExerciseRepository, IProgramRepository, IWorkoutRepository, IWorkoutExerciseRepository, ISetRepository
- Git repository with proper .gitignore and GitHub integration

## Application Requirements

### MVP Scope
- **Focus:** Resistance training with program templates
- **Core entities:** 
  - **Exercise** (name, description, primary_muscle)
  - **Program** (reusable workout template)
  - **ProgramExercise** (planned exercise with order, sets/reps/weight/rest)
  - **Workout** (actual session, references program, can be modified)
  - **WorkoutExercise** (exercise in actual workout)  
  - **Set** (actual reps/weight performed)
- **User flow:** Create programs → Build workouts from programs → Log actual performance
- **Key features:** Program templates, freestyle workouts, planned vs actual tracking

### API Endpoints (Hybrid Web/API Design)
```
GET  /                    # Home page (Razor) + API root
GET  /exercises           # List exercises page + JSON API
POST /exercises           # Create exercise form + JSON API
GET  /workouts           # List workouts page + JSON API
POST /workouts           # Create workout form + JSON API
POST /sets               # Log a set form + JSON API

# Same controllers return:
# - Razor Page views for browser requests
# - JSON responses for API/mobile requests
```

### Data Models (Current EF Core Entities)
```csharp
// All domain models completed ✅
public class Exercise { ... }
public class Program { ... }  
public class ProgramExercise { ... }
public class Workout { ... }
public class WorkoutExercise { ... }
public class Set { ... }

// All repository interfaces completed ✅
public interface IExerciseRepository { ... }
public interface IProgramRepository { ... }
public interface IWorkoutRepository { ... }
public interface IWorkoutExerciseRepository { ... }
public interface ISetRepository { ... }

// Next: EF Core implementation 📋
public class WorkoutContext : DbContext { ... }
public class ExerciseRepository : IExerciseRepository { ... }
```

## Enterprise Patterns to Learn
- **Clean Architecture** - Separation of concerns across layers ✅
- **Repository Pattern** - Abstraction over data access (on top of EF Core) 📋
- **Dependency Injection** - Built into .NET, will use throughout 📋
- **Code First Migrations** - Schema management through C# code 📋
- **LINQ** - Query abstraction over SQL 📋
- **Domain Modeling** - Program vs Workout separation, planned vs actual tracking ✅
- **Navigation Properties** - EF Core relationships and lazy loading 🔄

## Code Style Preferences
- **Explicit types** - Use strong typing, avoid `var` when learning
- **Entity Framework logging** - Always show generated SQL for learning
- **Repository interfaces** - Abstract data access like at work
- **Layered architecture** - Web → Core → Infrastructure dependencies

## Docker & Database Context
```bash
# SQL Server container management
docker ps                           # Check if sqlserver container running
docker start sqlserver             # Start if stopped
docker exec -it sqlserver sqlcmd   # Connect to SQL Server

# .NET EF Core commands (future)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Current Status
- ✅ .NET 8 SDK installed (8.0.401)
- ✅ VS Code with C# Dev Kit ready
- ✅ Simplified solution created with 3 projects
- ✅ Project references configured
- ✅ SQL Server Docker container available
- ✅ **Complete domain model:** Exercise, Program, ProgramExercise, Workout, WorkoutExercise, Set
- ✅ **Repository interfaces:** All 5 repository interfaces defined with async patterns
- 🔄 **Next:** Set up Entity Framework Core in Infrastructure project
- 📋 **After that:** Create DbContext and configure entity relationships

## Learning Focus Areas
1. **Clean Architecture** ✅ (completed project structure and references)
2. **Domain Modeling** ✅ (completed all 6 entities with relationships)
3. **Repository Pattern** ✅ (defined all interfaces with async patterns)
4. **Entity Framework Core** 🔄 (next - setup and configuration)
5. **Code First Migrations** 📋 (generate SQL schema from models)
6. **Automated Testing** 📋 (unit, integration, end-to-end tests)
7. **Hybrid Controllers** 📋 (serving both web and API responses)
8. **Razor Pages** 📋 (server-side rendering)
9. **Dependency Injection** 📋 (.NET's built-in DI container)
10. **SQL understanding** 📋 (through EF Core query logging)
11. **Blazor** 📋 (future phase 3)
12. **.NET MAUI** 📋 (future phase 4 - cross-platform mobile)
13. **iOS Native** 📋 (future phase 5 - comparison study)

## Quick Reference
```bash
# Project setup
cd ~/WorkoutTracker
dotnet build                    # Build entire solution (3 projects)
dotnet run --project WorkoutTracker.Web  # Run web application

# Project structure
dotnet sln list                 # Shows Core, Infrastructure, Web
```

## Guidance Approach
- Explain .NET concepts and relate to Ruby/Rails when applicable
- Show EF Core generated SQL for every query
- Guide toward writing repository interfaces first, then implementations
- Compare .NET patterns to work patterns when relevant
- Build incrementally, test each layer as we add it