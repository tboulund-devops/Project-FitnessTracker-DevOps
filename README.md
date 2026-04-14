# Fitness Tracking

> A comprehensive strength training journal for tracking workouts, monitoring progress, and achieving your fitness goals.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11+-7B2FF7?style=flat&logo=avalonia&logoColor=white)](https://avaloniaui.net/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)

---

## Vision

An application for tracking strength training workouts with easy logging of weight, reps, sets, and rest periods.
Users can plan future workouts, review past sessions, and visualize their progression through clear statistics and analytics.

---

## Tech-stack

Language: C#
Framwork: ASPNET dotnet.10
Database: _PostgreSQL_ (if distrubuted) / SQLite (if local on phone)
Front-End: Avalonia Application

---

## Architecture

3 Tier based:

Tier 1.
Front-End: React + vite

Tier 2.
Back-End: Clean Architechture

Tier 3.
Database: PostgreSQL

---

## Automation & CI/CD

### GitHub Workflows

This project uses GitHub Actions for continuous integration, deployment, and automated maintenance.

#### CI/CD Pipelines

**Main Branch Workflow** (`workflow.yml`)
- **Trigger**: Push to `main` branch
- **Purpose**: Continuous integration and deployment to staging environment
- **Key Steps**:
  - Semantic versioning (automatic version bumps)
  - Static code analysis with SonarCloud
  - Build .NET solution
  - Docker image build and push to GHCR
  - Deploy to staging server with Docker Compose
  - Database migrations with Flyway
  - Performance testing with k6
  - E2E testing with TestCafe
  - Frontend build and deployment

**Production Workflow** (`production_workflow.yml`)
- **Trigger**: Pull request to `Production` branch (only accepts merges from `main`)
- **Purpose**: Production deployment with comprehensive testing
- **Target Host**: `devops@89.150.149.43` via production repository variables
- **Key Steps**:
  - Source branch validation (enforces main → Production flow)
  - Full test suite (unit tests, mutation tests, coverage)
  - SonarCloud quality gate validation
  - Docker image build with production tags
  - Production server deployment
  - Performance and E2E testing validation
- **Required Production Variables/Secrets**:
  - Variables: `PRODUCTION_SERVER_IP`, `PRODUCTION_SERVER_USERNAME`, `DATABASE_USER`, `DATABASE_NAME`, `PROJECT_KEY`, `SONAR_URL`
  - Secrets: `PRODUCTION_SERVER_SSH_KEY`, `DATABASE_PASSWORD`, `SUDO_PASSWORD`, `GHCR_PAT`, `FITNESSTRACKERTOKEN`

#### Agentic Workflows

Automated AI-powered workflows that maintain and improve the repository:

**Daily Documentation Updater** (`daily-doc-updater`)
- **Schedule**: Weekly (configurable via workflow_dispatch)
- **Purpose**: Automatically reviews merged PRs from the last 24 hours and updates documentation
- **Features**:
  - Scans for new features, changes, and breaking changes
  - Updates README and other documentation files
  - Creates pull requests with documentation improvements
  - Maintains consistent documentation style
- **Labels**: `documentation`, `automation`

**Daily Repository Status** (`daily-repo-status`)
- **Schedule**: Weekly (configurable via workflow_dispatch)
- **Purpose**: Creates daily status reports as GitHub issues
- **Features**:
  - Summarizes recent repository activity
  - Tracks issues, PRs, discussions, and releases
  - Provides productivity insights and recommendations
  - Highlights community contributions
  - Suggests actionable next steps for maintainers
- **Labels**: `report`, `daily-status`

### Docker & Deployment

The project uses Docker Compose for orchestration with separate configurations:
- `docker-compose.yml` - Production environment
- `docker-compose.staging.yml` - Staging environment
- `docker-compose.dev.yml` - Local development

**Services**:
- Frontend (React + Vite) - Port 8030 (staging)
- Backend (ASP.NET Core) - Port 8081 (staging)
- Database (PostgreSQL) - Internal
- Flyway - Database migrations

### How to Use Automated Workflows

**Trigger Manual Documentation Updates**:
```bash
# Go to Actions tab in GitHub
# Select "Daily Documentation Updater"
# Click "Run workflow" button
# The bot will review recent changes and create a documentation PR if needed
```

**Trigger Manual Repository Status Report**:
```bash
# Go to Actions tab in GitHub
# Select "Daily Repository Status"
# Click "Run workflow" button
# The bot will create an issue with the latest repository status
```

**View Workflow Results**:
- Documentation PRs are labeled with `documentation` and `automation`
- Status reports are created as issues with labels `report` and `daily-status`
- All workflow runs can be monitored in the Actions tab

---

## Feature plan

### Week 5

_Kick-off week - no features to be planned here_

### Week 6

**Feature 1:** Login Backend

**Feature 2:** Login Frontend

### Week 7

_Winter vacation - nothing planned._

### Week 8

**Feature 1:** Add Workout Backend

**Feature 2:** Add set to workout Backend

### Week 9

**Feature 1:** Get workout backend

**Feature 2:** SPA Setup Frontend 

### Week 10

**Feature 1:** Home page frontend basic setup

**Feature 2:** Get Workout frontend

### Week 11

**Feature 1:** Add workout frontend

**Feature 2:** Add set to workout frontend

### Week 12

**Feature 1:** Navigation via Nav Bar

**Feature 2:** Log out

### Week 13

**Feature 1:** Registration User backend

**Feature 2:** Registration User Frontend

### Week 14

_Easter vacation - Collect Eastereggs._

### Week 15

**Feature 1:** Get Profile info Backend

**Feature 2:** Get Profile info Frontend

### Week 16

**Feature 1:** Change email Frontend

**Feature 2:** Change email Backend

### Week 17

**Feature 1:** [...]

**Feature 2:** [...]
