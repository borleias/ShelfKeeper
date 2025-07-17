# ShelfKeeper

## Test Coverage

This project uses Coverlet for test coverage and ReportGenerator to create HTML reports.

### Running Tests with Coverage

To run tests with coverage and generate a report, execute the following command:

```powershell
# On Windows with PowerShell
powershell -ExecutionPolicy Bypass -File run-tests-with-coverage.ps1
```

After running, a coverage report will be available at `TestResults/Report/index.html`.

### Coverage in CI

Test coverage is also run as part of the CI pipeline using GitHub Actions.