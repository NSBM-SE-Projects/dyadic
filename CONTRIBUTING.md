# Contributing to Dyadic

Author: Dwain

This guide covers **Local Setup**, **Day-to-Day Git Branching Rules** and **PR Conventions**, read it before your first commit.

## Table of Contents
1. [Make Commands](#make-commands)
2. [First-time Setup](#first-time-setup)
3. [Running the app](#running-the-app)
4. [Git Workflow](#git-workflow)
5. [Commit Message Conventions](#commit-message-conventions)
6. [Pull Requests](#pull-requests)
7. [Testing](#testing)
8. [Project Structure](#project-structure)
9. [Still Stuck?](#still-stuck)


## Make Commands

This lets you use commands like:

```bash
make run
make test
make reset-db

# See This Command Below For All Commands
make help
```
So you don't have to type in long commands again...

- Mac/Linux - **make** is built-in
- Windows - needs installation:
    - via Chocolatey: ```choco install make```
    - via winget: ```winget install GnuWin32.Make```

## First-time Setup

```bash
# Git Setup
git clone https://github.com/NSBM-SE-Projects/dyadic.git
cd dyadic

# Dev Setup
make setup
```

This script:
- Checks **Pre-requisites**
- Starts **SQL Server**
- Configures your local **Secrets**
- Runs **Migrations** 

Note: 
- PLEASE READ THE CLI IF THERE ARE ANY ERROR PLEASE!

### Mac Members (Reminder)

- Open `docker-compose.yml` and switch to the `azure-sql-edge` image line (commented instruction is in the file)

## Running The App

```bash
make run

# To Check If DB Is Running
docker compose ps
```

Open the URL it prints (usually `https://localhost:5001` or `http://localhost:5000`).

Stop the DB container when you're done for the day:

```bash
make down
```

## Git Workflow

We use **GitHub Flow**: `main` should always be deployable, all work happens on short-lived feature branches merged via Pull Request.

### Branch Naming

`<type>/<name>-<short-description>` in kebab-case:

| Prefix | Use for |
|---|---|
| `feat/` | New functionality |
| `fix/` | Bug fix |
| `refactor/` | Restructure, no behavior change |
| `test/` | Tests only |
| `docs/` | Documentation |
| `chore/` | Config, tooling, deps |

Examples:
- `feat/thamindu-student-proposal-submission`
- `fix/dwain-blind-filter`
- `test/ashen-matching-service-unit-tests`
- `chore/yameesha-bump-xunit-version`

### Keep Branches Short

Aim to merge within 1–3 days. If the work is too big split between branches.

### Keep Latest Changes From `main`

```bash
  git checkout main
  git pull
  git checkout feat/your-branch
  git merge main
  ```

## Commit Message Conventions

We follow **Conventional Commits** — prefix + short summary.

```
<type>: <short summary in present tense>

<optional body explaining why, not what>
```

### Micro-commits Over Mega-commits

Ten commits of 20 lines each is better than one commit of 200 lines. Easier to review, easier to revert.

## Pull Requests

### Opening a PR

1. Push your branch:
```bash
git push -u origin feat/your-feature-name
```
2. Open a PR on GitHub → base: `main`, compare: your branch.
3. Fill out the PR template (summary, changes, testing notes).
4. Request a review from [@dwainXDL](https://github.com/dwainXDL).

### PR Requirements

- You need approval from [@dwainXDL](https://github.com/dwainXDL).
- All CI checks passing (once CI is set up)
- No unresolved review comments

## Testing

Run the full test suite:

```bash
make test
```

Run one project:

```bash
dotnet test tests/Dyadic.UnitTests
```

Collect coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Target:** >60% coverage (minimum), >80% preferred. Every PR that adds logic should add tests.

- **Unit Tests** → `tests/Dyadic.UnitTests` — mock dependencies with Moq, no DB, no HTTP
- **Integration tests** → `tests/Dyadic.IntegrationTests` — hit a real (or InMemory) DB, exercise the web
  pipeline

## Project Structure

```
.
├── docker-compose.yml          # Local SQL Server
├── .env.example                # Copy to .env (gitignored)
├── Dyadic.sln
├── src/
│   ├── Dyadic.Domain/          # Entities, enums, domain interfaces
│   ├── Dyadic.Application/     # Services, business rules (matching logic here)
│   ├── Dyadic.Infrastructure/  # DbContext, EF migrations, repositories
│   └── Dyadic.Web/             # ASP.NET Core Razor Pages — UI + controllers
└── tests/
    ├── Dyadic.UnitTests/
    └── Dyadic.IntegrationTests/
  ```

**Dependency Direction:** `Web → Infrastructure → Application → Domain`. Never reverse this — `Domain` depends on nothing.

## Still Stuck?
Contact [@dwainXDL](https://github.com/dwainXDL) with:
  - The full command you ran
  - The full error message
  - Output of `dotnet --version` and `docker compose ps`