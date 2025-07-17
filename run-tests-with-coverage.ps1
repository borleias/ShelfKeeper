Write-Host "Running tests with coverage..." -ForegroundColor Green

# Clean up previous test results
$resultsDir = ".\TestResults"
if (Test-Path $resultsDir) {
    Remove-Item -Path $resultsDir -Recurse -Force
}

# Ensure coverage directory exists
$coverageDir = ".\TestResults\Coverage"
New-Item -Path $coverageDir -ItemType Directory -Force | Out-Null

# Run tests with coverage
dotnet test .\ShelfKeeper.sln `
    --collect:"XPlat Code Coverage" `
    --results-directory:$coverageDir `
    --settings:coverage.runsettings `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Continue even if some tests fail - we want coverage data regardless
Write-Host "Checking for coverage files..." -ForegroundColor Yellow
$coverageFiles = Get-ChildItem -Path $coverageDir -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage files found. Check if tests ran successfully." -ForegroundColor Red
    exit 1
}

# Create report directory
$reportDir = ".\TestResults\Report"
New-Item -Path $reportDir -ItemType Directory -Force | Out-Null

# Generate report
Write-Host "Generating coverage report..." -ForegroundColor Green
try {
    reportgenerator `
        -reports:$coverageDir\**\coverage.cobertura.xml `
        -targetdir:$reportDir `
        -reporttypes:Html `
        -sourcedirs:. `
        -historydir:.\TestResults\History

    Write-Host "Coverage report generated at: $reportDir\index.html" -ForegroundColor Green
    
    # Open the report in the default browser
    if ($?) {
        Write-Host "Opening coverage report..." -ForegroundColor Green
        Invoke-Item "$reportDir\index.html"
    }
}
catch {
    Write-Host "Error generating report: $_" -ForegroundColor Red
}
