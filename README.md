# Monivise

> Personal Financial Decision Engine — Spend Smarter, Not Just Less

Monivise is a real-time personal financial decision engine that answers one critical question before you spend: **"Can I afford this right now — and what happens if I do?"** Unlike traditional expense trackers, Monivise simulates every purchase against your actual financial capacity, accounting for fixed obligations, savings goals, and spending pace.

**Stack:** ASP.NET Core 9 Web API · C# 12 · Entity Framework Core · PostgreSQL · JWT Authentication · Next.js 15 · React 19 · TypeScript · Zustand · Tailwind CSS

**Core Concept:** Zero-based envelope budgeting with real-time risk assessment. Every spending decision is simulated before confirmation.

## System Philosophy

### The Three Financial Layers

Understanding this hierarchy is critical to all downstream logic:

| Layer | Definition | Includes |
|-------|-----------|----------|
| **Total Balance** | Sum of all money across every bucket | Fixed + Flexible + Savings |
| **Allocated Buckets** | Each income allocation by category | Per-bucket remaining amounts |
| **Spendable Money** | ONLY flexible bucket totals minus future obligations | Food + Transport + Fun + user-defined flexible |

> **Safe to Spend is derived entirely from Layer 3.** Layers 1 and 2 are informational. Only Layer 3 drives spending decisions.

### Core Principles

1. **All financial calculations are server-side.** Never in React components, never in API controllers. `FinancialCalculationService` is the single source of truth.
2. **Every displayed number is derived, not stored.** Components consume selectors; selectors consume the global store; the store reflects server state.
3. **No hardcoded financial rules.** All allocation percentages, bucket definitions, and risk thresholds are user-defined and database-persisted.
4. **Cycle-aware system.** A budget cycle has start date, end date, income, and carry-over rules. Daily and weekly views are always relative to the active cycle.
5. **Fixed obligations are reserved immediately.** Rent, bills, and other fixed expenses are treated as already spent from the moment income arrives — they never appear in Safe to Spend.

### The Decision Engine

When a user considers spending money, the engine runs a simulation and returns a structured risk assessment **before any money is deducted**:

1. User enters amount + selects bucket
2. Frontend calls `POST /api/decisions/simulate`
3. Engine computes impact on bucket, SafeToSpend, and DailyLimit
4. Evaluates risk level (Safe → Caution → Risky → Critical)
5. Checks Future Regret Signals (predictive warnings)
6. Full `SpendImpact` DTO returned to frontend in real time
7. User sees visual warning before confirming

---

