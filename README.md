# Fitness Tracking

> A strength training journal for logging workouts, tracking progress, and improving over time.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18+-61DAFB?style=flat&logo=react&logoColor=black)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

---

## Vision

Fitness Tracking helps users:
- Log workouts with sets, reps, and weight.
- Review previous sessions and profile information.
- Track consistency and progress through real usage data.

---

## Tech Stack

### Language Breakdown (GitHub Linguist)

- `57.7%` C#
- `30.7%` TypeScript
- `8.0%` CSS
- Remaining percentage: SQL, JavaScript, and workflow/configuration files

### Platform

- **Backend:** ASP.NET Core (`.NET 10`)
- **Frontend:** React + Vite + TypeScript
- **Database:** PostgreSQL
- **Containerization:** Docker + Docker Compose


---

## Architecture

Three-tier setup:

1. **Presentation tier:** React + Vite frontend
2. **Application tier:** ASP.NET Core backend (Clean Architecture inspired)
3. **Data tier:** PostgreSQL database

---

## Staging Server

- **URL:** http://89.150.149.43:8030/
- **Branch flow:** Updated automatically on every push to `main`
- **Pipeline:** `.github/workflows/workflow.yml`
- **What happens:** build + tests + analysis + image publish + deployment + migration + verification

This environment is used as the continuously deployed validation environment for the latest `main` changes.

---

## Production Server

- **URL:** http://46.62.146.222:8030/
- **Branch flow:** Updated only through a pull request from `main` to `Production`
- **Pipeline:** `.github/workflows/production_workflow.yml`
- **Guardrail:** Workflow validates that the source branch is exactly `main`

This keeps production releases intentional and controlled, with explicit promotion from staging-ready code.

---

## Automation & CI/CD

GitHub Actions drives CI, CD, and repository automation.

### CI + Staging CD (`workflow.yml`)

- **Trigger:** Push to `main`
- **Core checks:**
  - Semantic version generation
  - .NET build and unit tests with coverage
  - Frontend unit tests with coverage
  - Sonar analysis with quality gate wait
  - Mutation testing with Stryker
- **Delivery:**
  - Build and push backend/frontend Docker images to GHCR (`:staging` tags)
  - Deploy to staging host over SSH
  - Run Flyway validate/repair (if checksum mismatch)/migrate
  - Health checks for backend and frontend
  - Run k6 performance test and TestCafe E2E
  - Upload artifacts (mutation, k6, E2E, test diagnostics)

### Production CD (`production_workflow.yml`)

- **Trigger:** Pull request events targeting `Production` (`opened`, `synchronize`, `reopened`)
- **Branch policy in pipeline:** rejects PRs unless source branch is `main`
- **Core checks:** Same quality gates as staging flow (build, tests, coverage, Sonar, mutation)
- **Delivery:**
  - Build and push Docker images with `:production` tags
  - Deploy with `docker-compose.yml` to production host
  - Run Flyway validation and migrations
  - Verify service readiness
  - Run k6 and TestCafe checks
  - Upload execution reports as workflow artifacts

### Agentic Repository Automation

- **`daily-doc-updater` (weekly/manual):** Proposes documentation updates based on recent merged changes.
- **`daily-repo-status` (weekly/manual):** Creates repository status issues with activity and recommendations.
- **`code-simplifier` (weekly):** Opens refactoring PRs to improve readability and maintainability.

---

## Feature Plan

| Week | Focus | Planned Items |
| --- | --- | --- |
| 5 | Kick-off | No planned features |
| 6 | Authentication | Login backend, Login frontend |
| 7 | Winter break | Nothing planned |
| 8 | Workout creation | Add workout backend, Add set to workout backend |
| 9 | Retrieval + SPA | Get workout backend, SPA setup frontend |
| 10 | UI foundations | Home page setup frontend, Get workout frontend |
| 11 | Workout UX | Add workout frontend, Add set to workout frontend |
| 12 | Navigation | Nav bar navigation, Log out |
| 13 | Registration | Registration user backend, Registration user frontend |
| 14 | Easter break | Collect Easter eggs |
| 15 | Profile | Get profile info backend, Get profile info frontend |
| 16 | Email change | Change email frontend, Change email backend |
| 17 | Upcoming | Feature 1: [...], Feature 2: [...] |

---

## CALMR

### Culture

The team works with a generative culture: failures are treated as learning opportunities, communication is encouraged, new information is implemented quickly, and responsibilities are shared instead of isolated.

### Automation

CI/CD is heavily automated with GitHub Actions for testing, quality checks, image publishing, deployments, migrations, and post-deployment verification across staging and production flows.

### Lean

The workflow emphasizes small, incremental changes through `main`, fast feedback from staging, and controlled promotion to production, reducing waste and large risky releases.

### Measurement

The project measures quality and reliability through unit coverage, frontend coverage, Sonar quality gates, mutation testing, k6 performance runs, E2E tests, and stored workflow artifacts.

### Recovery

Recovery is supported by reproducible Docker deployments, health checks, staged promotion, and migration validation/repair paths that reduce downtime risk and make roll-forward operations safer.
