# Workout Tracker - Project Decisions Summary v2

## Project Overview
Building a workout tracking application to learn web development fundamentals, then modern frameworks.

**Primary Goal:** Learning fundamentals before using frameworks
**Secondary Goal:** Build a functional workout tracker for resistance training

## Technology Stack Decisions

### Backend Language & Framework
- **Chosen:** .NET 8 + ASP.NET Core
- **Considered:** Python + FastAPI, Node.js/JavaScript, Rust, PHP, Ruby on Rails
- **Reasoning:** 
  - Direct applicability to work environment (team migrating to .NET Core 8)
  - Learn Microsoft ecosystem end-to-end
  - Strong typing and enterprise patterns
  - Clean Architecture support built-in

### Frontend Approach
- **Phase 1:** Razor Pages (server-side rendering with .NET)
- **Phase 2:** Blazor Server (rebuild same functionality in C#)
- **Reasoning:** Learn both Microsoft frontend approaches, compare paradigms

### Mobile Strategy
- **Phase 1:** Build web app first
- **Phase 2:** Add API endpoints (JSON responses) alongside web views
- **Phase 3:** .NET MAUI mobile app consuming same APIs
- **Phase 4:** Explore iOS native development later
- **Benefits:** Single C# technology stack across web, mobile, and backend

### Database Approach
- **Database:** SQL Server in Docker container (macOS via Colima)
- **ORM:** Entity Framework Core with Code First approach
- **Pattern:** Repository pattern on top of EF Core (enterprise approach)
- **Configuration:** Fluent API with separate configuration classes per entity
- **Learning focus:** Understand generated SQL through EF Core logging

### Development Environment
- **.NET Version:** 8.0.401
- **IDE:** VS Code with C# Dev Kit (macOS), Visual Studio (Windows)
- **Database:** SQL Server in Docker via Colima (macOS)
- **Architecture:** Clean Architecture with simplified 3-project structure
- **Project Location:** `~/dev/dotnet/WorkoutTracker`

### Learning Methodology
- **Approach:** Build fundamentals first, then introduce frameworks
- **Pattern:** Build twice - once vanilla, then with framework to see benefits
- **Guidance Style:** Guide toward solutions rather than providing complete code

## Application Scope (MVP)

### Core Features
- **Focus:** Resistance training tracking with program templates
- **User Flow:** Create programs → Build workouts from programs → Log sets/reps/weight
- **Workout Types:** Template-based workouts AND freestyle workouts (no program required)
- **Core Entities:** 
  - **Exercise** (name, description, primary_muscle) - The movements
  - **Program** (template/plan with exercises) - Reusable workout templates
  - **ProgramExercise** (planned exercise in program with order, planned sets/reps/weight/rest)
  - **Workout** (actual session, optionally references program) - Specific training sessions
  - **WorkoutExercise** (exercise in actual workout) - Actual exercises performed
  - **Set** (actual reps/weight/RPE performed) - Performance data

### Progress Tracking
- Strength gains over time (weight progression)
- Volume gains over time (sets × reps × weight)
- Workout frequency (per week/month/quarter/year)
- Time spent working out
- RPE (Rate of Perceived Exertion) tracking
- Rest time monitoring

### User Management
- **Phase 1:** Single user (learning focus)
- **Phase 2:** Add authentication and multi-user support

## Architecture Decisions

### Web Application
- Start with web app first
- Focus on understanding HTTP fundamentals
- Progressive enhancement approach

### Mobile Applications  
- **Phase 3:** .NET MAUI (Multi-platform App UI)
- **Approach:** Same C# codebase for iOS/Android
- **Data Strategy:** Consume same API endpoints as web app
- **Timeline:** After web app and API endpoints are complete
- **Future exploration:** iOS native development

### Deployment & Cost
- **Target:** AWS (learning industry standard)
- **Budget:** Under $10/month
- **Approach:** Start simple, optimize for cost

## Phase 1 Completed: Entity Framework Core Setup

### What We Built
- .NET 8 solution with simplified Clean Architecture (3 projects)
- SQL Server Docker container configuration
- VS Code development environment with C# Dev Kit
- Project structure following enterprise patterns without over-engineering
- **Complete domain model:** Exercise, Program, ProgramExercise, Workout, WorkoutExercise, Set
- **Repository interfaces:** Full abstraction layer for data access with async patterns
- **BaseModel pattern:** Common audit fields (Id, CreatedAt, UpdatedAt) with automatic timestamp management
- **Nullable reference types:** Enabled across solution for better null safety
- **Entity Framework Core:** Complete DbContext with Fluent API configuration
- **Separate configuration classes:** Each entity has its own IEntityTypeConfiguration implementation

### Key Architecture Decisions
- Simplified architecture: Web → Infrastructure → Core (removed Shared)
- Repository pattern on top of Entity Framework Core
- Code First approach for database schema management
- Hybrid web/API controllers to serve both browsers and mobile clients
- DTOs live in Web project to avoid unnecessary complexity
- **Domain Model:** Program/Workout separation (templates vs actual sessions)
- **Data Types:** UTC timestamps, decimal for weights, int for reps, nullable for optional fields
- **Navigation Properties:** ICollection<T> for all one-to-many relationships
- **Interface Design:** YAGNI principle - only essential methods, add complexity when needed
- **BaseModel inheritance:** All entities inherit common audit fields with automatic SaveChanges handling
- **Primary Keys:** int identity (auto-increment) for simplicity and performance
- **Foreign Keys:** EF Convention (ProgramId, WorkoutId, etc.)

### Entity Framework Core Design Decisions

#### Relationship Patterns
- **Exercise ↔ ProgramExercise:** One-to-many with back-reference for "programs using this exercise"
- **Program ↔ ProgramExercise:** One-to-many with cascade delete (cleanup program templates)
- **Program ↔ Workout:** One-to-many with restrict delete (preserve historical workout data)
- **Exercise ↔ WorkoutExercise:** One-to-many with restrict delete (preserve historical performance data)
- **Workout ↔ WorkoutExercise:** One-to-many with cascade delete (cleanup workout data)
- **WorkoutExercise ↔ Set:** One-to-many with cascade delete (cleanup performance data)

#### Business Rules Enforced by Database
- **ProgramExercise:** Unique constraint on (ProgramId, ExerciseId) - one exercise per program template
- **WorkoutExercise:** Unique constraint on (WorkoutId, ExerciseId) - one exercise entry per workout
- **Set:** Unique constraint on (WorkoutExerciseId, Order) - proper set sequencing

#### Data Precision Decisions
- **Weight:** decimal(5,2) for exact precision (handles 999.99 lbs/kg)
- **String lengths:** 100 chars for names, 500 chars for descriptions
- **Optional relationships:** Program ↔ Workout (supports freestyle workouts)
- **Automatic audit trails:** CreatedAt/UpdatedAt set via SaveChanges override

### Project Structure
```
WorkoutTracker/
├── WorkoutTracker.sln              # Solution file
├── WorkoutTracker.Core/            # Domain models, business logic, interfaces
│   ├── Models/                     # Domain models (BaseModel and entities)
│   │   ├── BaseModel.cs            # Base class with Id, CreatedAt, UpdatedAt
│   │   ├── Exercise.cs             # Navigation: ProgramExercises collection
│   │   ├── Program.cs              # Navigation: ProgramExercises, Workouts collections
│   │   ├── ProgramExercise.cs      # FK: ProgramId, ExerciseId; Navigation: Program, Exercise
│   │   ├── Workout.cs              # FK: ProgramId (nullable); Navigation: Program, WorkoutExercises
│   │   ├── WorkoutExercise.cs      # FK: WorkoutId, ExerciseId; Navigation: Workout, Exercise, Sets
│   │   └── Set.cs                  # FK: WorkoutExerciseId; Navigation: WorkoutExercise
│   └── Interfaces/                 # Repository interfaces
│       ├── IExerciseRepository.cs
│       ├── IProgramRepository.cs
│       ├── IWorkoutRepository.cs
│       ├── IWorkoutExerciseRepository.cs
│       └── ISetRepository.cs
├── WorkoutTracker.Infrastructure/  # EF Core, repositories, data access
│   ├── Data/                       # DbContext and configurations
│   │   ├── WorkoutTrackerDbContext.cs     # Main DbContext with automatic audit handling
│   │   └── Configurations/               # Fluent API entity configurations
│   │       ├── ExerciseConfiguration.cs
│   │       ├── ProgramConfiguration.cs
│   │       ├── ProgramExerciseConfiguration.cs
│   │       ├── WorkoutConfiguration.cs
│   │       ├── WorkoutExerciseConfiguration.cs
│   │       └── SetConfiguration.cs
│   └── Repositories/               # Repository implementations (to be built)
└── WorkoutTracker.Web/            # ASP.NET Core app (Razor Pages + DTOs)
    ├── appsettings.json            # Connection string configuration
    └── Program.cs                  # DI container setup (to be configured)
```

## Next Steps
1. **Dependency Injection Setup** ⏳ - Configure DbContext in Program.cs
2. Generate first EF Core migration and review generated SQL
3. Apply migration to create database schema
4. Implement repository pattern with EF Core
5. Create test projects for unit/integration testing
6. Build first Razor Pages for exercise management
7. Add SQL logging to see generated queries

## Development Philosophy
- **Learning over speed** - Understanding problems before solutions
- **Fundamentals first** - Build from ground up, then use abstractions
- **Incremental complexity** - Add features and concepts gradually
- **Practical application** - Build something usable while learning
- **Test-driven learning** - Write unit, integration, and end-to-end tests as we go
- **Enterprise patterns** - Repository pattern, Clean Architecture, Fluent API configuration
- **Code quality** - Nullable reference types, explicit configuration, separation of concerns