## Local Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Node.js 20+](https://nodejs.org)
- [PostgreSQL 16](https://postgresql.org) (or use Railway in cloud)
- [VS Code](https://code.visualstudio.com/) + C# Dev Kit + Prettier + ESLint
- Git
- Postman (for API testing before frontend connection)

### Backend Setup

```bash
# Clone and build
git clone https://github.com/your-org/monivise.git
cd monivise
dotnet restore
dotnet build

# Create database
psql -U postgres -c "CREATE DATABASE monivise;"

# Configure secrets
cp Monivise.API/appsettings.Development.json.example Monivise.API/appsettings.Development.json
# Edit: set your PostgreSQL password and generate a 64-char JWT secret
# node -e "console.log(require('crypto').randomBytes(64).toString('hex'))"

# Run migrations
cd Monivise.API
dotnet ef migrations add InitialCreate --project ../Monivise.Infrastructure
dotnet ef database update --project ../Monivise.Infrastructure

# Start API
dotnet run
# API available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

### Frontend Setup

```bash
cd monivise-web
npm install

# Configure API URL
echo "NEXT_PUBLIC_API_URL=http://localhost:5000" > .env.local

# Start dev server
npm run dev
# Frontend available at http://localhost:3000
```

## Branch Strategy

We use **trunk-based development** with short-lived feature branches.

```
main
 └── develop          ← integration branch, always deployable
      ├── feature/issue-12-hero-card-component
      ├── feature/issue-18-decision-engine-risk-algorithm
      └── fix/issue-31-safe-to-spend-negative-edge-case
```

| Branch | Purpose | Who merges to it |
|--------|---------|-----------------|
| `main` | Production. Tagged releases only. | Merge from `develop` via PR after full review |
| `develop` | Integration. Must always build and pass tests. | Merge from feature branches via PR |
| `feature/*` | New work. One branch per issue. | Delete after PR is merged |
| `fix/*` | Bug fixes. | Delete after PR is merged |
| `chore/*` | Config, tooling, docs — no logic changes. | Delete after PR is merged |

**Rules:**
- Never commit directly to `main` or `develop`
- Branch names must include the issue number
- Branches must be deleted after merging — keep the repo clean
- Frontend and backend can share a branch only if the issue requires both

---

## Working on an Issue — Step by Step

### 1. Claim the issue

Assign it to yourself. Move the card from **Backlog** to **Up Next**.

```bash
gh issue edit <issue-number> --add-assignee @me
```

### 2. Sync your local develop branch

```bash
git checkout develop && git pull origin develop
```

### 3. Create your feature branch

```bash
git checkout -b feature/issue-<number>-<short-description>
# Example: git checkout -b feature/issue-12-hero-card-component
```

### 4. Move the card to In Progress

Let your teammate know if it touches shared files like `Program.cs` or `tailwind.config.ts`.

### 5. Do the work

Follow the task checklist in the issue body. Key rules:

- **No financial math in controllers or components.** All calculations go through `FinancialCalculationService`.
- **No raw EF queries in controllers.** All DB access goes through service classes.
- **No business logic in views or React components.** Views render ViewModels; components read from stores.
- **One concern per commit.** Don't bundle a feature and a refactor.
- **Build before committing.** Run `dotnet build` and `npm run build` before staging.

### 6. Run tests before committing

```bash
# Backend
dotnet test Monivise.Tests

# Frontend
cd monivise-web && npm run build && npm run lint
```

### 7. Stage and commit

See [Commit Message Convention](#commit-message-convention) below.

```bash
git add .
git commit -m "feat(dashboard): implement HeroCard with live SafeToSpend display

Resolves #12

- Added HeroCard component with gradient background and pace badge
- Integrated useDashboardStore for real-time data
- Added ProgressBar and StatCell sub-components
- Verified SafeToSpend calculation matches FinancialCalculationService output"
```

### 8. Push and open a pull request

```bash
git push origin feature/issue-12-hero-card-component
```

### 9. Respond to review

Address every comment. Do not dismiss without fixing or explaining. Re-request review when done.

### 10. After merge — clean up

```bash
git checkout develop && git pull origin develop
git branch -d feature/issue-12-hero-card-component
git push origin --delete feature/issue-12-hero-card-component
```

Move the issue card to **Done**. GitHub closes the issue automatically if you used `Resolves #<number>`.

---

## Commit Message Convention

We follow **Conventional Commits**. Every commit must follow this structure:

```
<type>(<scope>): <short summary>

<optional body>

<optional footer>
```

### Types

| Type | When to use |
|------|-------------|
| `feat` | A new feature or user-facing capability |
| `fix` | A bug fix |
| `refactor` | Code restructure — no behaviour change |
| `test` | Adding or updating tests |
| `chore` | Config files, tooling, dependencies, migrations |
| `docs` | Documentation only |
| `style` | Formatting, whitespace — zero logic change |
| `perf` | Performance improvement |

### Scopes

Use the area of the system the commit touches:

`auth` · `dashboard` · `buckets` · `income` · `expense` · `decision` · `weekly` · `history` · `calculation` · `api` · `db` · `config` · `ui` · `tests`

### Summary rules

- Lowercase, present tense, imperative mood: `add`, `fix`, `remove`, `update`
- No period at the end
- Maximum 72 characters on the first line
- Blank line between summary and body

### Examples

```bash
# Simple one-liner
git commit -m "feat(decision): implement risk evaluation algorithm"

# With body for a larger change
git commit -m "feat(calculation): add SafeToSpend and DailyLimit formulas

Resolves #8

- CalculateSafeToSpend: SUM(flexible remaining) - SUM(unpaid fixed)
- CalculateDailyLimit: (SafeToSpend / DaysRemaining) × PaceAdjustment
- PaceAdjustment: ≤0.8 → ×1.15, ≤1.0 → ×1.00, ≤1.2 → ×0.85, >1.2 → ×0.70
- Added 7 unit tests covering edge cases"

# Bug fix
git commit -m "fix(calculation): SafeToSpend no longer returns negative values

Resolves #15

Previously, when fixed obligations exceeded flexible remaining,
SafeToSpend returned negative numbers. Now clamped to 0 minimum
with Math.Max(0m, result)."

# Database migration
git commit -m "chore(db): add unique index on BucketBalance (BucketId + CycleId)

Prevents duplicate balance rows for the same bucket in the same cycle."

# Frontend component
git commit -m "feat(ui): add NumPad component for amount entry screens

- 3×4 grid with 000 key and backspace
- Prevents leading zeros and enforces max length
- Used in both income and decision flows"
```

### What NOT to do

```bash
# Too vague
git commit -m "fix stuff"
git commit -m "wip"
git commit -m "changes"

# Past tense
git commit -m "added HeroCard"      # wrong
git commit -m "add HeroCard"        # correct

# Bundled unrelated changes
git commit -m "feat(dashboard): add HeroCard and fix login bug"
```

---

## Pull Request Process

### Before opening a PR

- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet test` passes with zero failures
- [ ] `npm run build` passes (frontend)
- [ ] Branch is up to date with `develop`
- [ ] All checkboxes in the issue body are ticked
- [ ] No debug code, commented-out blocks, or `Console.WriteLine` left in
- [ ] No hardcoded credentials or connection strings
- [ ] JWT secret is NOT the placeholder value from setup

### Opening the PR

Use the template below. GitHub auto-populates it from `PULL_REQUEST_TEMPLATE.md`.

### PR title format

```
feat(dashboard): implement HeroCard with live SafeToSpend display (#12)
fix(calculation): clamp SafeToSpend to minimum 0 (#15)
chore(db): add BucketBalance unique index migration (#8)
```

### Labels to add

Every PR must have at least one label from each group:

**Type label** (pick one): `feat` · `fix` · `refactor` · `chore` · `docs`

**Layer label** (pick all that apply): `backend` · `frontend` · `database` · `auth` · `api`

**Phase label** (pick one): `phase-1` · `phase-2`

### Reviewers

- Every PR requires at least **one approving review** before merging
- Assign your teammate as reviewer when you open the PR
- Tag `@teammate` if it touches a file they are working on

### Merging

- Use **Squash and merge** for feature branches
- The squash commit message must follow the commit convention
- Delete the branch after merge

### Draft PRs

For early feedback before work is complete:

```bash
gh pr create --draft --title "feat(decision): implement risk algorithm" --body "Work in progress"
```

---

## Code Review Guidelines

### For the reviewer

- Review within **24 hours** of being assigned
- Be specific: "This query will produce N+1 requests — add `.ThenInclude`" not "This looks wrong"
- Use these prefixes:
  - **`[blocking]`** — must fix before merging
  - **`[suggestion]`** — optional improvement
  - **`[nit]`** — minor style preference, won't block

### For the author

- Respond to every comment — fix it or explain why not
- Don't resolve reviewer comments yourself — let the reviewer verify
- If a comment reveals misunderstanding, explain the intended design

### Things reviewers always check for Monivise

- Is there any financial math in a controller or React component? (must be in `FinancialCalculationService`)
- Is there any EF query in a controller? (must be in service)
- Does `CalculateSafeToSpend` handle the edge case where fixed > flexible? (must return 0, not negative)
- Are all new `decimal` properties configured with `HasPrecision(18, 2)`?
- Does every new POST action have `[ValidateAntiForgeryToken]`? (backend) or proper input validation? (frontend)
- Are new enum values stored as strings (not integers) in the DB?
- Does the frontend show `--` when `dailyLimit === 0` instead of `NaN`?
- Are all currency values formatted with `formatNGN()` consistently?


## Definition of Done

An issue is **Done** when ALL of the following are true:

- [ ] All task checkboxes in the issue body are ticked
- [ ] All acceptance criteria in the issue body are met
- [ ] `dotnet build` passes with zero warnings and zero errors
- [ ] `dotnet test` passes with zero failures
- [ ] `npm run build` passes (if frontend changes)
- [ ] The PR has been reviewed and approved by at least one teammate
- [ ] The PR has been squash-merged into `develop`
- [ ] The feature branch has been deleted
- [ ] The issue is closed on GitHub
- [ ] The card is in the **Done** column on the Project board
- [ ] **Financial calculations verified:** SafeToSpend, DailyLimit, and RiskLevel match expected values for test scenarios

If any of these are not true, the issue is not done — move the card back.

---

## Architecture Rules

These rules are non-negotiable. A PR that violates them will not be merged.

### Rule 1 — Domain is dependency-free

`Monivise.Domain.csproj` may only reference base .NET packages. No Entity Framework, no ASP.NET, no third-party libraries. If you need EF in Domain, you have the wrong layer.

### Rule 2 — No financial math outside FinancialCalculationService

Controllers and React components call services. `FinancialCalculationService` contains ALL financial math. A controller or component that calculates `safeToSpend` directly is a critical bug.

### Rule 3 — No EF queries in controllers

Controllers call services. Services query the database. The only exception is `Program.cs` for DI registration.

### Rule 4 — No business logic in views or React components

Views and components render data. Computed properties belong in ViewModels, selectors, or services.

### Rule 5 — All secrets via configuration

No API keys, passwords, or connection strings in source code. Use `appsettings.Development.json` locally (gitignored) and environment variables in production.

### Rule 6 — Use enums, never magic strings

```csharp
// Wrong
[Authorize(Roles = "Admin")]

// Correct
[Authorize(Roles = Roles.Admin)]
```

### Rule 7 — Every POST action has validation

Backend: `[ValidateAntiForgeryToken]` on every POST. Frontend: Input validation before API calls.

### Rule 8 — Currency formatting is centralized

All currency displays use `formatNGN()`. Never inline `toLocaleString` or manual `₦` concatenation.

### Rule 9 — Future days never show fabricated data

Weekly view: future days show empty bars + budget line only. Never simulate or estimate future spending.

---


## Useful Commands

### Backend

```bash
# Build entire solution
dotnet build

# Run tests
dotnet test Monivise.Tests

# Run with hot reload
dotnet watch run --project Monivise.API

# Add migration
dotnet ef migrations add <Name> --project Monivise.Infrastructure --startup-project Monivise.API

# Apply migration
dotnet ef database update --project Monivise.Infrastructure --startup-project Monivise.API

# Generate JWT secret
node -e "console.log(require('crypto').randomBytes(64).toString('hex'))"
```

### Frontend

```bash
# Install dependencies
npm install

# Dev server
npm run dev

# Production build
npm run build

# Lint
npm run lint
```

### Git workflow

```bash
# Sync and branch
git checkout develop && git pull origin develop
git checkout -b feature/issue-<number>-<description>

# Commit and push
git add . && git commit -m "feat(scope): summary"
git push origin HEAD

# PR and review
gh pr create --fill
gh pr review <number> --approve

# Cleanup after merge
git checkout develop && git pull && git branch -d <branch>
```

<div align="center">
  <sub>Monivise · Spend with Confidence · Built with Precision</sub>
</div>
