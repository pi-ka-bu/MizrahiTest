# TestAPI - Calculator Service

A modern, production-ready ASP.NET Core 8.0 REST API for performing arithmetic operations with comprehensive features including caching, event publishing, and external metadata integration.

## Features

- **Arithmetic Operations**: Add, Subtract, Multiply, Divide
- **JWT Authentication**: Secure API endpoints with Bearer token authentication
- **Distributed Caching**: Redis or in-memory cache support with configurable strategies
- **Event Publishing**: Kafka integration for operation event streaming
- **Metadata Service**: External API integration for operation metadata
- **Request Correlation**: X-ArithmeticOp-ID header for request tracing
- **API Documentation**: Interactive Swagger/OpenAPI documentation
- **Docker Support**: Full containerization with docker-compose orchestration
- **Comprehensive Testing**: Unit and integration tests with 95% coverage target

## Architecture

### Technology Stack

- **.NET 8.0** - Latest LTS framework
- **ASP.NET Core** - Modern minimal hosting model
- **Redis** - Distributed caching (optional)
- **Apache Kafka** - Event streaming platform
- **Swagger/OpenAPI** - API documentation
- **Mockoon** - External service mocking
- **xUnit** - Testing framework
- **Moq & FluentAssertions** - Test utilities

### Design Patterns

- **Decorator Pattern**: `CachedCalculationService` wraps `CalculationService`
- **Repository Pattern**: Service layer abstraction
- **Dependency Injection**: Constructor injection throughout
- **Middleware Pipeline**: Error handling, logging, authentication
- **Options Pattern**: Configuration management

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- (Optional) Docker Desktop for containerized deployment
- (Optional) Redis and Kafka for production features

### Running Locally (Development Mode)

Development mode uses in-memory caching and mock services - no Redis/Kafka required:

**Option 1: Using the solution file (recommended)**
```bash
dotnet run --project src/TestAPI/TestAPI.csproj
```

**Option 2: From the TestAPI directory**
```bash
cd src/TestAPI
dotnet run
```

The API will be available at:

- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:5001/swagger

### Building the Entire Solution

To build all projects (API + Client + Tests):

```bash
dotnet build MizrahiTest.sln
```

### Running with Docker Compose

Full infrastructure setup with Redis, Kafka, and all services:

```bash
docker-compose up
```

Services:

- **TestAPI**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Redis Commander**: http://localhost:8091
- **Kafka UI**: http://localhost:8090
- **Mockoon**: http://localhost:3000

## API Usage

You can interact with the API in two ways:

1. **Using HTTP/cURL** (shown below)
2. **Using the C# Client Library** (see [CLIENT-INTEGRATION.md](CLIENT-INTEGRATION.md))

### Using HTTP/cURL

#### 1. Generate JWT Token

```bash
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "testpass"}'
```

Response:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-11-22T10:30:00Z"
}
```

#### 2. Perform Calculations

```bash
curl -X POST http://localhost:5000/api/math \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "X-ArithmeticOp-ID: 12345" \
  -H "Content-Type: application/json" \
  -d '{
    "operation": "add",
    "x": 10,
    "y": 5
  }'
```

Response:

```json
{
  "result": 15,
  "operation": "add",
  "metadata": {
    "operation": "add",
    "description": "Performs addition of two numbers",
    "complexity": "O(1)",
    "examples": ["2 + 3 = 5", "10 + 5 = 15"]
  }
}
```

### Supported Operations

- `add` - Addition
- `subtract` - Subtraction
- `multiply` - Multiplication
- `divide` - Division (throws error on division by zero)

### Required Headers

- `Authorization`: Bearer {JWT_TOKEN}
- `X-ArithmeticOp-ID`: Any non-empty string for request correlation

### Using the C# Client Library

The project includes a type-safe C# client library for easy integration:

```csharp
using TestAPI.Client.Api;
using TestAPI.Client.Client;
using TestAPI.Client.Model;

// Configure the client
var config = new Configuration { BasePath = "http://localhost:5000" };
config.ApiKey.Add("Authorization", "Bearer YOUR_JWT_TOKEN");

// Create API instances
var authApi = new AuthApi(config);
var calculationApi = new CalculationApi(config);

// Get token
var tokenRequest = new TestAPIControllersTokenRequest { Username = "testuser" };
authApi.ApiAuthTokenPost(tokenRequest);

