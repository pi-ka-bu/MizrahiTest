# SwaggerHub Client Integration

## Overview

This document describes the integration of the SwaggerHub-generated C# client into the MizrahiTest project.

## Project Structure

The repository now contains three projects managed by a single solution file:

```
MizrahiTest/
├── MizrahiTest.sln              # Solution file managing all projects
├── src/
│   ├── TestAPI/                 # ASP.NET Core API (Server)
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Models/
│   │   └── TestAPI.csproj
│   ├── TestAPI.Client/          # Generated C# Client Library
│   │   ├── Api/
│   │   │   ├── AuthApi.cs
│   │   │   └── CalculationApi.cs
│   │   ├── Client/
│   │   │   ├── ApiClient.cs
│   │   │   ├── Configuration.cs
│   │   │   └── ...
│   │   ├── Model/
│   │   │   ├── TestAPIModelsMathRequest.cs
│   │   │   └── ...
│   │   └── TestAPI.Client.csproj
│   └── TestAPI.Tests/           # Unit & Integration Tests
│       └── TestAPI.Tests.csproj
├── docker-compose.yml
└── README.md
```

## Changes Made

### 1. Client Code Integration
- Copied generated client from `csharp-client-generated/src/IO.Swagger` to `src/TestAPI.Client`
- Updated target framework from .NET 4.7.1 to .NET 8.0 for consistency

### 2. Namespace Refactoring
All references updated from `IO.Swagger.*` to `TestAPI.Client.*`:
- `IO.Swagger.Api` → `TestAPI.Client.Api`
- `IO.Swagger.Client` → `TestAPI.Client.Client`
- `IO.Swagger.Model` → `TestAPI.Client.Model`

### 3. Solution File Created
Created `MizrahiTest.sln` that includes:
- `src/TestAPI/TestAPI.csproj`
- `src/TestAPI.Client/TestAPI.Client.csproj`
- `src/TestAPI.Tests/TestAPI.Tests.csproj`

### 4. Updated .gitignore
- Added `csharp-client-generated/` to ignore the original download folder
- Kept generated code in `src/TestAPI.Client/` tracked in git

## Using the Client Library

### Basic Usage

```csharp
using TestAPI.Client.Api;
using TestAPI.Client.Client;
using TestAPI.Client.Model;

// Configure the client
var config = new Configuration
{
    BasePath = "https://localhost:5001"
};

// Add authentication
config.ApiKey.Add("Authorization", "Bearer YOUR_JWT_TOKEN");

// Create API instance
var calculationApi = new CalculationApi(config);

// Make a request
var request = new TestAPIModelsMathRequest
{
    Operation = TestAPIModelsMathRequestOperationEnum.NUMBER_0, // Add
    X = 10,
    Y = 5
};

calculationApi.ApiMathPost(request, "unique-operation-id");
```

### Async Usage

```csharp
// All API methods have async versions
await calculationApi.ApiMathPostAsync(request, "unique-operation-id");
```

### Authentication Example

```csharp
// Get authentication token first
var authApi = new AuthApi(config);
var tokenRequest = new TestAPIControllersTokenRequest
{
    Username = "testuser"
};

var tokenResponse = authApi.ApiAuthTokenPost(tokenRequest);
// Extract token from response and use for subsequent calls
```

## Building the Solution

```bash
# Build all projects
dotnet build MizrahiTest.sln

# Run tests
dotnet test MizrahiTest.sln

# Run the API
dotnet run --project src/TestAPI/TestAPI.csproj
```

## NuGet Dependencies

The client library depends on:
- **RestSharp** (112.1.0) - HTTP client library
- **Newtonsoft.Json** (13.0.1) - JSON serialization
- **JsonSubTypes** (1.2.0) - Polymorphic JSON deserialization

## Distribution Options

### Option 1: Direct Project Reference
Add a project reference in consuming applications:

```xml
<ProjectReference Include="..\TestAPI.Client\TestAPI.Client.csproj" />
```

### Option 2: NuGet Package
Package the client as a NuGet package:

```bash
cd src/TestAPI.Client
dotnet pack -c Release
```

Then publish to:
- Internal NuGet feed
- Public nuget.org
- GitHub Packages

### Option 3: DLL Reference
Build and distribute the DLL directly:

```bash
dotnet build src/TestAPI.Client/TestAPI.Client.csproj -c Release
# DLL location: src/TestAPI.Client/bin/Release/net8.0/TestAPI.Client.dll
```

## Updating the Client

When the API changes:

1. **Update OpenAPI spec** - regenerate `swagger-spec.json`:
   ```bash
   dotnet run --project src/TestAPI
   curl http://localhost:5000/swagger/v1/swagger.json > swagger-spec.json
   ```

2. **Re-import to SwaggerHub** - upload the new spec

3. **Regenerate client code** - download from SwaggerHub

4. **Replace client files**:
   ```bash
   # Backup first
   cp -r src/TestAPI.Client src/TestAPI.Client.backup

   # Copy new generated code
   cp -r csharp-client-generated/src/IO.Swagger/* src/TestAPI.Client/

   # Re-apply customizations (namespace changes, .csproj updates)
   ```

5. **Rebuild and test**:
   ```bash
   dotnet build MizrahiTest.sln
   dotnet test MizrahiTest.sln
   ```

## Security

The client library uses secure, up-to-date dependencies:
- ✅ RestSharp 112.1.0 (no known vulnerabilities)
- ✅ Newtonsoft.Json 13.0.1
- ✅ JsonSubTypes 1.2.0

## Integration with Tests

You can now use the client library in your integration tests for cleaner, type-safe API calls:

```csharp
// Before: Manual HttpClient
var request = new HttpRequestMessage(HttpMethod.Post, "/api/math");
request.Headers.Add("Authorization", "Bearer " + token);
// ... manual JSON serialization

// After: Using generated client
var client = new CalculationApi(config);
await client.ApiMathPostAsync(mathRequest, operationId);
```

## Support

For issues with:
- **The API**: Check `src/TestAPI/` source code
- **The client library**: Regenerate from SwaggerHub or check `src/TestAPI.Client/`
- **Integration**: See this document or contact the development team
