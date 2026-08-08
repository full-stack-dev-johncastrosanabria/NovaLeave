#!/usr/bin/env bash
# NovaLeave Test Runner
# Runs all unit, integration, and end-to-end tests
# Usage:
#   ./run-tests.sh                  # Run all tests
#   ./run-tests.sh unit             # Run only unit tests
#   ./run-tests.sh integration      # Run only integration tests
#   ./run-tests.sh e2e              # Run only end-to-end tests

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"

# Load .env so integration tests can reach the Docker SQL container without manual exports
if [[ -f "$PROJECT_ROOT/.env" && -z "${NOVALEAVE_TEST_SQLSERVER:-}" ]]; then
  # shellcheck source=/dev/null
  set -a; source "$PROJECT_ROOT/.env"; set +a
  SA_PASS="${MSSQL_SA_PASSWORD:-YourStrong@Passw0rd}"
  SQL_PORT="${NOVALEAVE_SQL_PORT:-1433}"
  export NOVALEAVE_TEST_SQLSERVER="Server=localhost,${SQL_PORT};Database=NovaLeave_Test;User Id=sa;Password=${SA_PASS};TrustServerCertificate=True;MultipleActiveResultSets=true;Pooling=false"
fi

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test categories
UNIT_TESTS="tests/NovaLeave.UnitTests/NovaLeave.UnitTests.csproj"
INTEGRATION_TESTS="tests/NovaLeave.IntegrationTests/NovaLeave.IntegrationTests.csproj"
E2E_TESTS="tests/NovaLeave.EndToEndTests/NovaLeave.EndToEndTests.csproj"

# Function to print section headers
print_header() {
  echo ""
  echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
  echo -e "${BLUE}║ $1${NC}"
  echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
  echo ""
}

# Function to run tests for a project
run_test_project() {
  local project_name=$1
  local project_path=$2
  
  print_header "Running $project_name"
  
  if [ ! -f "$PROJECT_ROOT/$project_path" ]; then
    echo -e "${RED}✗ Test project not found: $project_path${NC}"
    return 1
  fi
  
  if dotnet test "$PROJECT_ROOT/$project_path" -c Debug --no-build --verbosity quiet; then
    echo -e "${GREEN}✓ $project_name passed${NC}"
    return 0
  else
    echo -e "${RED}✗ $project_name failed${NC}"
    return 1
  fi
}

# Function to print test summary
print_summary() {
  local passed=$1
  local failed=$2
  local total=$((passed + failed))
  
  echo ""
  echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
  echo -e "${BLUE}║ TEST SUMMARY${NC}"
  echo -e "${BLUE}╠════════════════════════════════════════════════════════════╣${NC}"
  echo -e "${BLUE}║ Total Test Suites: $total${NC}"
  
  if [ $passed -gt 0 ]; then
    echo -e "${BLUE}║ ${GREEN}Passed: $passed${BLUE}${NC}"
  fi
  
  if [ $failed -gt 0 ]; then
    echo -e "${BLUE}║ ${RED}Failed: $failed${BLUE}${NC}"
  fi
  
  echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
  echo ""
}

# Main test execution logic
main() {
  local test_type="${1:-all}"
  local passed=0
  local failed=0
  
  print_header "NovaLeave Test Runner"
  echo "Test Type: $test_type"
  echo "Working Directory: $PROJECT_ROOT"
  
  case "$test_type" in
    unit)
      if run_test_project "Unit Tests" "$UNIT_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      ;;
    integration)
      if run_test_project "Integration Tests" "$INTEGRATION_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      ;;
    e2e)
      if run_test_project "End-to-End Tests" "$E2E_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      ;;
    all)
      if run_test_project "Unit Tests" "$UNIT_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      
      if run_test_project "Integration Tests" "$INTEGRATION_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      
      if run_test_project "End-to-End Tests" "$E2E_TESTS"; then
        ((passed++))
      else
        ((failed++))
      fi
      ;;
    *)
      echo -e "${RED}Unknown test type: $test_type${NC}"
      echo "Usage: ./run-tests.sh [all|unit|integration|e2e]"
      exit 1
      ;;
  esac
  
  print_summary $passed $failed
  
  if [ $failed -gt 0 ]; then
    exit 1
  fi
}

main "$@"
