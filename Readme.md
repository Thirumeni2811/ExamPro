
## **Exam Invigilator Allocation System - ExamPro**

## Project Overview

This is a C# ASP.NET Core MVC layered application** designed to allocate invigilators to exam venues based on their availability. The system supports **Admin and Invigilator dashboards**, dynamic allocation, real-time updates, and token-based authentication.

---


#### I N I T I A L   C O M M I T


## Features Implemented (So Far)

* **Layered Architecture**:

  * `Exam.Domain` – Models, Enums, Response Interfaces
  * `Exam.DataAccess` – EF Core DbContext, Fluent Configurations
  * `Exam.Service` – (To be implemented) Business logic
  * `Exam.Web` – (To be implemented) Web UI for Admin & Invigilators
  
* **Database with EF Core Migrations**

* **Tables Created**:

  * `Users`
  * `Venues`
  * `InvigilatorAvailabilities`
  * `Allocations`
  
* **Enums**:

  * User Roles: `Admin`, `Invigilator`
  * DayOfWeek: `Monday` → `Sunday`
  * Allocation Status: `Draft`, `Published`
 
* **Constraints & Validations**:

  * Unique email in `Users`
  * Unique (InvigilatorId + DayOfWeek) in `InvigilatorAvailabilities`
  * Unique (VenueId + Date) in `Allocations`
  
* **CreatedAt** default timestamp for all tables

---

## Tech Stack

* **Framework**: .NET 9
* **Language**: C#
* **Architecture**: MVC with Layered Pattern
* **Database**: SQL Server
* **ORM**: Entity Framework Core
* **Authentication**: JWT (planned)
* **Password Hashing**: BCrypt (planned)

---

## Folder Structure

```
ExamSystem/
│
├── Exam.Domain/
│   ├── Models/        # User, Venue, InvigilatorAvailability, Allocation
│   ├── Enums/         # Role, DayOfWeek, AllocationStatus
│   └── ResponseFormat # IServiceResponse, ActionType
│
├── Exam.DataAccess/
│   ├── ExamDbContext.cs
│   ├── ExamDbContextFactory.cs
│   └── Migrations/
│
├── Exam.Service/      # (To be implemented)
└── Exam.Web/          # (To be implemented)
```

---

## Database Design

**Database Name**: `Exam`

### **Tables**

#### 1. Users

| Column        | Type             | Notes                |
| ------------- | ---------------- | -------------------- |
| Id            | UNIQUEIDENTIFIER | PK                   |
| Name          | NVARCHAR(100)    | Required             |
| Email         | NVARCHAR(150)    | Unique               |
| Password      | NVARCHAR(MAX)    | Required             |
| Role          | INT              | Enum                 |
| ProfileImage  | NVARCHAR(255)    | Optional             |
| ContactNumber | NVARCHAR(20)     | Optional             |
| Active        | BIT              | Default TRUE         |
| CreatedAt     | DATETIME         | Default GETUTCDATE() |

#### 2. Venues

| Column     | Type          | Notes                |
| ---------- | ------------- | -------------------- |
| Id         | INT IDENTITY  | PK                   |
| Name       | NVARCHAR(100) | Required             |
| HallNumber | NVARCHAR(50)  | Required             |
| CreatedAt  | DATETIME      | Default GETUTCDATE() |

#### 3. InvigilatorAvailabilities

| Column        | Type             | Notes                |
| ------------- | ---------------- | -------------------- |
| Id            | INT IDENTITY     | PK                   |
| InvigilatorId | UNIQUEIDENTIFIER | FK → Users(Id)       |
| DayOfWeek     | INT              | Enum                 |
| CreatedAt     | DATETIME         | Default GETUTCDATE() |

#### 4. Allocations

