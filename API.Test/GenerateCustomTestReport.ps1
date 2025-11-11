# PowerShell Script to Generate Custom Test Report from TRX Results
# This script transforms TRX test results into the custom interactive HTML report

param(
    [string]$TrxFilePath = "",
    [string]$OutputPath = "TestResults/custom-test-report.html",
    [string]$TemplatePath = "TestReportTemplate.html"
)

# Function to find the latest TRX file if not specified
function Get-LatestTrxFile {
    $trxFiles = Get-ChildItem -Path "TestResults" -Filter "*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        return $trxFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }
    return $null
}

# Function to parse TRX file and extract test results
function Parse-TrxFile {
    param([string]$Path)

    [xml]$xml = Get-Content $Path

    $results = @{
        totalTests = 0
        passed = 0
        failed = 0
        skipped = 0
        results = @()
    }

    # Parse UnitTestResult elements
    foreach ($testResult in $xml.TestRun.Results.UnitTestResult) {
        $results.totalTests++

        $outcome = $testResult.outcome
        if ($outcome -eq "Passed") {
            $results.passed++
        }
        elseif ($outcome -eq "Failed") {
            $results.failed++
        }
        else {
            $results.skipped++
        }

        # Extract error message if exists
        $errorMessage = ""
        if ($testResult.Output.ErrorInfo.Message) {
            $errorMessage = $testResult.Output.ErrorInfo.Message
        }

        $testItem = @{
            name = $testResult.testName
            outcome = $outcome
            duration = $testResult.duration
            errorMessage = $errorMessage
        }

        $results.results += $testItem
    }

    return $results
}

# Function to escape JavaScript strings
function Escape-JavaScriptString {
    param([string]$str)
    return $str -replace '\\', '\\' -replace '"', '\"' -replace "`n", '\n' -replace "`r", '\r'
}

# Function to generate JavaScript data from test results
function ConvertTo-JavaScriptData {
    param($testResults)

    $jsLines = @()
    $jsLines += "const testResults = {"
    $jsLines += "    totalTests: $($testResults.totalTests),"
    $jsLines += "    passed: $($testResults.passed),"
    $jsLines += "    failed: $($testResults.failed),"
    $jsLines += "    skipped: $($testResults.skipped),"
    $jsLines += "    results: ["

    $resultLines = @()
    foreach ($result in $testResults.results) {
        $escapedName = Escape-JavaScriptString $result.name
        $escapedError = Escape-JavaScriptString $result.errorMessage

        $resultLines += "        {"
        $resultLines += "            name: `"$escapedName`","
        $resultLines += "            outcome: `"$($result.outcome)`","
        $resultLines += "            duration: `"$($result.duration)`","
        $resultLines += "            errorMessage: `"$escapedError`""
        $resultLines += "        },"
    }

    # Remove trailing comma from last item
    if ($resultLines.Count -gt 0) {
        $lastLine = $resultLines[-1]
        $resultLines[-1] = $lastLine -replace ',$', ''
    }

    $jsLines += $resultLines
    $jsLines += "    ]"
    $jsLines += "};"

    return $jsLines -join "`n"
}

# Main script execution
Write-Host "======================================================================"
Write-Host "Custom Test Report Generator"
Write-Host "======================================================================"
Write-Host ""

# Ensure TestResults directory exists
$testResultsDir = "TestResults"
if (-not (Test-Path $testResultsDir)) {
    Write-Host "TestResults directory not found. Creating it..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
    Write-Host "TestResults directory created successfully" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "TestResults directory found" -ForegroundColor Green
    Write-Host ""
}

# Find TRX file
if (-not $TrxFilePath) {
    $latestTrx = Get-LatestTrxFile
    if ($latestTrx) {
        $TrxFilePath = $latestTrx.FullName
        Write-Host "Found latest TRX file: $($latestTrx.Name)" -ForegroundColor Green
    }
    else {
        Write-Host ""
        Write-Host "ERROR: No TRX file found in TestResults directory" -ForegroundColor Red
        Write-Host ""
        Write-Host "Solution: Please run tests first using:" -ForegroundColor Yellow
        Write-Host "  dotnet test"
        Write-Host ""
        Write-Host "Then run this script again to generate the report."
        Write-Host ""
        exit 1
    }
}
else {
    if (-not (Test-Path $TrxFilePath)) {
        Write-Host ""
        Write-Host "ERROR: TRX file not found: $TrxFilePath" -ForegroundColor Red
        Write-Host ""
        exit 1
    }
    Write-Host "Using specified TRX file: $TrxFilePath" -ForegroundColor Green
}
Write-Host ""

# Check if template exists
if (-not (Test-Path $TemplatePath)) {
    Write-Host "ERROR: Template file not found: $TemplatePath" -ForegroundColor Red
    exit 1
}

Write-Host "Template file found: $TemplatePath" -ForegroundColor Green
Write-Host ""

# Parse TRX file
Write-Host "Parsing test results..."
$testResults = Parse-TrxFile -Path $TrxFilePath
Write-Host "Found $($testResults.totalTests) tests total" -ForegroundColor Cyan
Write-Host "  Passed: $($testResults.passed)" -ForegroundColor Green
Write-Host "  Failed: $($testResults.failed)" -ForegroundColor Yellow
Write-Host "  Skipped: $($testResults.skipped)" -ForegroundColor Yellow
Write-Host ""

# Generate JavaScript data
Write-Host "Generating JavaScript data..."
$jsData = ConvertTo-JavaScriptData -testResults $testResults
Write-Host "JavaScript data generated successfully" -ForegroundColor Green
Write-Host ""

# Read template
$templateContent = Get-Content -Path $TemplatePath -Raw

# Replace the test data in template
$modifiedContent = $templateContent -replace 'const testResults = \{[^}]*totalTests: 0,[^}]*\};', $jsData

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Write the final HTML file
Set-Content -Path $OutputPath -Value $modifiedContent -Encoding UTF8

Write-Host "======================================================================"
Write-Host "SUCCESS: Report generated successfully!" -ForegroundColor Green
Write-Host "======================================================================"
Write-Host ""
Write-Host "Report saved to: $OutputPath" -ForegroundColor Green
Write-Host ""
Write-Host "To view the report:"
Write-Host "  1. Open the file in your web browser"
Write-Host "  2. Use the dropdown menu to filter results"
Write-Host "  3. Review test results and error details"
Write-Host ""
