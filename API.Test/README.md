# API.Test - Asset Management API Test Suite

Comprehensive test suite for the Asset Management API using xUnit, FluentAssertions, Moq, and WebApplicationFactory.

## 📋 Test Structure

### **Unit Tests**

#### **1. PasswordHasherTests** (`Services/PasswordHasherTests.cs`)
Tests the password hashing service:
- ✅ Generates non-empty hashed passwords
- ✅ Returns Base64 encoded strings
- ✅ Creates different hashes for same password (unique salt)
- ✅ Creates different hashes for different passwords
- ✅ Validates correct hash length

#### **2. OtpServiceTests** (`Services/OtpServiceTests.cs`)
Tests OTP generation, verification, and resend functionality:
- ✅ Generates valid 6-digit OTP codes
- ✅ Saves OTP to database with correct properties
- ✅ Deletes existing OTPs before generating new ones
- ✅ Verifies valid OTP successfully
- ✅ Rejects invalid OTP codes
- ✅ Handles expired OTPs
- ✅ Enforces maximum verification attempts
- ✅ Validates OTP existence before verification
- ✅ Resends OTP for valid pending users
- ✅ Rejects resend for non-existent users
- ✅ Handles expired registrations

#### **3. RegisterRepositoryTests** (`Repositories/RegisterRepositoryTests.cs`)
Tests user registration and activation:
- ✅ Creates pending user with valid data
- ✅ Validates input data before registration
- ✅ Prevents duplicate email registrations
- ✅ Deletes and recreates existing pending users
- ✅ Handles email sending failures
- ✅ Activates user with valid pending registration
- ✅ Rejects activation for non-existent users
- ✅ Removes expired registrations during activation

### **Integration Tests**

#### **UserControllerIntegrationTests** (`Integration/UserControllerIntegrationTests.cs`)
Tests complete API endpoints end-to-end:

**Registration Endpoint** (`POST /user/register`)
- ✅ Registers user with valid data
- ✅ Rejects invalid email format
- ✅ Rejects password mismatch

**Verification Endpoint** (`POST /user/verify`)
- ✅ Verifies OTP and activates account
- ✅ Moves data from pending_users to user_login
- ✅ Cleans up pending data after verification
- ✅ Rejects invalid OTP codes

**Resend OTP Endpoint** (`POST /user/resend-otp`)
- ✅ Resends OTP for valid pending user
- ✅ Rejects resend for non-existent user

## 🛠️ Test Technologies

- **xUnit**: Test framework
- **FluentAssertions**: Readable assertions
- **Moq**: Mocking dependencies
- **WebApplicationFactory**: Integration testing
- **EntityFrameworkCore.InMemory**: In-memory database for testing

## 🚀 Running Tests

### Standard Workflow:

#### Step 1: Run all tests (with TRX output)
```powershell
dotnet test --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

This command:
- Generates TRX file: `TestResults/test-results.trx`
- Generates coverage data: `TestResults/coverage.cobertura.xml`
- Creates `TestResults` directory if it doesn't exist

#### Step 2: Generate custom HTML report
```powershell
.\GenerateCustomTestReport.ps1
```

The custom report will be generated at: `TestResults/custom-test-report.html`

**Note:** The `GenerateCustomTestReport.ps1` script automatically creates the `TestResults` directory if it doesn't exist.

### Run specific test class (with results):
```powershell
dotnet test --filter "FullyQualifiedName~PasswordHasherTests" --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults

dotnet test --filter "FullyQualifiedName~OtpServiceTests" --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults

dotnet test --filter "FullyQualifiedName~RegisterRepositoryTests" --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults

dotnet test --filter "FullyQualifiedName~UserControllerIntegrationTests" --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