| Column        | Type             | Notes                          |
| ------------- | ---------------- | ----------------------         |
| Id            | INT IDENTITY     | PK                             |
| VenueId       | INT              | FK → Venues(Id)                |
| InvigilatorId | UNIQUEIDENTIFIER | FK → Users(Id)                 |
| Date          | DATE             | Required                       |
| Status        | INT              | Enum (Draft/Published/Approved)|
| CreatedAt     | DATETIME         | Default GETUTCDATE()           |

---

## Setup Instructions

### 1. Install Dependencies

* **.NET SDK** (9)

* **EF Core Tools**:
* EntityFrameworkCore
* EntityFrameworkCore.SqlServer
* EntityFrameworkCore.Tools

```bash
dotnet tool install --global dotnet-ef
```

### 2. Configure Connection String

Update **ExamDbContextFactory.cs**:

```csharp
const string connectionString =
    "Server=.;Database=Exam;Trusted_Connection=True;TrustServerCertificate=True;";
```

### 3. Run Initial Migration

```bash
dotnet ef migrations add InitialCreate --project Exam.DataAccess
dotnet ef database update --project Exam.DataAccess
```

---

## How to Update Field Name (e.g., `PasswordHash` → `Password`)

Renamed the property in your `User` model:

```csharp
// Old
public string PasswordHash { get; set; }

// New
public string Password { get; set; }
```

Follow these steps:

```bash
dotnet ef migrations add RenamePasswordField --project Exam.DataAccess
dotnet ef database update --project Exam.DataAccess
```

EF will generate:

```csharp
migrationBuilder.RenameColumn(
    name: "PasswordHash",
    table: "Users",
    newName: "Password");
```

## Azure Devops setup

## Create .gitignore

# 1. Initialize Git repository
git init

# 2. Configure user info (local to this repo only)
git config user.name "Thirumeni"
git config user.email "Thirumeni.Mallieswaran@kaarinfotech.com"
(git config --list)

# 3. Add Azure DevOps remote
git remote add origin https://2025MayUser04@dev.azure.com/2025MayUser04/ExamPro/_git/ExamPro

# 4. Stage and commit all files
git add .
git commit -m "Initial commit"

# 5. Push to Azure DevOps (main branch)
git branch -M main
git push -u origin main


# Commit - 2

* 1. Create the Web
* 2. Authentication setup (JWT)
* 3. Password Hashing (Bcrypt)
* 4. File uploader 
* 5. Service - Response
* 6. Domain - Exceptions

## Dependencies installed

* Bcrypt.Net-Next
* Microsoft.AspNetCore.Authentication.JwtBearer
* Microsoft.AspNetCore.Http.Abstractions
* Microsoft.AspNetCore.Authentication.JwtBearer (Web only)

# Commit - 3
* User - Service and Repo

# Commit - 4
* Venue - Service and Repo

# Commit - 5
* Invigilator Availabilty - Service and Repo

# Commit - 6
* Allocation - Service and Repo

## Status
| **Status**    | **Who Can See?**    | **Action Allowed?**                       | **Meaning**                                 |
| ------------- | ------------------- | ----------------------------------------- | ------------------------------------------- |
| **Draft**     | Admin only          | Edit/Delete                               | Initial allocation suggestion               |
| **Approved**  | Admin only          | Edit/Delete                               | Admin confirmed draft but not yet published |
| **Published** | Invigilator & Admin | No edit (except swap within availability) | Final schedule sent out                     |

# Commit - 7 and 8
* Allocation and status update

# Commit - 9
* Login (Admin) setup

# Commit - 10
* User - Admin setup

# Commit - 11
* Venue setup
* New Migration

# Commit - 12
* Availability setup
* Both admin and Invigilator

# Commit - 13
* Allocation management
* Manage Allocations - Draft, Approved, Publish

# Commit - 14
* Notification setup
```bash
dotnet ef migrations add AddNotifications --project Exam.DataAccess --startup-project Exam.Web
dotnet ef database update --project Exam.DataAccess --startup-project Exam.Web
```

# Commit - 15
* Allocation delete

