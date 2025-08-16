# Workout Tracker

A workout tracking web application built as a learning project to understand .NET 8, Entity Framework Core, and modern web development patterns.

## 🎯 Project Goals

This project serves as a hands-on learning experience for:
- **.NET 8** ecosystem and C# development
- **Clean Architecture** patterns and dependency management
- **Entity Framework Core** with Code First migrations
- **ASP.NET Core** with Razor Pages and Web APIs
- **Repository Pattern** and enterprise development practices

## 🏗️ Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
WorkoutTracker/
├── WorkoutTracker.Core/         # Domain models and business logic
├── WorkoutTracker.Infrastructure/   # Data access and external services  
└── WorkoutTracker.Web/         # ASP.NET Core application (Razor Pages)
```

**Dependencies:**
- Web → Infrastructure → Core
- Core has no external dependencies (pure domain logic)

## 🚀 Technology Stack

- **Backend:** .NET 8 (ASP.NET Core Web API + Razor Pages)
- **Database:** SQL Server in Docker with Entity Framework Core
- **Architecture:** Clean Architecture with Repository Pattern
- **Frontend (Phase 1):** Razor Pages (server-side rendering)
- **Frontend (Phase 2):** Blazor Server (planned)
- **Mobile (Future):** .NET MAUI cross-platform app

## 📋 Features

### Current (MVP Scope)
- **Exercise Management:** Create and manage resistance training exercises
- **Program Templates:** Build reusable workout programs with planned sets/reps/weights
- **Workout Tracking:** Execute programs and log actual performance vs planned

### Domain Models
- **Exercise:** Basic exercise definitions (name, description, primary muscle)
- **Program:** Reusable workout templates
- **ProgramExercise:** Planned exercises within programs (sets, reps, weight, rest)
- **Workout:** Actual workout sessions (planned)
- **Set:** Actual performance data (planned)

## 🛠️ Development Setup

### Prerequisites
- .NET 8 SDK (8.0.401+)
- Docker (for SQL Server)
- VS Code with C# Dev Kit extension

### Database Setup
```bash
# Start SQL Server container
docker start sqlserver

# Connection string (when implemented):
# Server=localhost,1433;Database=WorkoutTracker;User Id=SA;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
```

### Running the Application
```bash
# Build entire solution
dotnet build

# Run web application
dotnet run --project WorkoutTracker.Web

# Navigate to: https://localhost:5001
```

## 🗺️ Development Roadmap

### ✅ Phase 1: Foundation (Complete)
- [x] Clean Architecture project structure
- [x] Core domain models (Exercise, Program, ProgramExercise)
- [x] Project dependencies and references

### 🔄 Phase 2: Data Layer (In Progress)
- [ ] Entity Framework Core setup with Code First
- [ ] Repository interfaces and implementations
- [ ] Database migrations and seeding
- [ ] Complete remaining models (Workout, WorkoutExercise, Set)

### 📋 Phase 3: Web Layer (Next)
- [ ] Razor Pages for CRUD operations
- [ ] Hybrid controllers (Web + API responses)
- [ ] Form handling and validation
- [ ] Basic styling and user experience

### 📋 Phase 4: API Enhancement (Future)
- [ ] RESTful API endpoints
- [ ] JSON responses for mobile consumption
- [ ] API documentation

### 📋 Phase 5: Frontend Evolution (Future)
- [ ] Rebuild frontend with Blazor Server
- [ ] Compare server-rendered vs SPA approaches
- [ ] Performance and user experience analysis

### 📋 Phase 6: Mobile (Future)
- [ ] .NET MAUI cross-platform mobile app
- [ ] Consume same API endpoints as web app
- [ ] Cross-platform development experience

## 📚 Learning Focus Areas

1. **Clean Architecture** - Separation of concerns and dependency management
2. **Entity Framework Core** - Code First approach, migrations, LINQ
3. **Repository Pattern** - Abstraction over data access
4. **ASP.NET Core** - Razor Pages, Web APIs, dependency injection
5. **Domain Modeling** - Program vs Workout separation, planned vs actual tracking
6. **Enterprise Patterns** - Patterns applicable to professional development

## 🤝 Contributing

This is a personal learning project, but feedback and suggestions are welcome! Feel free to:
- Open issues for questions or suggestions
- Share insights about .NET best practices
- Discuss architectural decisions and trade-offs

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

---

*Built as a learning project to understand .NET 8, Entity Framework Core, and enterprise development patterns.*