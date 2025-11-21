using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TestAPI.Controllers;
using TestAPI.Interfaces;
using TestAPI.Models;
using static TestAPI.Models.MathRequest;

namespace TestAPI.Tests.Unit.Controllers;

public class CalculationControllerTests
{
    private readonly Mock<ICalculationService> _calculationServiceMock;
    private readonly Mock<ILogger<CalculationController>> _loggerMock;
    private readonly CalculationController _sut;

    public CalculationControllerTests()
    {
        _calculationServiceMock = new Mock<ICalculationService>();
        _loggerMock = new Mock<ILogger<CalculationController>>();
        _sut = new CalculationController(_calculationServiceMock.Object, _loggerMock.Object);
    }

    #region Successful Calculations

    [Fact]
    public async Task Calculate_WithAddOperation_ReturnsOkResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 15, Operation = "add" };

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Calculate(request, "test-id-123");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Calculate_WithSubtractOperation_ReturnsOkResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 5, Operation = "subtract" };

        _calculationServiceMock.Setup(x => x.SubtractAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Calculate(request, "test-id-123");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Calculate_WithMultiplyOperation_ReturnsOkResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 50, Operation = "multiply" };

        _calculationServiceMock.Setup(x => x.MultiplyAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Calculate(request, "test-id-123");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Calculate_WithDivideOperation_ReturnsOkResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 2, Operation = "divide" };

        _calculationServiceMock.Setup(x => x.DivideAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Calculate(request, "test-id-123");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Header Validation

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Calculate_WithMissingArithmeticOpId_ReturnsBadRequest(string arithmeticOpId)
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.Calculate(request, arithmeticOpId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().NotBeNull();
        badRequestResult.Value.ToString().Should().Contain("X-ArithmeticOp-ID");
    }

    [Fact]
    public async Task Calculate_WithValidArithmeticOpId_ProcessesRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock
            .Setup(x => x.AddAsync(request))
            .ReturnsAsync(new MathResponse { Result = 15 });

        // Act
        var result = await _sut.Calculate(request, "valid-id");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Model Validation

    [Fact]
    public async Task Calculate_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _sut.ModelState.AddModelError("Operation", "Invalid operation");

        // Act
        var result = await _sut.Calculate(request, "test-id");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().BeOfType<SerializableError>();
    }

    #endregion

    #region Exception Handling

    [Fact]
    public async Task Calculate_WithDivideByZero_ReturnsBadRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        _calculationServiceMock
            .Setup(x => x.DivideAsync(request))
            .ThrowsAsync(new DivideByZeroException("Cannot divide by zero"));

        // Act
        var result = await _sut.Calculate(request, "test-id");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.ToString().Should().Contain("Cannot divide by zero");
    }

    [Fact]
    public async Task Calculate_WithInvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = (OperationEnum)999,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.Calculate(request, "test-id");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.ToString().Should().Contain("Unknown operation");
    }

    [Fact]
    public async Task Calculate_WithUnexpectedException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock
            .Setup(x => x.AddAsync(request))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _sut.Calculate(request, "test-id");

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.ToString().Should().Contain("Unexpected error");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task Calculate_WithMissingHeader_LogsWarning()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        // Act
        await _sut.Calculate(request, "");

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString().Contains("missing X-ArithmeticOp-ID")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Calculate_WithValidRequest_LogsInformation()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock
            .Setup(x => x.AddAsync(request))
            .ReturnsAsync(new MathResponse { Result = 15 });

        // Act
        await _sut.Calculate(request, "test-id-123");

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Processing")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Calculate_WithDivideByZero_LogsWarning()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        _calculationServiceMock
            .Setup(x => x.DivideAsync(request))
            .ThrowsAsync(new DivideByZeroException());

        // Act
        await _sut.Calculate(request, "test-id");

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Division by zero")),
                    It.IsAny<DivideByZeroException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Calculate_WithUnexpectedException_LogsError()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock
            .Setup(x => x.AddAsync(request))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        await _sut.Calculate(request, "test-id");

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Service Call Verification

    [Fact]
    public async Task Calculate_CallsCorrectServiceMethod_ForAddOperation()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(new MathResponse());

        // Act
        await _sut.Calculate(request, "test-id");

        // Assert
        _calculationServiceMock.Verify(x => x.AddAsync(request), Times.Once);
        _calculationServiceMock.Verify(x => x.SubtractAsync(It.IsAny<MathRequest>()), Times.Never);
        _calculationServiceMock.Verify(x => x.MultiplyAsync(It.IsAny<MathRequest>()), Times.Never);
        _calculationServiceMock.Verify(x => x.DivideAsync(It.IsAny<MathRequest>()), Times.Never);
    }

    [Fact]
    public async Task Calculate_CallsCorrectServiceMethod_ForSubtractOperation()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        _calculationServiceMock
            .Setup(x => x.SubtractAsync(request))
            .ReturnsAsync(new MathResponse());

        // Act
        await _sut.Calculate(request, "test-id");

        // Assert
        _calculationServiceMock.Verify(x => x.SubtractAsync(request), Times.Once);
        _calculationServiceMock.Verify(x => x.AddAsync(It.IsAny<MathRequest>()), Times.Never);
    }

    #endregion
}
