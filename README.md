# 🧠 Exam Online API – Clean Architecture & Vertical Slice Design

> A **Modern, Scalable, and Maintainable** Online Examination System built with **.NET 8**, implementing **Clean Architecture**, **Vertical Slice Architecture**, **CQRS with MediatR**, **FluentValidation**, and **Entity Framework Core**.

---

## 📘 Overview

**Exam Online API** is an enterprise-grade backend solution designed for online examination platforms.  
It is built using **Clean Architecture** and **Vertical Slice principles**, focusing on **separation of concerns**, **feature isolation**, and **testability**.

Every feature (Category, Exam, Dashboard, Analytics, etc.) is **self-contained** with its own commands, queries, handlers, and validations — ensuring modularity, scalability, and long-term maintainability.

This architecture makes the system highly extensible for real-world production use — such as online education systems, corporate exam platforms, or university evaluation portals.

---

## 🧱 Architecture Philosophy

### 🧩 Vertical Slice Architecture
Instead of separating by technical layers (Controllers, Services, Repositories), this architecture groups code **by feature**.  
Each feature slice is **independent** and can evolve separately.

```
📦 Exam_Online_API
 ┣ 📂 Application
 ┃ ┣ 📂 Common
 ┃ ┣ 📂 Extensions
 ┃ ┣ 📂 Factories
 ┃ ┗ 📂 Features
 ┃    ┗ 📂 Admin
 ┃       ┣ 📂 Categories
 ┃       ┣ 📂 Exams
 ┃       ┗ 📂 Dashboard
 ┣ 📂 Domain
 ┣ 📂 Infrastructure
 ┗ 📂 API
```

### 🧼 Clean Architecture Layers
- **Domain Layer** → Core entities, enums, and business rules  
- **Application Layer** → CQRS (Commands, Queries, Handlers, Validators)  
- **Infrastructure Layer** → EF Core, caching, file storage, identity, logging  
- **Presentation (API)** → Controllers that expose endpoints via MediatR  

---

## 🚀 Core Features

| Category | Description |
|-----------|--------------|
| 🗂 **Categories Management** | Create, update, delete, and analyze categories |
| 🧾 **Exam Management** | CRUD operations, scheduling, impact analysis, and file uploads |
| 📊 **Admin Dashboard** | Global statistics for users, categories, exams, and performance |
| 💾 **Caching Layer** | Hybrid (Memory + Distributed) cache with smart invalidation |
| 🧩 **CQRS Architecture** | Commands for mutations, Queries for reads |
| 🧠 **Validation Pipeline** | FluentValidation integrated with MediatR Pipeline Behavior |
| 🔄 **Background Jobs** | File cleanup and cache maintenance using Hangfire |
| 🔐 **Role-based Auth** | Built-in `User`, `Admin`, `SuperAdmin` roles via ASP.NET Identity |
| 🧰 **Exception Handling** | Centralized `GlobalExceptionHandler` for consistent responses |
| 🧑‍💻 **Developer Experience** | Feature-based foldering, strong logging, and modular testing |

---

## 🧠 Technical Highlights

- **CQRS + MediatR** → Separation of reads/writes for clarity and performance  
- **FluentValidation** → Request validation integrated with MediatR pipeline  
- **GlobalExceptionHandler** → Converts exceptions into consistent JSON error responses  
- **Entity Factories** → Encapsulate entity creation logic for clean domain modeling  
- **DbInitializer** → Auto-migrates and seeds data for smooth deployment  
- **CacheInvalidationService** → Keeps cache synchronized with database changes  
- **HybridCacheService** → MemoryCache + Distributed cache integration  
- **Hangfire Background Jobs** → Asynchronous cleanup of stale files and logs  
- **Logging** → Structured logging with ILogger and log level control  

---

## 🧩 Key Components Explained

