<div align="center">

```
███╗   ███╗ ██████╗ ███╗   ██╗██╗██╗   ██╗██╗███████╗███████╗
████╗ ████║██╔═══██╗████╗  ██║██║██║   ██║██║██╔════╝██╔════╝
██╔████╔██║██║   ██║██╔██╗ ██║██║██║   ██║██║███████╗█████╗
██║╚██╔╝██║██║   ██║██║╚██╗██║██║╚██╗ ██╔╝██║╚════██║██╔══╝
██║ ╚═╝ ██║╚██████╔╝██║ ╚████║██║ ╚████╔╝ ██║███████║███████╗
╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═══╝  ╚═╝╚══════╝╚══════╝
```

**Personal Financial Decision Engine**

_Spend with Confidence. Not Just Less._

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=flat-square&logo=postgresql)](https://postgresql.org)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen?style=flat-square)](CONTRIBUTING.md)

</div>

---

## What Is Monivise?

**Monivise is not a budgeting app.**

It is a real-time personal financial decision engine built around a single question that traditional finance apps refuse to answer:

> **"Can I afford this right now — and what happens if I do?"**

Most budgeting apps track what already happened. Monivise simulates what _will_ happen — before you spend. Every purchase decision runs through a consequence engine that computes risk, shows post-spend projections, and tells you in real time whether a transaction is Safe, Cautious, Risky, or Critical given your actual financial position at that moment.

The number that matters is not your bank balance. It is not your monthly income. It is your **Safe to Spend** — the real discretionary amount available after fixed obligations are reserved and savings are protected.

---

## Tech Stack

| Layer             | Technology                                       |
| ----------------- | ------------------------------------------------ | --- | ---- | ----------------- |
| Backend API       | ASP.NET Core 9 Web API · C# 12                   |
| ORM & Persistence | Entity Framework Core · PostgreSQL 16            |
| Authentication    | JWT Bearer Tokens · BCrypt                       |
| Frontend          | Blazor WebAssembly (.NET 9) · MudBlazor · Fluxor |
| State Management  | Fluxor (Redux pattern for Blazor)                |
| Styling           | CSS Custom Properties · MudBlazor Theme System   |
| Testing           | xUnit · FluentAssertions · Moq · bUnit           |     | Docs | Swagger / OpenAPI |

---

## Core Concepts

### The Three Financial Layers

```
┌─────────────────────────────────────────────┐
│  Layer 1 — Total Balance                    │
│  Fixed + Flexible + Savings (informational) │
├─────────────────────────────────────────────┤
│  Layer 2 — Allocated Buckets                │
│  Per-category remaining amounts             │
├─────────────────────────────────────────────┤
│  Layer 3 — Safe to Spend ← THE NUMBER      │
│  Flexible remaining MINUS fixed obligations │
│  Savings excluded entirely                  │
└─────────────────────────────────────────────┘
```

Only Layer 3 drives spending decisions. Layers 1 and 2 are informational.

### Bucket Types

| Type         | Behaviour                                | Safe to Spend Impact                                                      |
| ------------ | ---------------------------------------- | ------------------------------------------------------------------------- |
| **Fixed**    | Non-negotiable obligations (rent, bills) | Balance is **deducted** from Safe to Spend — treated as already owed      |
| **Flexible** | Everyday discretionary spending          | Balance **contributes** to Safe to Spend                                  |
| **Savings**  | Protected growth                         | **Completely excluded** — never visible to Safe to Spend in any direction |

### The Decision Engine

When a user considers a spend, the engine runs a full simulation before any money moves:

```
User enters amount
      ↓
POST /api/decisions/simulate
      ↓
Engine computes: bucket impact · SafeToSpend delta · DailyLimit after · depletion %
      ↓
Risk evaluated: Safe → Caution → Risky → Critical
      ↓
Future Regret Signals checked (predictive warnings)
      ↓
Full SpendImpact DTO returned in real time
      ↓
User sees consequence — then decides
```

---

## Architecture Overview

```
Monivise/
├── src/
│   ├── Monivise.Domain/            # Pure C# — zero external dependencies
│   │   ├── Entities/               # User, BudgetCycle, Bucket, Transaction, AuditLog
│   │   ├── Enums/                  # BucketType, CycleStatus, RiskLevel
│   │   └── Exceptions/             # DomainException hierarchy
│   │
│   ├── Monivise.Application/       # Use cases, interfaces, DTOs, services
│   │   ├── Interfaces/             # IFinancialCalculationService (and all others)
│   │   ├── Services/               # FinancialCalculationService ← THE HEART
│   │   └── DTOs/                   # All request/response contracts
│   │
│   ├── Monivise.Infrastructure/    # EF Core, PostgreSQL, JWT, repositories
│   │   └── Data/                   # AppDbContext, configurations, migrations
│   │
│   └── Monivise.API/               # ASP.NET Core Web API — thin controllers only
│       ├── Controllers/
│       └── Middleware/             # ExceptionMiddleware (DomainException → 422)
│
├── tests/
│   └── Monivise.UnitTests/         # Financial logic tests — xUnit
│
└── Monivise.App/                   # Blazor WebAssembly frontend
    ├── wwwroot/                    # index.html, CSS custom properties, fonts
    ├── Pages/                      # Routable pages (@page directive)
    ├── Components/                 # Reusable Blazor components
    │   ├── Atoms/                  # GlowButton, Chip, ProgressBar, NumPad
    │   ├── Cards/                  # BucketCard, TransactionCard
    │   ├── Dashboard/              # SafeToSpendBanner, PaceIndicator
    │   ├── Simulator/              # ConsequencePanel, RiskBadge
    │   ├── Charts/                 # WeeklyBarChart, BucketDonut
    │   └── Navigation/             # BottomNav
    ├── Layouts/                    # AppShell, MainLayout, AuthLayout
    ├── Services/                   # Typed HTTP client wrappers, business services
    ├── API/                        # ApiClient base, typed API clients
    ├── Authentication/             # JwtAuthStateProvider, TokenStore, RefreshTokenHandler
    ├── State/                      # Fluxor — AppState, Actions, Reducers, Effects
    ├── DTOs/                       # Data Transfer Objects (mirror API contracts)
    ├── Themes/                     # MoniTheme.cs, ThemeTokens.cs
    └── Utilities/                  # CurrencyFormatter, DateHelpers, ValidationHelpers
```

**Dependency direction is strict and one-way:**

```
Domain ← Application ← Infrastructure ← API
  ↑____________________________________________|
                     Tests
```

Domain never references Application, Infrastructure, or API. If you are adding an EF or ASP.NET `using` inside Domain — stop. That belongs in another layer.

---

## Local Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [PostgreSQL 16](https://postgresql.org)
- [Git](https://git-scm.com)
- Postman (API verification before frontend connection)

### Backend Setup

```bash
# 1. Clone
git clone https://github.com/0xCiphera/monivise.git
cd monivise

# 2. Restore and build
dotnet restore && dotnet build

# 3. Create the database
psql -U postgres -c "CREATE DATABASE monivise_dev;"

# 4. Configure secrets (never commit this file)
cp src/Monivise.API/appsettings.Development.example.json \
   src/Monivise.API/appsettings.Development.json
# Edit: set PostgreSQL password + generate a JWT secret:
node -e "console.log(require('crypto').randomBytes(64).toString('hex'))"

# 5. Run migrations
dotnet ef migrations add InitialCreate \
  --project src/Monivise.Infrastructure \
  --startup-project src/Monivise.API

dotnet ef database update \
  --project src/Monivise.Infrastructure \
  --startup-project src/Monivise.API

# 6. Start API
dotnet run --project src/Monivise.API
# API:     http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend Setup

```bash
# 1. Navigate to Blazor project
cd src/Monivise.App

# 2. Restore packages
dotnet restore

# 3. Configure API base URL
echo '{"ApiBaseUrl":"http://localhost:5000"}' > wwwroot/appsettings.Development.json

# 4. Run the Blazor WASM app
dotnet run
# Frontend: http://localhost:5259 (or as shown in console)

```

### Verify Backend (Postman Sequence)

Run these 9 requests in order. Every one must succeed before starting frontend work.

| #   | Request                          | Body                                                                                               | Expected                               |
| --- | -------------------------------- | -------------------------------------------------------------------------------------------------- | -------------------------------------- |
| 1   | `POST /api/auth/register`        | `{ "email": "me@test.com", "password": "Test123!", "displayName": "Tobiloba", "currency": "NGN" }` | 201 + JWT                              |
| 2   | `POST /api/auth/login`           | `{ "email": "me@test.com", "password": "Test123!" }`                                               | 200 + JWT                              |
| 3   | `POST /api/buckets`              | `{ "name": "Food", "icon": "🍱", "allocationPercent": 25, "type": "Flexible" }`                    | 201                                    |
| 4   | `POST /api/buckets` ×4           | Rent(35,Fixed) · Transport(15,Flexible) · Savings(15,Savings) · Fun(10,Flexible)                   | All 201, sum=100                       |
| 5   | `POST /api/transactions/income`  | `{ "amount": 200000, "source": "Salary" }`                                                         | 201 + allocation breakdown             |
| 6   | `GET /api/dashboard`             | Bearer token                                                                                       | 200 + safeToSpend, dailyLimit, buckets |
| 7   | `POST /api/decision/simulate`    | `{ "bucketId": "<food-id>", "amount": 5000 }`                                                      | 200 + SpendImpact + riskLevel          |
| 8   | `POST /api/transactions/expense` | `{ "bucketId": "<food-id>", "amount": 5000, "note": "Groceries" }`                                 | 201                                    |
| 9   | `GET /api/dashboard`             | Bearer token                                                                                       | safeToSpend shows ₦5,000 less          |

---

## First-Run Walkthrough

Follow this sequence on a fresh database to verify the full system works end-to-end.

| Step | Action                                    | What to Verify                                                       |
| ---- | ----------------------------------------- | -------------------------------------------------------------------- |
| 1    | Register via `/api/auth/register`         | JWT returned, user created                                           |
| 2    | Login — dashboard shows empty state       | Buckets list empty                                                   |
| 3    | Create 6 buckets summing to exactly 100%  | System accepts the set                                               |
| 4    | Add ₦200,000 income                       | Preview matches actual allocation                                    |
| 5    | Check Safe to Spend                       | Must equal: Flexible remaining MINUS unpaid Fixed. Savings excluded. |
| 6    | Simulate a spend exceeding bucket balance | Risk shows Critical                                                  |
| 7    | Confirm a normal spend                    | Dashboard refreshes, SafeToSpend decreases by exact amount           |
| 8    | Check weekly view                         | Bars only for today and past — never future days                     |
| 9    | Check history                             | All transactions newest first with correct icons and amounts         |

---

## Financial Engine Rules

These rules are the core of the product. Any code that violates them is a critical bug.

### Rule 1 — Safe to Spend Formula (Immutable)

```
SafeToSpend = SUM(Flexible bucket remaining) − SUM(unpaid Fixed obligations)
```

- Savings buckets are **EXCLUDED ENTIRELY**
- Fixed obligations already paid do NOT reduce SafeToSpend
- Result is **clamped to 0** — never show a negative number to the user
- This formula lives in `FinancialCalculationService` and **nowhere else**

### Rule 2 — Daily Spend Limit

```
DailyLimit = (SafeToSpend / DaysRemaining) × PaceAdjustment

PaceAdjustment:
  pace ≤ 0.8  →  × 1.15  (ahead of schedule — be generous)
  pace ≤ 1.0  →  × 1.00  (on track)
  pace ≤ 1.2  →  × 0.85  (slightly over — tighten)
  pace >  1.2  →  × 0.70  (significantly over — restrict)
```

- `DaysRemaining` uses `Math.Max(1, ...)` — never divide by zero
- Display `--` in UI when `dailyLimit === 0` (no income recorded yet)

### Rule 3 — Risk Level (Evaluated in Priority Order)

```
Critical  →  PostSafe < 0  OR  Depletion > 100%
Risky     →  PostDaily < AvgDaily × 0.5  OR  Depletion > 85%
Caution   →  PostDaily < AvgDaily × 0.8  OR  Depletion > 70%
Safe      →  otherwise
```

Critical is always checked first and overrides all other levels.

### Rule 4 — Income Allocation

```
// Highest percentage first. Last bucket absorbs rounding remainder.
for each bucket (ordered by AllocationPercent DESC):
  if last:  amount = income − totalSoFar
  else:     amount = Round(income × (percent / 100), 2)
  totalSoFar += amount
```

### Rule 5 — Weekly View Integrity

- Future days show **no bar** — only the daily budget reference line
- **Never fabricate spending data** for dates that have not occurred
- Past and today display actuals only

---

## Architecture Rules

These are enforced at code review. A PR that violates any of them will not be merged.

| Rule                                                        | Description                                                                                          |
| ----------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **No financial math outside `FinancialCalculationService`** | Controllers, components, and DTOs never calculate SafeToSpend, DailyLimit, or RiskLevel              |
| **Domain has zero framework dependencies**                  | `Monivise.Domain.csproj` references only base .NET packages — no EF, no ASP.NET                      |
| **No EF queries in controllers**                            | Controllers call services. Services query the database.                                              |
| **No business logic in Blazor components**                  | Components read from Fluxor stores and render. All derivation happens in services or state reducers. |
| **JWT access token in-memory only**                         | TokenStore singleton holds the token. Never persist to localStorage or sessionStorage.               |
| **Components call services, not API clients**               | Services orchestrate between API clients and Fluxor state. Keeps components clean.                   |
| **All secrets via configuration**                           | No API keys, passwords, or connection strings in source code                                         |
| **Use enums, never magic strings**                          | `BucketType.Fixed` not `"Fixed"` in business logic                                                   |
| **Every POST has validation**                               | Backend: `[ValidateAntiForgeryToken]` + DTO annotations. Frontend: validate before calling API.      |
| **Currency formatting is centralised**                      | Use `formatCurrency(amount, currency)` — never inline `₦` concatenation                              |
| **Soft delete only**                                        | Buckets and transactions are never hard-deleted. `IsActive = false` preserves history.               |

---

## Branch Strategy

Trunk-based development with short-lived feature branches off `develop`.

```
main
 └── develop                          ← integration branch, always deployable
      ├── feature/issue-12-hero-card
      ├── feature/issue-18-decision-engine-risk
      ├── fix/issue-31-safe-to-spend-negative
      └── chore/issue-05-setup-eslint
```

| Branch      | Purpose                                        | Merge target                 |
| ----------- | ---------------------------------------------- | ---------------------------- |
| `main`      | Production — tagged releases only              | From `develop` via PR        |
| `develop`   | Integration — must always build and pass tests | From feature branches via PR |
| `feature/*` | New features — one branch per issue            | `develop`                    |
| `fix/*`     | Bug fixes                                      | `develop`                    |
| `chore/*`   | Config, tooling, docs, migrations              | `develop`                    |

**Naming convention:** `<type>/issue-<number>-<short-description>`

```bash
feature/issue-12-hero-card-component
fix/issue-31-safe-to-spend-negative-edge-case
chore/issue-05-configure-eslint-and-prettier
```

**Rules:**

- Never commit directly to `main` or `develop`
- Branch name must contain the issue number
- Delete branches after merging — keep the repository clean
- WIP limit: maximum 2 branches active per person at a time

---

## Commit Convention

Monivise follows [Conventional Commits](https://www.conventionalcommits.org/).

### Format

```
<type>(<scope>): <summary>

<body — optional>

<footer — optional>
```

### Types

| Type       | Use when                                   |
| ---------- | ------------------------------------------ |
| `feat`     | New feature or user-facing capability      |
| `fix`      | Bug fix                                    |
| `refactor` | Code restructure with no behaviour change  |
| `test`     | Adding or updating tests                   |
| `chore`    | Config, tooling, dependencies, migrations  |
| `docs`     | Documentation only                         |
| `style`    | Formatting, whitespace — zero logic change |
| `perf`     | Performance improvement                    |

### Scopes

`auth` · `dashboard` · `buckets` · `income` · `expense` · `decision` · `weekly` · `history` · `calculation` · `api` · `db` · `config` · `ui` · `tests` · `blazor` · `fluxor` · `mudblazor`

### Summary Rules

- Lowercase, present tense, imperative mood
- No period at the end
- Maximum 72 characters on the first line
- Blank line between summary and body

### Examples

```bash
# Simple
git commit -m "feat(decision): implement risk evaluation algorithm"

# With body
git commit -m "feat(calculation): implement SafeToSpend and DailyLimit formulas

Resolves #8

- GetSafeToSpend: SUM(flex remaining) - SUM(unpaid fixed). Savings excluded.
- GetDailyLimit: (SafeToSpend / DaysRemaining) × PaceAdjustment
- Pace adjustment: ≤0.8→×1.15, ≤1.0→×1.00, ≤1.2→×0.85, >1.2→×0.70
- Added 6 unit tests covering edge cases including negative clamping"

# Bug fix
git commit -m "fix(calculation): clamp SafeToSpend to minimum 0

Resolves #15

When fixed obligations exceeded flexible remaining, SafeToSpend
returned a negative value. Added Math.Max(0m, result) clamp."

# Migration
git commit -m "chore(db): add unique index on BucketBalance (BucketId + CycleId)

Prevents duplicate balance rows for the same bucket in the same cycle."

# Frontend component
git commit -m "feat(ui): add NumPad component for amount entry screens

- 3×4 grid with 000 key and backspace
- Prevents leading zeros, enforces 7-digit maximum
- Used in both income and decision flows"
```

### What NOT to Do

```bash
git commit -m "fix stuff"          # too vague
git commit -m "wip"                # not a commit
git commit -m "changes"            # meaningless
git commit -m "added HeroCard"     # past tense
git commit -m "feat(dashboard): add HeroCard and fix login bug"  # bundled
```

---

## Environment Variables

### Backend (`appsettings.Development.json` — gitignored)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=monivise_dev;Username=postgres;Password=<your-password>"
  },
  "Jwt": {
    "Key": "<64-char-random-hex>",
    "Issuer": "monivise-api",
    "Audience": "monivise-client"
  }
}
```

### Backend (Production — environment variables only)

| Variable                               | Description                                 |
| -------------------------------------- | ------------------------------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string (SSL required) |
| `Jwt__Key`                             | 64-character cryptographically random key   |
| `Jwt__Issuer`                          | `monivise-api`                              |
| `Jwt__Audience`                        | `monivise-client`                           |
| `ASPNETCORE_ENVIRONMENT`               | `Production`                                |
| `ASPNETCORE_URLS`                      | `http://+:8080`                             |

### Frontend

| Variable     | Description                                              |
| ------------ | -------------------------------------------------------- |
| `ApiBaseUrl` | Backend API base URL (set in `wwwroot/appsettings.json`) |

---

## Deployment Guide

### Backend — Railway

```
1. Push solution to GitHub
2. railway.app → New Project → Deploy from GitHub → select Monivise.API
3. Add PostgreSQL plugin → copy DATABASE_URL
4. Set environment variables in Railway dashboard
5. Railway auto-detects .NET and builds
6. Run migrations: Railway console → dotnet ef database update
7. Note your Railway URL (e.g. monivise-api.railway.app)
```

### Frontend — Azure Static Web Apps

```
Push monivise-web to GitHub
Create Azure Static Web App → Deploy from GitHub → select monivise-web
Build configuration: output location = wwwroot
Add app setting: ApiBaseUrl=https://monivise-api.railway.app
Deploy
Update Railway's CORS allowed origins to your Azure URL
```

### Pre-Launch Checklist

- [ ] Register a real account and log in from the deployed URL
- [ ] Create 6 buckets summing to exactly 100%
- [ ] Add income — allocation preview matches actual server allocation
- [ ] Safe to Spend = Flexible remaining MINUS unpaid Fixed (Savings excluded)
- [ ] Simulate a spend exceeding bucket balance — Risk shows Critical
- [ ] Confirm a spend — dashboard refreshes, SafeToSpend decreases by exact amount
- [ ] Weekly view: bars for today and past only, never future
- [ ] History shows all transactions newest first with correct icons and amounts
- [ ] JWT key is NOT the placeholder value from setup
- [ ] No CORS errors in browser console
- [ ] Swagger disabled or protected in production

---

## Useful Commands

```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test tests/Monivise.UnitTests

# Run with hot reload
dotnet watch run --project src/Monivise.API

# Add migration
dotnet ef migrations add <Name> \
  --project src/Monivise.Infrastructure \
  --startup-project src/Monivise.API

# Apply migration
dotnet ef database update \
  --project src/Monivise.Infrastructure \
  --startup-project src/Monivise.API

# Generate a cryptographically secure JWT key
node -e "console.log(require('crypto').randomBytes(64).toString('hex'))"

# Frontend dev server
cd monivise-web && dotnet watch run

# Frontend production build
cd monivise-web && dotnet publish -c Release -o ./dist

# Create a branch and push
git checkout develop && git pull origin develop
git checkout -b feature/issue-<number>-<description>
git push origin HEAD

# Open a PR from CLI
gh pr create --fill

# Clean up after merge
git checkout develop && git pull
git branch -d feature/issue-<number>-<description>
git push origin --delete feature/issue-<number>-<description>
```

---

## API Endpoint Reference

### Auth

| Method | Endpoint             | Auth | Description               |
| ------ | -------------------- | ---- | ------------------------- |
| POST   | `/api/auth/register` | No   | Create account            |
| POST   | `/api/auth/login`    | No   | Authenticate, receive JWT |

### Dashboard

| Method | Endpoint         | Description                                                   |
| ------ | ---------------- | ------------------------------------------------------------- |
| GET    | `/api/dashboard` | SafeToSpend, DailyLimit, Pace, all buckets with live balances |

### Budget Cycles

| Method | Endpoint             | Description                           |
| ------ | -------------------- | ------------------------------------- |
| POST   | `/api/cycles/start`  | Create active cycle for current month |
| GET    | `/api/cycles/active` | Get active cycle details              |

### Buckets

| Method | Endpoint            | Description                                    |
| ------ | ------------------- | ---------------------------------------------- |
| GET    | `/api/buckets`      | List all active buckets with live balance data |
| POST   | `/api/buckets`      | Create bucket                                  |
| PUT    | `/api/buckets/{id}` | Update name, icon, %, type                     |
| DELETE | `/api/buckets/{id}` | Soft-delete (IsActive = false)                 |

### Transactions

| Method | Endpoint                          | Description                                     |
| ------ | --------------------------------- | ----------------------------------------------- |
| POST   | `/api/transactions/income`        | Record income, auto-allocate across all buckets |
| POST   | `/api/transactions/expense`       | Record expense against a specific bucket        |
| GET    | `/api/transactions/current-cycle` | All transactions for active cycle               |

### Decision Engine

| Method | Endpoint                 | Description                                                     |
| ------ | ------------------------ | --------------------------------------------------------------- |
| POST   | `/api/decision/simulate` | Run consequence simulation — returns SpendImpact, no data saved |

### Weekly Projection

| Method | Endpoint                | Description                            |
| ------ | ----------------------- | -------------------------------------- |
| GET    | `/api/dashboard/weekly` | 7-day projection based on current pace |

---

<div align="center">
  <sub>Monivise · Personal Financial Decision Engine · Built with precision</sub>
</div>
