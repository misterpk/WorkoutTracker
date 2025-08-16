# Workout Tracker - Project Decisions Summary

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
- **Learning focus:** Understand generated SQL through EF Core logging

### Development Environment
- **.NET Version:** 8.0.401
- **IDE:** VS Code with C# Dev Kit (macOS), Visual Studio (Windows)
- **Database:** SQL Server in Docker via Colima (macOS)
- **Architecture:** Clean Architecture with simplified 3-project structure

### Learning Methodology
- **Approach:** Build fundamentals first, then introduce frameworks
- **Pattern:** Build twice - once vanilla, then with framework to see benefits
- **Guidance Style:** Guide toward solutions rather than providing complete code

## Application Scope (MVP)

### Core Features
- **Focus:** Resistance training tracking with program templates
- **User Flow:** Create programs → Build workouts from programs → Log sets/reps/weight
- **Core Entities:** 
  - **Exercise** (name, description, primary_muscle) - The movements
  - **Program** (template/plan with exercises) - Reusable workout templates
  - **ProgramExercise** (planned exercise in program with order, planned sets/reps/weight/rest)
  - **Workout** (actual session, references program, can be modified) - Specific training sessions
  - **WorkoutExercise** (exercise in actual workout) - Actual exercises performed
  - **Set** (actual reps/weight performed) - Performance data

### Progress Tracking
- Strength gains over time
- Volume gains over time  
- Workout frequency (per week/month/quarter/year)
- Time spent working out

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

## Phase 1 Completed: Development Environment Setup

### What We Built
- .NET 8 solution with simplified Clean Architecture (3 projects)
- SQL Server Docker container configuration
- VS Code development environment with C# Dev Kit
- Project structure following enterprise patterns without over-engineering

### Key Decisions
- Simplified architecture: Web → Infrastructure → Core (removed Shared)
- Repository pattern on top of Entity Framework Core
- Code First approach for database schema management
- Hybrid web/API controllers to serve both browsers and mobile clients
- DTOs live in Web project to avoid unnecessary complexity
- **Domain Model:** Program/Workout separation (templates vs actual sessions)
- **Data Types:** UTC timestamps, float for weights, non-nullable for performance
- **Navigation Properties:** EF Core relationships with `= null!` pattern

## Next Steps
1. Set up project references and dependencies
2. Create Entity Framework models and DbContext
3. Implement repository pattern interfaces and implementations
4. Build first Razor Pages for exercise management
5. Add API endpoints alongside web views
6. Create .NET MAUI mobile app

## Development Philosophy
- **Learning over speed** - Understanding problems before solutions
- **Fundamentals first** - Build from ground up, then use abstractions
- **Incremental complexity** - Add features and concepts gradually
- **Practical application** - Build something usable while learning
- **Test-driven learning** - Write unit, integration, and end-to-end tests as we go