### Run with detailed output:
```powershell
dotnet test --verbosity detailed --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

### Run with code coverage:
```powershell
dotnet test --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx" --results-directory ./TestResults
```

## 📁 Test Results

All test results are generated in the `TestResults/` directory:

```
API.Test/
├── TestResults/
│   ├── test-results-[timestamp].trx      # Test results XML (for each run)
│   ├── custom-test-report.html           # Interactive custom HTML report
│   └── coverage.cobertura.xml            # Code coverage data
```

### Viewing Results:

1. **Custom HTML Report** (Recommended):
   - Open `TestResults/custom-test-report.html` in your web browser
   - Features: Interactive filtering (Passed/Failed/Skipped), test details, error messages

2. **TRX Files** (Visual Studio):
   - Open `.trx` files directly in Visual Studio
   - View detailed test results, stack traces, and timings

## 📂 Test Helpers

### **TestDbContextFactory** (`Helpers/TestDbContextFactory.cs`)
Creates in-memory database contexts for testing with proper configuration.

### **TestDataBuilder** (`Helpers/TestDataBuilder.cs`)
Provides builder methods for creating test data:
- `CreateValidUserRegisterDto()` - Valid user registration DTO
- `CreatePendingUser()` - Pending user entity
- `CreateOtpCode()` - OTP code entity
- `CreateUserLogin()` - User login entity

### **CustomWebApplicationFactory** (`Integration/CustomWebApplicationFactory.cs`)
Configures test environment for integration tests with in-memory database.

### **GenerateCustomTestReport.ps1** (`GenerateCustomTestReport.ps1`)
PowerShell script that transforms TRX test results into an interactive HTML report:
- Parses TRX XML results
- Converts test data to JavaScript
- Injects data into TestReportTemplate.html
- Automatically creates TestResults directory if needed
- Generates: `TestResults/custom-test-report.html`

## 📊 Test Coverage

### Components Tested:
- ✅ **PasswordHasher Service** - 100% coverage
- ✅ **OTP Service** - Complete flow coverage
- ✅ **RegisterRepository** - All scenarios covered
- ✅ **UserController API Endpoints** - All endpoints tested

### Test Scenarios:
- ✅ **Happy Path** - All success scenarios
- ✅ **Validation Failures** - Invalid input handling
- ✅ **Business Logic Failures** - Expired OTPs, max attempts, etc.
- ✅ **Edge Cases** - Non-existent users, expired registrations, etc.
- ✅ **Database Operations** - CRUD operations and cleanup

## 🔍 Test Patterns

### **Arrange-Act-Assert (AAA)**
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and mocks
    var input = CreateTestData();

    // Act - Execute the method under test
    var result = await _service.Method(input);

    // Assert - Verify the results
    result.Should().Be(expected);
}
```

### **Dependency Injection with Mocking**
Services are properly mocked using Moq:
```csharp
var mockService = new Mock<IService>();
mockService.Setup(s => s.Method()).ReturnsAsync(result);
```

### **In-Memory Database**
Tests use Entity Framework's in-memory database for isolation:
```csharp
var context = TestDbContextFactory.CreateInMemoryContext();
```

## 🎯 Best Practices Implemented

1. ✅ **Test Isolation** - Each test uses a unique database instance
2. ✅ **Descriptive Naming** - Test names clearly describe scenario and expectation
3. ✅ **Proper Cleanup** - `IDisposable` pattern for resource cleanup
4. ✅ **Comprehensive Coverage** - Happy path, edge cases, and error scenarios
5. ✅ **Integration Testing** - Full request/response pipeline testing
6. ✅ **Readable Assertions** - FluentAssertions for clear test failures
7. ✅ **Mocking Best Practices** - Only mock external dependencies
8. ✅ **Test Data Builders** - Reusable test data creation

## 📈 Next Steps

To extend test coverage:
1. Add tests for EmailService with mock SMTP
2. Add tests for additional controllers as they're created
3. Add performance tests for high-load scenarios
4. Add security tests for authentication/authorization
5. Add end-to-end tests with real database

## 🤝 Contributing

When adding new tests:
1. Follow the AAA pattern
2. Use descriptive test method names
3. Add tests for both success and failure scenarios
4. Keep tests isolated and independent
5. Use test helpers and builders for test data
6. Clean up resources properly
