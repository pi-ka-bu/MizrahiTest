using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TestAPI.Models;
using static TestAPI.Models.MathRequest;

namespace TestAPI.Tests.Integration;

public class CalculatorApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CalculatorApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    #region Add Operation Tests

    [Fact]
    public async Task Calculate_AddOperation_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "add-test-001");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Should().NotBeNull();
        result.Result.Should().Be(15);
        result.Operation.Should().Be("add");
    }

    [Fact]
    public async Task Calculate_AddWithNegativeNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = -10,
            Y = -5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "add-test-002");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(-15);
    }

    #endregion

    #region Subtract Operation Tests

    [Fact]
    public async Task Calculate_SubtractOperation_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "subtract-test-001");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(5);
        result.Operation.Should().Be("subtract");
    }

    #endregion

    #region Multiply Operation Tests

    [Fact]
    public async Task Calculate_MultiplyOperation_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "multiply-test-001");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(50);
        result.Operation.Should().Be("multiply");
    }

    #endregion

    #region Divide Operation Tests

    [Fact]
    public async Task Calculate_DivideOperation_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "divide-test-001");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(2);
        result.Operation.Should().Be("divide");
    }

    [Fact]
    public async Task Calculate_DivideByZero_ReturnsBadRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        var httpRequest = CreateAuthorizedRequest(request, "divide-test-002");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("divide by zero");
    }

    #endregion

    #region Authentication Tests

    // Note: Authentication is bypassed in tests using TestAuthHandler
    // Real authentication is tested separately or in production environment

    #endregion

    #region Header Validation Tests

    [Fact]
    public async Task Calculate_WithoutArithmeticOpIdHeader_ReturnsBadRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/math")
        {
            Content = content,
        };
        // No need to add Authorization header - TestAuthHandler handles it automatically

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().ContainEquivalentOf("arithmeticOpId");
    }

    [Fact]
    public async Task Calculate_WithValidHeaders_ProcessesRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "valid-header-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Content Type Tests

    [Fact]
    public async Task Calculate_WithJsonContentType_ReturnsJson()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "json-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Calculate_WithDecimalNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10.5m,
            Y = 5.25m,
        };

        var httpRequest = CreateAuthorizedRequest(request, "decimal-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(15.75m);
    }

    [Fact]
    public async Task Calculate_WithZeroValues_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 0,
        };

        var httpRequest = CreateAuthorizedRequest(request, "zero-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(0);
    }

    [Fact]
    public async Task Calculate_WithLargeNumbers_ReturnsCorrectResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 999999.99m,
            Y = 999999.99m,
        };

        var httpRequest = CreateAuthorizedRequest(request, "large-number-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Response Validation

    [Fact]
    public async Task Calculate_ReturnsResponseWithAllRequiredFields()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var httpRequest = CreateAuthorizedRequest(request, "response-fields-test");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Should().NotBeNull();
        result.RequestId.Should().NotBeNullOrEmpty();
        result.Operation.Should().NotBeNullOrEmpty();
        result.X.Should().Be(10);
        result.Y.Should().Be(5);
        result.Result.Should().Be(15);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Helper Methods

    private HttpRequestMessage CreateAuthorizedRequest(MathRequest request, string operationId)
    {
        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/math")
        {
            Content = content,
        };

        // No need to add Authorization header - TestAuthHandler handles it automatically
        httpRequest.Headers.Add("X-ArithmeticOp-ID", operationId);

        return httpRequest;
    }

    #endregion

    #region Multiple Operations Tests

    [Theory]
    [InlineData(OperationEnum.AddEnum, 10, 5, 15)]
    [InlineData(OperationEnum.SubtractEnum, 10, 5, 5)]
    [InlineData(OperationEnum.MultiplyEnum, 10, 5, 50)]
    [InlineData(OperationEnum.DivideEnum, 10, 5, 2)]
    public async Task Calculate_AllOperations_ReturnCorrectResults(
        OperationEnum operation,
        decimal x,
        decimal y,
        decimal expectedResult
    )
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = operation,
            X = x,
            Y = y,
        };

        var httpRequest = CreateAuthorizedRequest(request, $"theory-test-{operation}");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MathResponse>();
        result.Result.Should().Be(expectedResult);
    }

    #endregion
}
