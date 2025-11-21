using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TestAPI.Models;
using TestAPI.Services;
using static TestAPI.Models.MathRequest;

namespace TestAPI.Tests.Unit.Services;

public class CalculationServiceTests
{
    private readonly Mock<ILogger<CalculationService>> _loggerMock;
    private readonly CalculationService _sut;

    public CalculationServiceTests()
    {
        _loggerMock = new Mock<ILogger<CalculationService>>();
        _sut = new CalculationService(_loggerMock.Object);
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithPositiveNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(15);
        result.Operation.Should().Be("add");
        result.X.Should().Be(10);
        result.Y.Should().Be(5);
        result.FromCache.Should().BeFalse();
        result.RequestId.Should().NotBeNullOrEmpty();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AddAsync_WithNegativeNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = -10,
            Y = -5,
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Result.Should().Be(-15);
        result.Operation.Should().Be("add");
    }

    [Fact]
    public async Task AddAsync_WithZero_ReturnsCorrectSum()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 0,
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Result.Should().Be(10);
    }

    [Fact]
    public async Task AddAsync_WithDecimalNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10.5m,
            Y = 5.25m,
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Result.Should().Be(15.75m);
    }

    #endregion

    #region SubtractAsync Tests

    [Fact]
    public async Task SubtractAsync_WithPositiveNumbers_ReturnsCorrectDifference()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.SubtractAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(5);
        result.Operation.Should().Be("subtract");
        result.X.Should().Be(10);
        result.Y.Should().Be(5);
        result.FromCache.Should().BeFalse();
    }

    [Fact]
    public async Task SubtractAsync_WithNegativeResult_ReturnsCorrectDifference()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 5,
            Y = 10,
        };

        // Act
        var result = await _sut.SubtractAsync(request);

        // Assert
        result.Result.Should().Be(-5);
    }

    [Fact]
    public async Task SubtractAsync_WithZero_ReturnsCorrectDifference()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 0,
        };

        // Act
        var result = await _sut.SubtractAsync(request);

        // Assert
        result.Result.Should().Be(10);
    }

    [Fact]
    public async Task SubtractAsync_WithDecimalNumbers_ReturnsCorrectDifference()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10.75m,
            Y = 5.25m,
        };

        // Act
        var result = await _sut.SubtractAsync(request);

        // Assert
        result.Result.Should().Be(5.5m);
    }

    #endregion

    #region MultiplyAsync Tests

    [Fact]
    public async Task MultiplyAsync_WithPositiveNumbers_ReturnsCorrectProduct()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.MultiplyAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(50);
        result.Operation.Should().Be("multiply");
        result.X.Should().Be(10);
        result.Y.Should().Be(5);
        result.FromCache.Should().BeFalse();
    }

    [Fact]
    public async Task MultiplyAsync_WithNegativeNumbers_ReturnsCorrectProduct()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = -10,
            Y = 5,
        };

        // Act
        var result = await _sut.MultiplyAsync(request);

        // Assert
        result.Result.Should().Be(-50);
    }

    [Fact]
    public async Task MultiplyAsync_WithZero_ReturnsZero()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 0,
        };

        // Act
        var result = await _sut.MultiplyAsync(request);

        // Assert
        result.Result.Should().Be(0);
    }

    [Fact]
    public async Task MultiplyAsync_WithDecimalNumbers_ReturnsCorrectProduct()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10.5m,
            Y = 2m,
        };

        // Act
        var result = await _sut.MultiplyAsync(request);

        // Assert
        result.Result.Should().Be(21m);
    }

    #endregion

    #region DivideAsync Tests

    [Fact]
    public async Task DivideAsync_WithPositiveNumbers_ReturnsCorrectQuotient()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 5,
        };

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(2);
        result.Operation.Should().Be("divide");
        result.X.Should().Be(10);
        result.Y.Should().Be(5);
        result.FromCache.Should().BeFalse();
    }

    [Fact]
    public async Task DivideAsync_WithDecimalResult_ReturnsCorrectQuotient()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 4,
        };

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Result.Should().Be(2.5m);
    }

    [Fact]
    public async Task DivideAsync_WithNegativeNumbers_ReturnsCorrectQuotient()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = -10,
            Y = 5,
        };

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Result.Should().Be(-2);
    }

    [Fact]
    public async Task DivideAsync_WithZeroDividend_ReturnsZero()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 0,
            Y = 5,
        };

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Result.Should().Be(0);
    }

    [Fact]
    public async Task DivideAsync_WithZeroDivisor_ThrowsDivideByZeroException()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        // Act
        Func<Task> act = async () => await _sut.DivideAsync(request);

        // Assert
        await act.Should().ThrowAsync<DivideByZeroException>().WithMessage("Cannot divide by zero");
    }

    [Fact]
    public async Task DivideAsync_WithDecimalNumbers_ReturnsCorrectQuotient()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10.5m,
            Y = 2m,
        };

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Result.Should().Be(5.25m);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task AddAsync_LogsDebugMessage()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        // Act
        await _sut.AddAsync(request);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("addition")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DivideAsync_WithZeroDivisor_LogsWarning()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        // Act
        try
        {
            await _sut.DivideAsync(request);
        }
        catch (DivideByZeroException)
        {
            // Expected
        }

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("division by zero")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion
}