// Perform calculation
var mathRequest = new TestAPIModelsMathRequest
{
    Operation = TestAPIModelsMathRequestOperationEnum.NUMBER_0, // Add
    X = 10,
    Y = 5
};
calculationApi.ApiMathPost(mathRequest, "operation-123");
```

For complete client library documentation, see [CLIENT-INTEGRATION.md](CLIENT-INTEGRATION.md)

## Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json`:

### Cache Settings

```json
{
  "CacheSettings": {
    "Type": "InMemory", // or "Redis"
    "RedisConnectionString": "localhost:6379",
    "DefaultExpirationMinutes": 60
  }
}
```

### Kafka Settings

```json
{
  "KafkaSettings": {
    "BootstrapServers": "localhost:9092",
    "TopicName": "calculator-operations",
    "Enabled": false // Set to true in production
  }
}
```

### JWT Settings

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-here-min-32-chars",
    "Issuer": "TestAPI",
    "Audience": "TestAPIUsers",
    "ExpirationMinutes": 60
  }
}
```

## Testing

### Run All Tests

**Option 1: Using the solution file (recommended)**
```bash
dotnet test MizrahiTest.sln
```

**Option 2: From the test project directory**
```bash
cd src/TestAPI.Tests
dotnet test
```

### Run with Coverage

```bash
dotnet test MizrahiTest.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Structure

```
TestAPI.Tests/
├── Unit/
│   ├── Services/
│   │   ├── CalculationServiceTests.cs
│   │   ├── CachedCalculationServiceTests.cs
│   │   └── MetadataServiceTests.cs
│   └── Controllers/
│       └── CalculationControllerTests.cs
└── Integration/
    └── CalculatorApiTests.cs
```

## Project Structure

```
MizrahiTest/
├── MizrahiTest.sln           # Solution file (manages all projects)
├── src/
│   ├── TestAPI/              # Main API project (Server)
│   │   ├── Controllers/      # API endpoints
│   │   ├── Services/         # Business logic
│   │   ├── Models/           # Request/response models
│   │   ├── Interfaces/       # Service contracts
│   │   ├── Middlewares/      # Custom middleware
│   │   ├── Security/         # Authentication handlers
│   │   ├── Configuration/    # Configuration classes
│   │   └── Filters/          # Swagger filters
│   ├── TestAPI.Client/       # C# Client Library (generated from Swagger)
│   │   ├── Api/              # API client classes
│   │   ├── Client/           # HTTP client infrastructure
│   │   └── Model/            # Request/response models
│   └── TestAPI.Tests/        # Test project
├── mockoon/                  # Mock service configs
├── docker-compose.yml        # Docker orchestration
├── CLIENT-INTEGRATION.md     # Client library usage guide
└── README.md                 # This file
```

## Development

### Adding New Operations

1. Add operation enum to `Models/MathRequest.cs`
2. Implement method in `Services/CalculationService.cs`
3. Add interface method in `Interfaces/ICalculationService.cs`
4. Update controller switch in `Controllers/CalculationController.cs`
5. Add tests in `TestAPI.Tests`

### Building for Production

```bash
cd src/TestAPI
dotnet publish -c Release -o ./publish
```

## Monitoring & Observability

### Logging

Structured logging using `Microsoft.Extensions.Logging`:

- Request/Response logging via middleware
- Operation tracking with correlation IDs
- Error logging with stack traces

### Health Checks

```bash
curl http://localhost:5000/health
```

### Kafka Event Monitoring

Access Kafka UI at http://localhost:8090 to view published operation events.

### Redis Cache Monitoring

Access Redis Commander at http://localhost:8091 to inspect cached values.

## Troubleshooting

### Port Already in Use

```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/macOS
lsof -i :5000
kill -9 <PID>
```

### Redis Connection Failed

Development mode automatically falls back to in-memory cache. For production:

```bash
docker run -p 6379:6379 redis:7-alpine
```

### Kafka Connection Issues

Ensure Zookeeper is running before Kafka:

```bash
docker-compose up zookeeper
docker-compose up kafka
```

## API Endpoints

| Method | Endpoint          | Description         | Auth Required |
| ------ | ----------------- | ------------------- | ------------- |
| POST   | `/api/auth/token` | Generate JWT token  | No            |
| POST   | `/api/math`       | Perform calculation | Yes           |
| GET    | `/swagger`        | API documentation   | No            |

## Security

- JWT tokens expire after 60 minutes (configurable)
- HTTPS redirect in production
- Input validation on all endpoints
- X-ArithmeticOp-ID header validation
- Error messages don't expose internal details

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is for educational and demonstration purposes only.
