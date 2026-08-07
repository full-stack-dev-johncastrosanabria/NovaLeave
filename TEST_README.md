# NovaLeave Testing Guide

Complete guide for installing, running, and managing tests in the NovaLeave vacation request management system.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Running Tests](#running-tests)
- [Test Structure](#test-structure)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### System Requirements

- **macOS** (tested on macOS 12+) or Linux
- **Docker** (for SQL Server and test databases)
- **.NET SDK 10.0+** (or current SDK version from `Directory.Build.props`)
- **Bash 4.0+** (for test runner script)

### Verify Prerequisites

```bash
# Check .NET SDK
dotnet --version

# Check Docker
docker --version

# Check Bash
bash --version
```

---

## Installation

### 1. Clone or Navigate to Repository

```bash
cd /path/to/NovaLeave
```

### 2. Install Dependencies

The project uses:
- **xUnit** for unit and integration tests
- **Testcontainers** for containerized SQL Server instances
- **Playwright** for end-to-end browser testing
- **FluentValidation** for validation testing

Dependencies are automatically restored via:

```bash
# Restore NuGet packages (automatic with `dotnet test` or `dotnet build`)
dotnet restore
```

### 3. Start SQL Server (for Integration & E2E Tests)

Integration and end-to-end tests require a real SQL Server instance.

**Option A: Docker Compose (Recommended)**

```bash
# Start the containerized SQL Server
docker compose up -d

# Verify it's healthy
docker compose ps
# Output should show: "healthy" status
```

**Option B: Local SQL Server**

If you have SQL Server installed locally, ensure it's running on the default port (1433).

### 4. Configure Environment

The project uses `.env` file for configuration. Ensure it exists and contains:

```bash
MSSQL_SA_PASSWORD=NovaLeaveHgZaLOlTwfNS!7
NOVALEAVE_SQL_PORT=14333
ASPNETCORE_ENVIRONMENT=Development
```

---

## Running Tests

### Quick Start

Run all tests with one command:

```bash
./run-tests.sh
```

### Test Categories

#### Run All Tests

```bash
./run-tests.sh all
```

#### Run Unit Tests Only

```bash
./run-tests.sh unit
```

**What it tests**: Domain invariants, business rules, calculations, validation logic  
**Duration**: ~10–30 seconds  
**Database**: No SQL Server required

#### Run Integration Tests Only

```bash
./run-tests.sh integration
```

**What it tests**: Application use cases, authorization, concurrency, audit trail, EF Core transactions  
**Duration**: ~1–3 minutes  
**Database**: Requires SQL Server (real instance, not in-memory)  
**Isolation**: Each test gets its own isolated database

#### Run End-to-End Tests Only

```bash
./run-tests.sh e2e
```

**What it tests**: Critical browser journeys (login, create, approve, reject, timeout, calendar, HR views), accessibility smoke test  
**Duration**: ~3–8 minutes  
**Database**: Requires SQL Server  
**Browser**: Uses Playwright (headless by default)

### Advanced Test Options

#### Run Specific Test Class

```bash
# Unit test example
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  -c Debug --filter "ClassName=VacationRequestValidationTests"

# Integration test example
dotnet test tests/NovaLeave.IntegrationTests/NovaLeave.IntegrationTests.csproj \
  -c Debug --filter "ClassName=UC01AuthenticateTests"
```

#### Run with Verbose Output

```bash
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  -c Debug --verbosity detailed
```

#### Run with Coverage

```bash
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  -c Debug /p:CollectCoverage=true /p:CoverageFormat=cobertura
```

---

## Test Structure

### Directory Organization

```
NovaLeave/
├── tests/
│   ├── NovaLeave.UnitTests/
│   │   ├── Domain/                    # Domain invariants, working-day calc, balance rules
│   │   ├── Application/               # Use case scenarios, command/query validation
│   │   └── Infrastructure/            # EF Core, Identity, concurrency
│   │
│   ├── NovaLeave.IntegrationTests/
│   │   ├── UC01_Authenticate/         # Login, session, antiforgery
│   │   ├── UC03_ViewOwnRequests/      # List own requests
│   │   ├── UC04_CreateRequest/        # Create with all validation rules
│   │   ├── UC05_EditPendingRequest/   # Edit, revalidation, RowVersion
│   │   ├── UC06_ViewRequestDetail/    # Detail, audit trail
│   │   ├── Concurrency/               # Optimistic locking scenarios
│   │   ├── Approval/                  # Approval, rejection, deactivation
│   │   ├── Balance/                   # Balance integrity, movements
│   │   ├── Timeout/                   # Timeout cancellation job
│   │   ├── Accrual/                   # Monthly accrual job
│   │   └── Authorization/             # Role-based access control
│   │
│   └── NovaLeave.EndToEndTests/
│       ├── Critical/                  # Essential user journeys
│       │   ├── LoginTests.cs
│       │   ├── CreateRequestTests.cs
│       │   ├── ApproveRejectTests.cs
│       │   ├── CalendarTests.cs
│       │   ├── TimeoutTests.cs
│       │   └── HRViewsTests.cs
│       └── Accessibility/             # WCAG 2.1 AA baseline smoke test
│           └── AccessibilityTests.cs
├── run-tests.sh                       # Test runner script
└── TEST_README.md                     # This file
```

### Test Frameworks

| Layer | Framework | ORM | Database |
|-------|-----------|-----|----------|
| **Unit** | xUnit | N/A | No DB |
| **Integration** | xUnit + WebApplicationFactory | EF Core | Real SQL Server |
| **E2E** | xUnit + Playwright | Via web app | Real SQL Server |

### Test-First Discipline

Tests are organized by:
- **Use Case (UC)**: Each numbered UC has full contract coverage (given/when/then)
- **Vertical Slice**: Each Application command/query has its own test file
- **Concurrency & Races**: Dedicated race condition test suites
- **Acceptance Criteria**: Test names and arrangements mirror spec acceptance scenarios

---

## Troubleshooting

### Issue: "Connection refused" or "Cannot connect to SQL Server"

**Solution:**

1. Verify Docker container is running:
   ```bash
   docker compose ps
   ```
   Should show `novaleave-sqlserver` with status `Up` and `(healthy)`.

2. If not running, start it:
   ```bash
   docker compose up -d
   sleep 30  # Wait for healthcheck
   ```

3. Check the `.env` file for correct port and password:
   ```bash
   grep -E "MSSQL_SA_PASSWORD|NOVALEAVE_SQL_PORT" .env
   ```

4. Verify connectivity manually:
   ```bash
   docker exec novaleave-sqlserver /opt/mssql-tools18/bin/sqlcmd \
     -C -S localhost -U sa -P "YourPassword" -Q "SELECT 1"
   ```

### Issue: Tests timeout or hang

**Solution:**

1. Increase test timeout in test project `.csproj`:
   ```xml
   <PropertyGroup>
     <CollectCoverage>false</CollectCoverage>
     <Timeout>60000</Timeout>  <!-- milliseconds -->
   </PropertyGroup>
   ```

2. Check if SQL Server is healthy:
   ```bash
   docker compose ps
   # Should show "(healthy)" not "(unhealthy)"
   ```

3. Run with verbose output to see where it hangs:
   ```bash
   ./run-tests.sh all 2>&1 | tee test-output.log
   ```

### Issue: "Cannot load Playwright browsers" (E2E tests)

**Solution:**

1. Install Playwright browsers:
   ```bash
   cd tests/NovaLeave.EndToEndTests
   playwright install
   ```

2. Ensure you have write permissions to `~/.cache/ms-playwright/`:
   ```bash
   mkdir -p ~/.cache/ms-playwright
   chmod 755 ~/.cache/ms-playwright
   ```

3. If still failing, run with browser debugging:
   ```bash
   PWDEBUG=1 dotnet test tests/NovaLeave.EndToEndTests/NovaLeave.EndToEndTests.csproj
   ```

### Issue: "Tests fail with 'OleDbException' or schema errors"

**Solution:**

1. Drop and recreate the test databases:
   ```bash
   # Connect to SQL Server
   docker exec -it novaleave-sqlserver \
     /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "YourPassword"
   
   # Then run in sqlcmd:
   DROP DATABASE IF EXISTS NovaLeave_UnitTests;
   DROP DATABASE IF EXISTS NovaLeave_IntegrationTests;
   GO
   ```

2. Re-run tests (they will auto-create schemas via EF Core migrations):
   ```bash
   ./run-tests.sh
   ```

### Issue: "dotnet test: command not found"

**Solution:**

Ensure .NET SDK is installed and in PATH:

```bash
# Check installation
dotnet --version

# If not found, install from https://dotnet.microsoft.com/download
# Or use Homebrew (macOS):
brew install dotnet
```

---

## CI/CD Integration

### GitHub Actions Example

Add to `.github/workflows/tests.yml`:

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: YourStrong@Passw0rd
          ACCEPT_EULA: Y
        options: >-
          --health-cmd "/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P YourStrong@Passw0rd -Q SELECT 1"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 3
        ports:
          - 1433:1433

    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - run: ./run-tests.sh all
```

---

## Local Development Workflow

### 1. Before Committing Code

```bash
# Run affected test category (e.g., unit for domain changes)
./run-tests.sh unit

# Or run specific test class
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  --filter "ClassName=YourTestClass"
```

### 2. Before Submitting PR

```bash
# Run all tests
./run-tests.sh all

# Fix any failures
# Then commit
git add -A
git commit -m "feat: Add new feature"
```

### 3. Debugging a Failing Test

```bash
# Run with verbose output
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  --filter "ClassName=FailingTest" --verbosity detailed

# Or open in IDE and set breakpoints
# Then run with debugger
dotnet test tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj \
  --filter "ClassName=FailingTest" --logger "console;verbosity=detailed"
```

---

## Performance Notes

- **Unit tests**: Fastest (~10–30 sec). Run frequently during development.
- **Integration tests**: Slower (~1–3 min). Require real SQL Server and data setup/teardown.
- **E2E tests**: Slowest (~3–8 min). Use Playwright, real browser, full app startup.

**Recommendation**: Commit gates should run `./run-tests.sh all` to ensure complete coverage.

---

## Additional Resources

- **xUnit Documentation**: https://xunit.net/
- **WebApplicationFactory**: https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1
- **Playwright .NET**: https://playwright.dev/dotnet/
- **SQL Server Testing**: https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker

---

**Last Updated**: 2026-08-07  
**Maintained By**: Development Team  
**Status**: Production-Ready
