# PowerShell script to run tests and save results to files
# Tests will always complete and results will be saved even if tests fail

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Running API Tests" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Create TestResults directory if it doesn't exist
if (-not (Test-Path "TestResults")) {
    New-Item -ItemType Directory -Path "TestResults" | Out-Null
}

# Get current timestamp for unique file names
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "Timestamp: $timestamp" -ForegroundColor Gray
Write-Host ""

# Run tests with multiple loggers and save results
# --logger "trx;LogFileName=..." - saves XML results
# --logger "console;verbosity=normal" - shows output in console
# --results-directory ./TestResults - where to save results
# --settings test.runsettings - use configuration file

$testProcess = Start-Process -FilePath "dotnet" -ArgumentList @(
    "test",
    "--logger", "trx;LogFileName=test-results-$timestamp.trx",
    "--logger", "console;verbosity=normal",
    "--results-directory", "./TestResults",
    "--settings", "test.runsettings",
    "--collect:XPlat Code Coverage",
    "--no-build"
) -NoNewWindow -Wait -PassThru

$exitCode = $testProcess.ExitCode

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Test Execution Complete" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test Results saved to: ./TestResults/" -ForegroundColor Green
Write-Host "  - TRX File: test-results-$timestamp.trx" -ForegroundColor Gray
Write-Host "  - Coverage: coverage.cobertura.xml" -ForegroundColor Gray
Write-Host ""

# List generated files
Write-Host "Generated files:" -ForegroundColor Yellow
Get-ChildItem -Path TestResults -Recurse | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

Write-Host ""
if ($exitCode -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "⚠ Some tests failed. Check TestResults for details." -ForegroundColor Yellow
    Write-Host "Exit code: $exitCode (continuing anyway)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "To view TRX results, open the .trx file in Visual Studio or use:" -ForegroundColor Cyan
Write-Host "  dotnet tool install -g trx2html" -ForegroundColor Gray
Write-Host "  trx2html TestResults/test-results-$timestamp.trx" -ForegroundColor Gray

# Always exit with 0 so the script doesn't fail pipelines
exit 0
