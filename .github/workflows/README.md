# GitHub Actions Workflows Documentation

> Complete guide to the CI/CD pipelines for the Asset Management API

## Table of Contents

- [Overview](#overview)
- [Workflows Summary](#workflows-summary)
- [Setup Instructions](#setup-instructions)
- [CI Workflow](#ci-workflow)
- [CD Workflow](#cd-workflow)
- [PR Validation Workflow](#pr-validation-workflow)
- [Database Migration Check Workflow](#database-migration-check-workflow)
- [Configuration Guide](#configuration-guide)
- [Secrets Management](#secrets-management)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)
- [Customization Examples](#customization-examples)

---

## Overview

This repository includes four automated GitHub Actions workflows that provide comprehensive CI/CD capabilities:

1. **CI (Continuous Integration)** - Build, test, and validate code quality
2. **CD (Continuous Deployment)** - Deploy to Render platform
3. **PR Validation** - Validate pull requests before merge
4. **Database Migration Check** - Validate database migrations

### Benefits

- Automated testing and quality checks
- Consistent deployment process
- Early detection of issues
- Automatic code review assistance
- Migration validation before production

### Workflow Triggers

```
Code Push → CI Workflow → Tests Pass → CD Workflow → Deploy to Production
     ↓
Pull Request → PR Validation → Code Review → Merge
     ↓
Migration Changes → DB Migration Check → Validate SQL
```

---

## Workflows Summary

| Workflow | File | Trigger | Duration | Purpose |
|----------|------|---------|----------|---------|
| **CI** | `ci.yml` | Push to main/dev, PRs | 5-8 min | Build, test, quality checks |
| **CD** | `cd-render.yml` | CI success, manual | 2-3 min | Deploy to Render |
| **PR Validation** | `pr-validation.yml` | Pull request opened | 3-5 min | Fast PR validation |
| **DB Migration** | `db-migration-check.yml` | Migration file changes | 4-6 min | Validate migrations |

### Workflow Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                     Code Changes                             │
└──────────────────────┬──────────────────────────────────────┘
                       │
          ┌────────────┴──────────────┐
          ▼                           ▼
┌──────────────────┐        ┌──────────────────┐
│   CI Workflow    │        │  PR Validation   │
│                  │        │   (on PR)        │
│ • Build          │        │                  │
│ • Test           │        │ • Quick build    │
│ • Code quality   │        │ • Fast tests     │
│ • Security scan  │        │ • Metadata check │
│ • Docker build   │        │                  │
└────────┬─────────┘        └──────────────────┘
         │
         │ (on success + push to dev)
         ▼
┌──────────────────┐
│   CD Workflow    │
│                  │
│ • Deploy to      │
│   Render         │
│ • Health check   │
│ • Verification   │
└──────────────────┘

If migration files changed:
         │
         ▼
┌──────────────────┐
│ DB Migration     │
│ Check            │
│                  │
│ • Validate SQL   │
│ • Test on        │
│   PostgreSQL     │
│ • Check breaking │
│   changes        │
└──────────────────┘
```

---

## Setup Instructions

### Prerequisites

1. **GitHub Repository**
   - Repository with admin access
   - GitHub Actions enabled

2. **Render Account** (for CD workflow)
   - Web service created
   - Database provisioned
   - Environment variables configured

3. **Required Files**
   ```
   .github/workflows/
   ├── ci.yml
   ├── cd-render.yml
   ├── pr-validation.yml
   └── db-migration-check.yml
   ```

### Initial Setup

#### Step 1: Enable GitHub Actions

```
1. Go to repository Settings
2. Click "Actions" → "General"
3. Enable "Allow all actions and reusable workflows"
4. Click "Save"
```

#### Step 2: Configure Secrets

```
Settings → Secrets and variables → Actions → New repository secret
```

Add these secrets:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `RENDER_API_KEY` | Render API key (optional) | `rnd_xxxxxxxxxxxxx` |
| `DATABASE_URL` | Production database URL | `postgresql://user:pass@host/db` |
| `STAGING_DATABASE_URL` | Staging database URL | `postgresql://user:pass@host/db` |

#### Step 3: Configure Environment Variables

```
Settings → Environments → New environment
```

Create environments:
- `production`
- `staging`

For each environment, add:
- Protection rules (require reviewers)
- Environment secrets
- Deployment branch restrictions

#### Step 4: Verify Workflows

```bash
# Trigger CI manually
git commit --allow-empty -m "Test CI workflow"
git push origin development

# Check workflow status
# Actions tab → CI - Build and Test
```

### Permissions Required

```yaml
# Required permissions for workflows
permissions:
  contents: read        # Read repository contents
  pull-requests: write  # Comment on PRs
  issues: write         # Create issues
  checks: write         # Report check runs
  statuses: write       # Update commit statuses
```

---

## CI Workflow

**File:** `.github/workflows/ci.yml`

### Purpose

Comprehensive continuous integration that:
- Builds the application
- Runs all tests with coverage
- Checks code quality
- Scans for security vulnerabilities
- Tests Docker image
- Validates deployment readiness

### Trigger Events

```yaml
on:
  push:
    branches:
      - development
      - main
      - master
  pull_request:
    branches:
      - development
      - main
      - master
  workflow_dispatch:  # Manual trigger
```

### Jobs Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        CI Workflow                           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Job 1: Build                                                │
│  • Checkout code                                             │
│  • Setup .NET 8.0                                            │
│  • Restore dependencies                                      │
│  • Build solution (Release)                                  │
│  • Upload build artifacts                                    │
│  └─► Duration: ~2 minutes                                    │
│                                                               │
│  Job 2: Test (depends on Build)                             │
│  • Run all tests                                             │
│  • Generate code coverage                                    │
│  • Create coverage report                                    │
│  • Comment on PR with results                                │
│  • Upload test results                                       │
│  └─► Duration: ~2-3 minutes                                  │
│                                                               │
│  Job 3: Code Quality (depends on Build)                     │
│  • Format check (dotnet format)                              │
│  • Code analysis                                             │
│  └─► Duration: ~1 minute                                     │
│                                                               │
│  Job 4: Security Scan (depends on Build)                    │
│  • Check for vulnerable packages                             │
│  • Fail if vulnerabilities found                             │
│  └─► Duration: ~1 minute                                     │
│                                                               │
│  Job 5: Docker Build (depends on Build + Test)              │
│  • Build Docker image                                        │
│  • Test image builds correctly                               │
│  • Use cache for faster builds                               │
│  └─► Duration: ~2-3 minutes                                  │
│                                                               │
│  Job 6: Deployment Check (depends on all)                   │
│  • Verify deployment files exist                             │
│  • Check: Dockerfile, render.yaml, .env.example              │
│  └─► Duration: ~30 seconds                                   │
│                                                               │
│  Job 7: CI Success (depends on all)                         │
│  • Mark pipeline as successful                               │
│  • Generate summary                                          │
│  └─► Duration: ~10 seconds                                   │
│                                                               │
│  Total Duration: ~5-8 minutes                                │
└─────────────────────────────────────────────────────────────┘
```

### Environment Variables

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true
```

### Key Features

#### 1. Build with Caching

```yaml
- name: Restore dependencies
  run: dotnet restore

- name: Build solution
  run: dotnet build --configuration Release --no-restore
```

#### 2. Test with Coverage

```yaml
- name: Run tests with coverage
  run: |
    dotnet test \
      --configuration Release \
      --no-build \
      --logger "trx;LogFileName=test-results.trx" \
      --collect:"XPlat Code Coverage" \
      --results-directory ./TestResults
```

#### 3. Coverage Reporting

```yaml
- name: Generate coverage report
  uses: danielpalme/ReportGenerator-GitHub-Action@5.3.11
  with:
    reports: 'TestResults/**/coverage.cobertura.xml'
    targetdir: 'TestResults/CoverageReport'
    reporttypes: 'HtmlInline;Cobertura;MarkdownSummaryGithub'
```

#### 4. Security Scanning

```yaml
- name: Check for vulnerable packages
  run: dotnet list package --vulnerable --include-transitive
  continue-on-error: false  # Fail if vulnerabilities found
```

### Success Criteria

All jobs must pass:
- ✅ Build succeeds
- ✅ All tests pass
- ✅ No code quality issues
- ✅ No security vulnerabilities
- ✅ Docker image builds
- ✅ Deployment files present

### Artifacts Generated

1. **Build Artifacts** (1 day retention)
   - Compiled binaries
   - Release configuration

2. **Test Results** (7 days retention)
   - TRX test reports
   - Coverage data

3. **Coverage Report** (7 days retention)
   - HTML coverage report
   - Markdown summary

### Viewing Results

```
1. Go to Actions tab
2. Click on workflow run
3. View job details and artifacts
4. Download coverage report
```

---

## CD Workflow

**File:** `.github/workflows/cd-render.yml`

### Purpose

Automated deployment to Render platform:
- Deploys after CI passes
- Supports manual deployment
- Verifies deployment success
- Provides deployment summary

### Trigger Events

```yaml
on:
  # Automatic: After CI succeeds on development branch
  workflow_run:
    workflows: ["CI - Build and Test"]
    types: [completed]
    branches: [development]

  # Manual: Deploy to specific environment
  workflow_dispatch:
    inputs:
      environment:
        description: 'Deployment environment'
        required: true
        default: 'production'
        type: choice
        options:
          - production
          - staging
```

### Jobs Overview

```
┌─────────────────────────────────────────────────────────────┐
│                       CD Workflow                            │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Job 1: Check CI Status                                     │
│  • Verify CI workflow succeeded                             │
│  • Decide if deployment should proceed                       │
│  └─► Duration: ~10 seconds                                   │
│                                                               │
│  Job 2: Deploy to Render (if CI passed)                    │
│  • Trigger Render deployment                                │
│  • Monitor deployment progress                              │
│  • Wait for deployment completion                           │
│  └─► Duration: ~2-3 minutes                                  │
│                                                               │
│  Job 3: Verify Deployment                                   │
│  • Wait for service to be ready                             │
│  • Check deployment status                                  │
│  • Generate deployment summary                              │
│  └─► Duration: ~1 minute                                     │
│                                                               │
│  Total Duration: ~2-3 minutes                               │
└─────────────────────────────────────────────────────────────┘
```

### Deployment Flow

```
CI Success on 'development' branch
         │
         ▼
Check CI Status
         │
         ├─► CI Failed? → Skip deployment
         │
         └─► CI Passed? → Continue
                  │
                  ▼
         Deploy to Render
         (Render auto-builds from repo)
                  │
                  ▼
         Wait 60 seconds
                  │
                  ▼
         Verify Deployment
         • Check health endpoint
         • Confirm deployment status
                  │
                  ▼
         Generate Summary
         • Deployment info
         • Links to service
         • Next steps
```

### Environment Configuration

```yaml
environment:
  name: ${{ github.event.inputs.environment || 'production' }}
  url: https://assetmanagement-api.onrender.com
```

### Manual Deployment

```bash
# Via GitHub UI:
# 1. Go to Actions tab
# 2. Select "CD - Deploy to Render"
# 3. Click "Run workflow"
# 4. Select environment (production/staging)
# 5. Click "Run workflow"

# Via GitHub CLI:
gh workflow run cd-render.yml -f environment=production
```

### Deployment Verification

```yaml
- name: Verify deployment
  run: |
    # Health check
    curl -f https://assetmanagement-api.onrender.com/api/asset-categories

    # Or custom health endpoint
    curl -f https://assetmanagement-api.onrender.com/health
```

### Rollback Procedure

If deployment fails:

```bash
# 1. Identify last successful commit
git log --oneline

# 2. Revert to previous commit
git revert HEAD
git push origin development

# 3. Or redeploy previous version manually
gh workflow run cd-render.yml -f environment=production
```

---

## PR Validation Workflow

**File:** `.github/workflows/pr-validation.yml`

### Purpose

Fast validation for pull requests:
- Quick feedback before full CI
- Validates PR metadata
- Checks for merge conflicts
- Runs basic build and tests
- Automated code review

### Trigger Events

```yaml
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
    branches:
      - development
      - main
      - master
```

### Concurrency Control

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.event.pull_request.number }}
  cancel-in-progress: true  # Cancel old runs when new commit pushed
```

### Jobs Overview

```
┌─────────────────────────────────────────────────────────────┐
│                   PR Validation Workflow                     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Job 1: Check PR Status                                     │
│  • Skip if PR is in draft mode                              │
│  └─► Duration: ~5 seconds                                    │
│                                                               │
│  Job 2: PR Metadata                                         │
│  • Validate PR title (min 10 chars)                         │
│  • Check PR description exists                              │
│  • Suggest labels                                           │
│  └─► Duration: ~10 seconds                                   │
│                                                               │
│  Job 3: Quick Validation                                    │
│  • Fast build                                               │
│  • Run tests                                                │
│  • Comment results on PR                                    │
│  └─► Duration: ~2-3 minutes                                  │
│                                                               │
│  Job 4: Conflict Check                                      │
│  • Check for merge conflicts                                │
│  • Suggest rebase if needed                                 │
│  └─► Duration: ~30 seconds                                   │
│                                                               │
│  Job 5: Code Review Checks                                  │
│  • Check code formatting                                    │
│  • Count changed files                                      │
│  • Warn if PR is too large                                  │
│  └─► Duration: ~1 minute                                     │
│                                                               │
│  Job 6: Summary                                             │
│  • Generate validation summary                              │
│  • Mark PR as ready or needs work                           │
│  └─► Duration: ~10 seconds                                   │
│                                                               │
│  Total Duration: ~3-5 minutes                               │
└─────────────────────────────────────────────────────────────┘
```

### PR Quality Checks

#### 1. Title Validation

```yaml
- name: Validate PR title
  run: |
    PR_TITLE="${{ github.event.pull_request.title }}"

    if [ ${#PR_TITLE} -lt 10 ]; then
      echo "❌ PR title is too short (minimum 10 characters)"
      exit 1
    fi

    echo "✅ PR title is valid: $PR_TITLE"
```

#### 2. Description Check

```yaml
- name: Check PR description
  run: |
    PR_BODY="${{ github.event.pull_request.body }}"

    if [ -z "$PR_BODY" ]; then
      echo "⚠️ PR description is empty"
      echo "Consider adding:"
      echo "- What changes were made"
      echo "- Why these changes were needed"
      echo "- How to test the changes"
    fi
```

#### 3. Size Warning

```yaml
- name: Check for large PRs
  run: |
    CHANGED_FILES=${{ steps.changes.outputs.changed_files }}

    if [ $CHANGED_FILES -gt 50 ]; then
      echo "⚠️ This PR changes $CHANGED_FILES files"
      echo "Consider splitting into smaller PRs"
    fi
```

### Auto-Comments on PR

```yaml
- name: Comment PR with results
  uses: actions/github-script@v7
  with:
    script: |
      const message = '### 🔍 PR Validation Results\n\n' +
                     '✅ Build and tests passed!\n\n' +
                     '**Next Steps:**\n' +
                     '- Wait for full CI pipeline\n' +
                     '- Request code review\n';

      github.rest.issues.createComment({
        owner: context.repo.owner,
        repo: context.repo.repo,
        issue_number: context.payload.pull_request.number,
        body: message
      });
```

### Draft PR Behavior

```yaml
- name: Check if PR is draft
  run: |
    if [ "${{ github.event.pull_request.draft }}" == "true" ]; then
      echo "PR is in draft mode - skipping validation"
      exit 0
    fi
```

---

## Database Migration Check Workflow

**File:** `.github/workflows/db-migration-check.yml`

### Purpose

Validate database migrations before deployment:
- Validate SQL syntax
- Test migrations on PostgreSQL
- Check for breaking changes
- Verify naming conventions

### Trigger Events

```yaml
on:
  pull_request:
    paths:
      - 'migrations/**'
      - 'API/Data/Migrations/**'

  workflow_dispatch:  # Manual trigger
```

### Jobs Overview

```
┌─────────────────────────────────────────────────────────────┐
│              Database Migration Check Workflow               │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Job 1: Validate Migration Files                           │
│  • Check file naming (001_description.sql)                  │
│  • Detect duplicate migration numbers                       │
│  • Basic SQL syntax validation                              │
│  └─► Duration: ~30 seconds                                   │
│                                                               │
│  Job 2: Test Migrations on PostgreSQL                      │
│  • Spin up PostgreSQL 16 container                          │
│  • Create kosan schema                                      │
│  • Apply all migrations in order                            │
│  • Verify database state                                    │
│  └─► Duration: ~2-3 minutes                                  │
│                                                               │
│  Job 3: Check Breaking Changes                             │
│  • Analyze for DROP statements                              │
│  • Check for new constraints (NOT NULL, UNIQUE)             │
│  • Warn about potential issues                              │
│  └─► Duration: ~30 seconds                                   │
│                                                               │
│  Job 4: Summary                                             │
│  • Generate migration summary                               │
│  • Mark as ready for production                             │
│  └─► Duration: ~10 seconds                                   │
│                                                               │
│  Total Duration: ~4-6 minutes                               │
└─────────────────────────────────────────────────────────────┘
```

### PostgreSQL Service Container

```yaml
services:
  postgres:
    image: postgres:16
    env:
      POSTGRES_DB: assetmanagement_test
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    options: >-
      --health-cmd pg_isready
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
    ports:
      - 5432:5432
```

### Migration Validation

#### 1. Naming Convention

```bash
# Valid: 001_create_users.sql
# Valid: 002_add_email_column.sql
# Invalid: create_users.sql
# Invalid: 1_migration.sql

# Check pattern
if ! [[ $filename =~ ^[0-9]{3}_[a-z_]+\.sql$ ]]; then
  echo "❌ Invalid migration filename: $filename"
  echo "Expected format: NNN_description.sql"
  exit 1
fi
```

#### 2. Duplicate Detection

```bash
# Find duplicate migration numbers
DUPLICATES=$(ls migrations/*.sql | sed 's/.*\/\([0-9]*\)_.*/\1/' | sort | uniq -d)

if [ ! -z "$DUPLICATES" ]; then
  echo "❌ Duplicate migration numbers found: $DUPLICATES"
  exit 1
fi
```

#### 3. Breaking Changes Detection

```bash
# Check for potentially dangerous operations
if grep -qi "DROP TABLE\|DROP COLUMN\|ALTER.*DROP" "$file"; then
  echo "⚠️ WARNING: $file contains DROP statements"
  echo "Ensure:"
  echo "- Database backup is available"
  echo "- Rollback procedure is documented"
fi
```

### Testing Migrations

```yaml
- name: Apply all migrations in order
  run: |
    for file in $(ls migrations/[0-9]*.sql | sort -V); do
      echo "Applying migration: $file"

      if PGPASSWORD=postgres psql -h localhost -U postgres \
         -d assetmanagement_test -f "$file"; then
        echo "✅ Successfully applied: $file"
      else
        echo "❌ Failed to apply: $file"
        exit 1
      fi
    done
```

---

## Configuration Guide

### Customizing Workflow Triggers

#### Run on Specific Branches

```yaml
# Only run on main and staging branches
on:
  push:
    branches:
      - main
      - staging
```

#### Run on File Changes

```yaml
# Only run when specific files change
on:
  push:
    paths:
      - 'API/**'
      - 'migrations/**'
      - '.github/workflows/**'
```

#### Exclude Paths

```yaml
# Don't run when only docs change
on:
  push:
    paths-ignore:
      - '**.md'
      - 'docs/**'
```

### Customizing Test Coverage

```yaml
# Change coverage thresholds
- name: Check coverage threshold
  run: |
    COVERAGE=$(grep -oP 'Line coverage: \K[\d.]+' coverage.txt)
    THRESHOLD=80

    if (( $(echo "$COVERAGE < $THRESHOLD" | bc -l) )); then
      echo "❌ Coverage $COVERAGE% is below threshold $THRESHOLD%"
      exit 1
    fi
```

### Adding Slack Notifications

```yaml
- name: Notify Slack on failure
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK_URL }}
    payload: |
      {
        "text": "❌ CI failed for ${{ github.repository }}",
        "blocks": [
          {
            "type": "section",
            "text": {
              "type": "mrkdwn",
              "text": "Build failed on branch `${{ github.ref_name }}`"
            }
          }
        ]
      }
```

### Environment-Specific Configuration

```yaml
# Different behavior for different environments
- name: Deploy
  run: |
    if [ "${{ github.event.inputs.environment }}" == "production" ]; then
      echo "Deploying to production with extra checks"
      npm run deploy:prod
    else
      echo "Deploying to staging"
      npm run deploy:staging
    fi
```

---

## Secrets Management

### Required Secrets

Configure in: `Settings → Secrets and variables → Actions`

#### Repository Secrets

| Secret | Description | Used In |
|--------|-------------|---------|
| `RENDER_API_KEY` | Render API key for deployments | CD workflow |
| `DATABASE_URL` | Production database connection | CD, DB migration |
| `STAGING_DATABASE_URL` | Staging database connection | CD, DB migration |
| `SLACK_WEBHOOK_URL` | Slack notifications (optional) | All workflows |

#### Environment Secrets

For each environment (production, staging):

```
Environment: production
├── DATABASE_URL
├── JWT_SECRET
├── EMAIL_PASSWORD
└── SMTP_CREDENTIALS
```

### Using Secrets in Workflows

```yaml
# Access repository secret
- name: Deploy
  env:
    API_KEY: ${{ secrets.RENDER_API_KEY }}
  run: |
    curl -X POST https://api.render.com/deploy \
      -H "Authorization: Bearer $API_KEY"

# Access environment secret
- name: Database migration
  env:
    DATABASE_URL: ${{ secrets.DATABASE_URL }}
  run: |
    psql $DATABASE_URL -f migrations/001_migration.sql
```

### Security Best Practices

1. **Never log secrets**
   ```yaml
   # ❌ BAD - exposes secret
   - run: echo "API Key: ${{ secrets.API_KEY }}"

   # ✅ GOOD - no logging
   - run: deploy-script.sh
     env:
       API_KEY: ${{ secrets.API_KEY }}
   ```

2. **Use environment protection**
   ```yaml
   environment:
     name: production
     # Requires approval before deployment
   ```

3. **Rotate secrets regularly**
   ```bash
   # Update secret via GitHub CLI
   gh secret set DATABASE_URL --body "new-connection-string"
   ```

---

## Troubleshooting

### Issue 1: Workflow Not Triggering

**Symptoms:** Workflow doesn't run on push/PR

**Solutions:**

1. **Check workflow file syntax**
   ```bash
   # Validate YAML
   yamllint .github/workflows/ci.yml
   ```

2. **Verify trigger configuration**
   ```yaml
   # Make sure branch name matches
   on:
     push:
       branches:
         - development  # Must match exactly
   ```

3. **Check Actions permissions**
   ```
   Settings → Actions → General
   Ensure "Allow all actions" is selected
   ```

### Issue 2: Tests Failing in CI but Pass Locally

**Symptoms:** Tests pass on local machine but fail in CI

**Solutions:**

1. **Check .NET version**
   ```yaml
   # Ensure version matches local
   - uses: actions/setup-dotnet@v4
     with:
       dotnet-version: '8.0.x'  # Match your local version
   ```

2. **Check test runsettings**
   ```yaml
   - run: dotnet test --settings API.Test/test.runsettings
   ```

3. **Add debug logging**
   ```yaml
   - run: dotnet test --logger "console;verbosity=detailed"
   ```

### Issue 3: Docker Build Fails

**Symptoms:** Docker build job fails in CI

**Solutions:**

1. **Increase timeout**
   ```yaml
   - name: Build Docker image
     timeout-minutes: 20  # Increase from default 10
   ```

2. **Check Dockerfile path**
   ```yaml
   uses: docker/build-push-action@v5
   with:
     context: .
     file: ./Dockerfile  # Verify path is correct
   ```

3. **Add build logs**
   ```yaml
   - name: Build Docker image
     run: docker build --progress=plain -t app .
   ```

### Issue 4: Deployment Not Triggering

**Symptoms:** CD workflow doesn't run after CI success

**Solutions:**

1. **Check workflow_run syntax**
   ```yaml
   workflow_run:
     workflows: ["CI - Build and Test"]  # Must match CI name exactly
     types: [completed]
     branches: [development]
   ```

2. **Verify CI status check**
   ```yaml
   if: ${{ github.event.workflow_run.conclusion == 'success' }}
   ```

3. **Use workflow_dispatch for manual trigger**
   ```bash
   gh workflow run cd-render.yml
   ```

### Issue 5: Secret Not Found

**Symptoms:** `Error: Secret XXXX not found`

**Solutions:**

1. **Verify secret name matches**
   ```yaml
   # Exact match required (case-sensitive)
   env:
     API_KEY: ${{ secrets.RENDER_API_KEY }}  # Not render_api_key
   ```

2. **Check secret scope**
   ```
   Repository secret: Available to all workflows
   Environment secret: Only available when environment is specified
   ```

3. **Verify secret value is set**
   ```
   Settings → Secrets → Check secret exists
   ```

---

## Best Practices

### 1. Keep Workflows Fast

```yaml
# ✅ GOOD - Parallel jobs
jobs:
  build:
    runs-on: ubuntu-latest
  test:
    runs-on: ubuntu-latest
    needs: build
  lint:
    runs-on: ubuntu-latest  # Runs in parallel with test

# ❌ BAD - Sequential jobs
jobs:
  job1:
    needs: []
  job2:
    needs: [job1]
  job3:
    needs: [job2]  # Slow!
```

### 2. Use Caching

```yaml
# Cache NuGet packages
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

# Cache Docker layers
- uses: docker/build-push-action@v5
  with:
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

### 3. Fail Fast

```yaml
# Stop on first error
strategy:
  fail-fast: true

# Or continue on error for non-critical jobs
- name: Format check
  run: dotnet format --verify-no-changes
  continue-on-error: true
```

### 4. Use Matrix Builds

```yaml
# Test on multiple .NET versions
strategy:
  matrix:
    dotnet-version: ['6.0.x', '7.0.x', '8.0.x']

steps:
  - uses: actions/setup-dotnet@v4
    with:
      dotnet-version: ${{ matrix.dotnet-version }}
```

### 5. Add Status Badges

```markdown
# In README.md
![CI](https://github.com/user/repo/workflows/CI/badge.svg)
![CD](https://github.com/user/repo/workflows/CD/badge.svg)
```

### 6. Use Concurrency Limits

```yaml
# Cancel old workflow runs
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

### 7. Protect Main Branch

```
Settings → Branches → Add rule

Rules:
✅ Require pull request reviews
✅ Require status checks to pass (select CI workflow)
✅ Require conversation resolution
✅ Include administrators
```

---

## Customization Examples

### Example 1: Add Email Notifications

```yaml
# Add to end of CI workflow
- name: Send email on failure
  if: failure()
  uses: dawidd6/action-send-mail@v3
  with:
    server_address: smtp.gmail.com
    server_port: 587
    username: ${{ secrets.EMAIL_USERNAME }}
    password: ${{ secrets.EMAIL_PASSWORD }}
    subject: CI Failed - ${{ github.repository }}
    body: |
      Build failed for commit ${{ github.sha }}
      Branch: ${{ github.ref_name }}
      View: https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }}
    to: team@example.com
    from: ci@example.com
```

### Example 2: Automated Dependency Updates

Create `.github/workflows/dependency-update.yml`:

```yaml
name: Dependency Update

on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly on Sunday
  workflow_dispatch:

jobs:
  update-dependencies:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Update NuGet packages
        run: |
          dotnet list package --outdated
          dotnet outdated --upgrade

      - name: Create Pull Request
        uses: peter-evans/create-pull-request@v5
        with:
          commit-message: Update dependencies
          title: 'chore: Update NuGet packages'
          body: Automated dependency update
          branch: dependency-updates
```

### Example 3: Performance Testing

```yaml
# Add to CI workflow
performance-test:
  name: Performance Testing
  runs-on: ubuntu-latest
  needs: build

  steps:
    - uses: actions/checkout@v4

    - name: Run performance tests
      run: |
        dotnet test --filter "Category=Performance" \
          --logger "console;verbosity=detailed"

    - name: Check performance metrics
      run: |
        # Compare against baseline
        # Fail if performance degrades >10%
        python scripts/check_performance.py
```

### Example 4: Security Scanning

```yaml
# Add security scanning job
security-scan:
  name: Security Scan
  runs-on: ubuntu-latest

  steps:
    - uses: actions/checkout@v4

    - name: Run Snyk security scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}

    - name: Run CodeQL analysis
      uses: github/codeql-action/analyze@v2
```

---

## Summary

This documentation covered:

- **4 GitHub Actions workflows** for comprehensive CI/CD
- **Setup and configuration** for each workflow
- **Secrets management** and security best practices
- **Troubleshooting** common issues
- **Customization examples** for extending workflows

### Key Features

✅ Automated testing and quality checks
✅ Continuous deployment to Render
✅ PR validation and auto-review
✅ Database migration validation
✅ Docker image building
✅ Security vulnerability scanning
✅ Code coverage reporting

### Quick Links

- [CI Workflow](.github/workflows/ci.yml)
- [CD Workflow](.github/workflows/cd-render.yml)
- [PR Validation](.github/workflows/pr-validation.yml)
- [DB Migration Check](.github/workflows/db-migration-check.yml)

### Next Steps

1. Review and customize workflows for your needs
2. Configure required secrets in repository settings
3. Set up environment protection rules
4. Enable branch protection on main branch
5. Add status badges to README.md

---

**Last Updated:** November 2024
**Version:** 1.0
**Maintained by:** Asset Management API Team
