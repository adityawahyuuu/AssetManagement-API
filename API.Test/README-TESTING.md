# API Test Suite

This directory contains integration tests and unit tests for the Asset Management API.

## 📋 Test Configuration

The test suite is configured to:
- ✅ Save test results to files (TRX, HTML, coverage reports)
- ✅ Continue execution even if tests fail (exit code 0)
- ✅ Generate code coverage reports
- ✅ Output detailed console logs

## 🚀 Running Tests

### Option 1: Using Test Scripts (Recommended)

#### On Linux/Mac:
```bash
cd API.Test
./run-tests.sh
```

#### On Windows (PowerShell):
```powershell
cd API.Test
.\run-tests.ps1
```

**Benefits:**
- Automatically creates timestamped result files
- Always exits with code 0 (won't fail CI/CD pipelines)
- Shows summary of test results
- Lists all generated files

### Option 2: Using dotnet CLI Directly

#### Run all tests with results saved:
```bash
dotnet test --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

#### Run with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

#### Run with custom settings:
```bash
dotnet test --settings test.runsettings
```

#### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~UserControllerExtendedTests"
```

#### Run tests matching a name pattern:
```bash
dotnet test --filter "Name~Login"
```

## 📁 Test Results Location

All test results are saved in the `TestResults/` directory:

```
API.Test/
├── TestResults/
│   ├── test-results-20250110_143022.trx    # Test results XML
│   ├── coverage.cobertura.xml               # Code coverage
│   └── [timestamp]/                         # Additional coverage data
├── test.runsettings                         # Test configuration
├── run-tests.sh                             # Linux/Mac test script
└── run-tests.ps1                            # Windows test script
```

## 📊 Viewing Test Results

### TRX Files (XML Test Results)

**Visual Studio:**
- Open the `.trx` file directly in Visual Studio
- View detailed test results, stack traces, and timings

**Convert to HTML:**
```bash
# Install the converter tool
dotnet tool install -g trx2html

# Convert TRX to HTML
trx2html TestResults/test-results-[timestamp].trx
```

### Code Coverage Reports

**View Cobertura XML:**
- Use tools like ReportGenerator, Codecov, or Coveralls
- Integrate with CI/CD pipelines

**Generate HTML Coverage Report:**
```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:Html

# Open the report
open TestResults/CoverageReport/index.html
```

## 🔧 Test Configuration (test.runsettings)

The `test.runsettings` file configures:

- **Parallel Execution:** Tests run in parallel for faster execution
- **Result Formats:** TRX, HTML, and coverage formats
- **Target Framework:** net8.0
- **Coverage Settings:** Includes/excludes for code coverage

### Modifying Settings

Edit `test.runsettings` to customize:
- Maximum CPU threads
- Logging verbosity
- Coverage include/exclude patterns
- Parallel execution settings

## 🧪 Test Categories

### Integration Tests (`Integration/`)
- Test full request/response cycles
- Use in-memory database
- Test authentication flows
- Validate API contracts

**Example:**
- `UserControllerExtendedTests.cs` - User authentication and password reset
- `AssetControllerIntegrationTests.cs` - Asset CRUD operations
- `RoomControllerIntegrationTests.cs` - Room management

### Unit Tests (`Services/`, `Repositories/`)
- Test individual components in isolation
- Use mocks for dependencies
- Validate business logic

**Example:**
- `OtpServiceTests.cs` - OTP generation and verification
- `RegisterRepositoryTests.cs` - User registration logic

## 🎯 Best Practices

### Writing Tests

1. **Use Descriptive Names:**
   ```csharp
   [Fact]
   public async Task Login_WithValidCredentials_ShouldReturnToken()
   ```

2. **Follow Arrange-Act-Assert Pattern:**
   ```csharp
   // Arrange
   var loginDto = new LoginDto { Email = "test@example.com", Password = "pass" };

   // Act
   var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

   // Assert
   response.StatusCode.Should().Be(HttpStatusCode.OK);
   ```

3. **Use FluentAssertions:**
   ```csharp
   result.Should().NotBeNull();
   result.Token.Should().NotBeNullOrEmpty();
   response.StatusCode.Should().Be(HttpStatusCode.OK);
   ```

4. **Clean Up Resources:**
   ```csharp
   public void Dispose()
   {
       _dbContext?.Dispose();
   }
   ```

### Running Tests in CI/CD

The test scripts always exit with code 0, making them suitable for CI/CD:

**GitHub Actions:**
```yaml
- name: Run Tests
  run: |
    cd API.Test
    ./run-tests.sh

- name: Upload Test Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: API.Test/TestResults/
```

**Azure Pipelines:**
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    arguments: '--logger trx --results-directory ./TestResults'
    workingDirectory: 'API.Test'
  continueOnError: true
```

## 📝 Troubleshooting

### Tests Not Running

**Check .NET SDK:**
```bash
dotnet --version  # Should be 8.0 or higher
```

**Restore packages:**
```bash
dotnet restore
```

**Build project:**
```bash
dotnet build
```

### In-Memory Database Issues

If you see BitArray errors, ensure `ApplicationDbContext.cs` has the in-memory converter:
```csharp
var isInMemory = Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
if (isInMemory) {
    entity.Property(e => e.is_confirmed).HasConversion(...);
}
```

### Test Results Not Generated

Ensure the `TestResults` directory is writable:
```bash
mkdir -p TestResults
chmod 755 TestResults
```

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Code Coverage with Coverlet](https://github.com/coverlet-coverage/coverlet)

## 🤝 Contributing

When adding new tests:
1. Follow existing naming conventions
2. Add test to appropriate directory (Integration/ or unit test folder)
3. Ensure test is isolated and doesn't depend on other tests
4. Update this README if adding new test categories
