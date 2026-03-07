# Personal Finance Tracker

> A structured C# console application for tracking personal income and expenses, built with a layered architecture and CSV persistence.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Language](https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=csharp)
![Status](https://img.shields.io/badge/Status-v1%20Console-blue?style=flat-square)
![Architecture](https://img.shields.io/badge/Architecture-Layered-orange?style=flat-square)

---

## Overview

Personal Finance Tracker is a C# console application that lets users log, categorize, edit, and summarize financial transactions. Data is persisted locally via CSV files. The project follows a layered architecture pattern — separating models, business logic, and data access — laying the groundwork for a future REST API version.

---

## Architecture

```
PersonalFinanceTracker/
├── Models/
│   ├── Transaction.cs          # Core domain model with factory methods
│   ├── FinanceSummary.cs       # Summary aggregate (income, expense, balance)
│   └── Enums.cs                # TransactionType, ExpenseCategory, IncomeCategory
├── Services/
│   └── FinanceService.cs       # Business logic: add, edit, delete, summarize
├── Repositories/
│   └── CsvTransactionRepository.cs  # Data access: CSV read/write
├── tests/                      # Unit test project
├── Program.cs                  # Console UI + menu loop
└── PersonalFinanceTracker.sln
```

### Layer Responsibilities

| Layer | Class | Responsibility |
|---|---|---|
| **Model** | `Transaction` | Represents a single financial record |
| **Model** | `FinanceSummary` | Aggregated totals and category breakdowns |
| **Service** | `FinanceService` | All business rules — CRUD, validation, summary logic |
| **Repository** | `CsvTransactionRepository` | File I/O — load and save transactions to CSV |
| **Entry Point** | `Program` | Menu loop, user input handling, display formatting |

---

## Design Decisions

**Factory Methods on Transaction**
Instead of exposing a public constructor with nullable category fields, `Transaction` exposes `CreateExpense()` and `CreateIncome()` static factory methods. This makes the intent explicit at the call site and prevents invalid states (e.g. an expense with an income category).

```csharp
// Clear intent, no ambiguity
var t = Transaction.CreateExpense(id, date, amount, category, description);
```

**Strongly-typed Categories via Enums**
Categories are defined as enums (`ExpenseCategory`, `IncomeCategory`) rather than free-text strings. This prevents typos, enables exhaustive switch handling, and makes the codebase easier to extend.

**Repository Pattern**
Data access is isolated behind `CsvTransactionRepository`. The service layer depends on the repository interface — not on file I/O directly. This means swapping CSV for a database (e.g. MySQL + Entity Framework) in v2 requires changing only the repository, not the service or UI.

**Dependency Injection (manual)**
`FinanceService` receives its repository via constructor injection. This decouples the service from the data source and makes unit testing straightforward.

```csharp
_repository = new CsvTransactionRepository();
_service = new FinanceService(_repository);  // injected — not newed inside
```

---

## Features

- Add income and expense transactions with date, amount, category, and description
- Edit existing transactions — update any field individually
- Delete transactions by ID
- View all transactions in a formatted table
- View financial summary — total income, total expense, balance, breakdown by category
- Save and load transactions to/from CSV files
- Input validation on all fields — dates, amounts, menu options, file names

---

## Tech Stack

| Technology | Usage |
|---|---|
| C# / .NET 8 | Core language and runtime |
| LINQ | Transaction filtering and summary aggregation |
| File I/O (CSV) | Local data persistence |
| Enums | Strongly-typed transaction categories |

---

## How To Run

```bash
# 1. Clone the repository
git clone https://github.com/Nzo-Cloud/personal-finance-tracker.git

# 2. Navigate to project directory
cd personal-finance-tracker

# 3. Run the application
dotnet run
```

> Requires [.NET 8 SDK](https://dotnet.microsoft.com/download)

---

## Roadmap

This console app is **v1** — a foundation for a production-ready REST API.

| Version | Description | Status |
|---|---|---|
| v1 | Console app with CSV persistence | ✅ Complete |
| v2 | REST API — ASP.NET Core + MySQL | 🔨 Planned |
| v2+ | JWT authentication, deployment | 📋 Backlog |

The v2 API will reuse the same domain models and business logic from this project, replacing the CSV repository with Entity Framework + MySQL and exposing endpoints via ASP.NET Core controllers.

---

## Author

**Lorenzo Balitian**
- GitHub: [@Nzo-Cloud](https://github.com/Nzo-Cloud)

---

> *v1 — Console Application. Built as a learning and portfolio project.*