### 🧰 ValidationBehavior (Pipeline)
Automatically validates every MediatR request using registered validators before handler execution.

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
```

### ⚙️ GlobalExceptionHandler
Handles all API exceptions (Validation, NotFound, Conflict, BusinessLogic, Unauthorized, etc.)  
and returns structured JSON responses.

### 🧾 Category Feature Example
Each feature includes:
- `Command` / `Query` classes  
- `Handler` implementing `IRequestHandler`  
- `Validator` using FluentValidation  
- `Controller` with clean REST endpoints  

For example:  
`CreateCategoryCommand`, `CreateCategoryHandler`, and `CreateCategoryValidator`.

---

## 🧰 Setup Instructions

### 🪜 Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- (Optional) [Hangfire Dashboard](https://www.hangfire.io/)

---

### ▶️ Getting Started

```bash
# Clone the repository
git clone https://github.com/<your-username>/Exam_Online_API.git
cd Exam_Online_API

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

---

### 👤 Default Admin Credentials

| Role | Email | Password |
|------|--------|----------|
| SuperAdmin | `eslamklyep2019@gmail.com` | `@AanTk5A7V1W2h99Thm` |

*(Auto-created by `DbInitializer` during first run)*

---

## 📡 API Endpoints Overview

| Endpoint | Method | Description |
|-----------|---------|-------------|
| `/api/admin/categories` | `GET` | Get all categories (paginated, searchable) |
| `/api/admin/categories` | `POST` | Create a new category |
| `/api/admin/categories/{id}` | `PUT` | Update category |
| `/api/admin/categories/{id}` | `DELETE` | Delete category |
| `/api/admin/categories/deletion-impact/{id}` | `GET` | Analyze data impact before deletion |
| `/api/admin/exams` | `GET` | Retrieve exams with filters & sorting |
| `/api/admin/exams` | `POST` | Create a new exam |
| `/api/admin/exams/deletion-impact/{id}` | `GET` | Get deletion impact for specific exam |
| `/api/admin/dashboard/stats` | `GET` | Get overall system statistics |
| `/api/admin/reports/exams` | `GET` | Generate exam performance reports |

---

## 🧠 Example Request Flow

1. **Request Validation** → `ValidationBehavior`
2. **Handler Execution** → Business logic in `Handler`
3. **Entity Creation** → via `Factory` classes
4. **Database Persistence** → via `ApplicationDbContext`
5. **Cache Refresh** → via `CacheInvalidationService`
6. **Response Returned** → Clean DTO response to client

---

## 🧩 Folder Naming Convention (Clean & Vertical)

| Folder | Purpose |
|---------|----------|
| `Application/Common` | Shared behaviors, exceptions, and services |
| `Application/Features` | Each feature is isolated as a vertical slice |
| `Domain/Entities` | Business domain models |
| `Infrastructure/Persistence` | EF Core DbContext and migration configuration |
| `Infrastructure/Services` | Caching, file handling, background jobs |
| `API/Controllers` | Thin controllers — only call MediatR |

---

## 📊 Example: Category Creation Response

```json
{
  "id": "3b1e6e6f-1a77-4b44-bc19-d3a4cb72c839",
  "title": "Mathematics",
  "iconPath": "/uploads/categories/math.png",
  "createdAt": "2025-10-19T10:35:22Z",
  "message": "Category created successfully"
}
```

---

## 🧩 Design Patterns Used

- CQRS (Command-Query Responsibility Segregation)
- Factory Pattern
- Mediator Pattern
- Repository Abstraction (via EF Core)
- Validation Pipeline
- Caching Pattern
- Dependency Injection
- Background Job Pattern

---

## 🧠 Project Purpose

This project is more than just an exam API —  
it’s a **blueprint** for building enterprise-level applications with **Clean + Vertical Slice Architecture** in .NET.

It provides:
- Reusable architectural skeleton  
- Scalable modular design  
- Ready-to-extend structure for future features (Students, Results, Authentication, etc.)

---

## 👨‍💻 Author

**🧑‍💻 Islam Klieb**  
🌐 GitHub: [github.com/yourusername](https://github.com/islam-klieb)  
🏗️ Designed with passion for clean, maintainable, enterprise-ready .NET architecture.

---

## 📜 License

This project is released under the **MIT License**.  
You are free to use, modify, and distribute with proper attribution.

---

### 🌟 Support & Contribution

If you like this project, consider giving it a **⭐ star** on GitHub!  
Feel free to fork, contribute, or report issues to help it grow.

> **“Clean Architecture is not just structure — it’s discipline.”** 🧩
