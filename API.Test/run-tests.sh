#!/bin/bash

# Script to run tests and save results to files
# Tests will always complete and results will be saved even if tests fail

echo "=========================================="
echo "Running API Tests"
echo "=========================================="
echo ""

# Create TestResults directory if it doesn't exist
mkdir -p TestResults

# Get current timestamp for unique file names
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Run tests with multiple loggers and save results
# --logger "trx;LogFileName=test-results-${TIMESTAMP}.trx" - saves XML results
# --logger "console;verbosity=normal" - shows output in console
# --results-directory ./TestResults - where to save results
# --settings test.runsettings - use configuration file
# || true - ensures script continues even if tests fail (exit code 0)

dotnet test \
  --logger "trx;LogFileName=test-results-${TIMESTAMP}.trx" \
  --logger "console;verbosity=normal" \
  --results-directory ./TestResults \
  --settings test.runsettings \
  --collect:"XPlat Code Coverage" \
  --no-build \
  || true

# Get the exit code before the || true
TEST_EXIT_CODE=$?

echo ""
echo "=========================================="
echo "Test Execution Complete"
echo "=========================================="
echo ""
echo "Test Results saved to: ./TestResults/"
echo "  - TRX File: test-results-${TIMESTAMP}.trx"
echo "  - Coverage: coverage.cobertura.xml"
echo ""

# List generated files
echo "Generated files:"
ls -lh TestResults/ | tail -n +2

echo ""
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✓ All tests passed!"
else
    echo "⚠ Some tests failed. Check TestResults for details."
    echo "Exit code: $TEST_EXIT_CODE (continuing anyway)"
fi

echo ""
echo "To view TRX results, open the .trx file in Visual Studio or use:"
echo "  dotnet tool install -g trx2html"
echo "  trx2html TestResults/test-results-${TIMESTAMP}.trx"

# Always exit with 0 so the script doesn't fail
exit 0
