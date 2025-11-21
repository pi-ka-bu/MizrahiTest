using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TestAPI.Configuration;
using TestAPI.Interfaces;
using TestAPI.Models;
using TestAPI.Services;
using static TestAPI.Models.MathRequest;

namespace TestAPI.Tests.Unit.Services;

public class CachedCalculatorServiceTests
{
    private readonly Mock<ICalculationService> _calculationServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IMetadataService> _metadataServiceMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<CachedCalculatorService>> _loggerMock;
    private readonly IOptions<CacheSettings> _cacheSettings;
    private readonly CachedCalculatorService _sut;

    public CachedCalculatorServiceTests()
    {
        _calculationServiceMock = new Mock<ICalculationService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _metadataServiceMock = new Mock<IMetadataService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<CachedCalculatorService>>();

        _cacheSettings = Options.Create(
            new CacheSettings { Type = CacheType.Memory, DurationSeconds = 60 }
        );

        _sut = new CachedCalculatorService(
            _calculationServiceMock.Object,
            _cacheServiceMock.Object,
            _metadataServiceMock.Object,
            _eventPublisherMock.Object,
            _cacheSettings,
            _loggerMock.Object
        );
    }

    #region Cache Hit Tests

    [Fact]
    public async Task AddAsync_WhenCacheHit_ReturnsCachedResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var cachedResponse = new MathResponse
        {
            Result = 15,
            Operation = "add",
            X = 10,
            Y = 5,
            FromCache = false,
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>("add:10:5"))
            .ReturnsAsync(cachedResponse);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(15);
        result.FromCache.Should().BeTrue();

        _calculationServiceMock.Verify(x => x.AddAsync(It.IsAny<MathRequest>()), Times.Never);
        _metadataServiceMock.Verify(
            x => x.GetOperationMetadataAsync(It.IsAny<string>()),
            Times.Never
        );
        _eventPublisherMock.Verify(
            x =>
                x.PublishCalculationEventAsync(
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task SubtractAsync_WhenCacheHit_DoesNotCallUnderlyingService()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        var cachedResponse = new MathResponse { Result = 5, Operation = "subtract" };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>("subtract:10:5"))
            .ReturnsAsync(cachedResponse);

        // Act
        await _sut.SubtractAsync(request);

        // Assert
        _calculationServiceMock.Verify(x => x.SubtractAsync(It.IsAny<MathRequest>()), Times.Never);
    }

    #endregion

    #region Cache Miss Tests

    [Fact]
    public async Task AddAsync_WhenCacheMiss_CallsUnderlyingService()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse
        {
            Result = 15,
            Operation = "add",
            X = 10,
            Y = 5,
            FromCache = false,
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("add"))
            .ReturnsAsync("metadata for add operation");

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(15);
        result.FromCache.Should().BeFalse();

        _calculationServiceMock.Verify(x => x.AddAsync(request), Times.Once);
        _metadataServiceMock.Verify(x => x.GetOperationMetadataAsync("add"), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenCacheMiss_CachesResult()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 15, Operation = "add" };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("add"))
            .ReturnsAsync("metadata for add");

        // Act
        await _sut.AddAsync(request);

        // Assert
        _cacheServiceMock.Verify(
            x => x.SetAsync("add:10:5", response, TimeSpan.FromSeconds(60)),
            Times.Once
        );
    }

    [Fact]
    public async Task AddAsync_WhenCacheMiss_PublishesEvent()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 15, Operation = "add" };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("add"))
            .ReturnsAsync("metadata for add");

        // Act
        await _sut.AddAsync(request);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishCalculationEventAsync("add", 10, 5, 15),
            Times.Once
        );
    }

    #endregion

    #region All Operations Tests

    [Fact]
    public async Task SubtractAsync_WhenCacheMiss_WorksCorrectly()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.SubtractEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 5 };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.SubtractAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("subtract"))
            .ReturnsAsync("metadata for subtract");

        // Act
        var result = await _sut.SubtractAsync(request);

        // Assert
        result.Result.Should().Be(5);
        _cacheServiceMock.Verify(
            x => x.SetAsync("subtract:10:5", response, It.IsAny<TimeSpan>()),
            Times.Once
        );
    }

    [Fact]
    public async Task MultiplyAsync_WhenCacheMiss_WorksCorrectly()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.MultiplyEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 50 };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.MultiplyAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("multiply"))
            .ReturnsAsync("metadata for multiply");

        // Act
        var result = await _sut.MultiplyAsync(request);

        // Assert
        result.Result.Should().Be(50);
        _eventPublisherMock.Verify(
            x => x.PublishCalculationEventAsync("multiply", 10, 5, 50),
            Times.Once
        );
    }

    [Fact]
    public async Task DivideAsync_WhenCacheMiss_WorksCorrectly()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 5,
        };

        var response = new MathResponse { Result = 2 };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.DivideAsync(request)).ReturnsAsync(response);

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("divide"))
            .ReturnsAsync("metadata for divide");

        // Act
        var result = await _sut.DivideAsync(request);

        // Assert
        result.Result.Should().Be(2);
        _cacheServiceMock.Verify(
            x => x.SetAsync("divide:10:5", response, It.IsAny<TimeSpan>()),
            Times.Once
        );
    }

    #endregion

    #region Cache Key Tests

    [Theory]
    [InlineData(10, 5, "add:10:5")]
    [InlineData(-10, 5, "add:-10:5")]
    [InlineData(10.5, 5.25, "add:10.5:5.25")]
    [InlineData(0, 0, "add:0:0")]
    public async Task AddAsync_GeneratesCorrectCacheKey(decimal x, decimal y, string expectedKey)
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = x,
            Y = y,
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(s => s.AddAsync(request)).ReturnsAsync(new MathResponse());

        _metadataServiceMock
            .Setup(m => m.GetOperationMetadataAsync("add"))
            .ReturnsAsync("metadata for add");

        // Act
        await _sut.AddAsync(request);

        // Assert
        _cacheServiceMock.Verify(c => c.GetAsync<MathResponse>(expectedKey), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task DivideAsync_WhenCalculationThrows_ExceptionPropagates()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.DivideEnum,
            X = 10,
            Y = 0,
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock
            .Setup(x => x.DivideAsync(request))
            .ThrowsAsync(new DivideByZeroException("Cannot divide by zero"));

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("divide"))
            .ReturnsAsync("metadata for divide");

        // Act
        Func<Task> act = async () => await _sut.DivideAsync(request);

        // Assert
        await act.Should().ThrowAsync<DivideByZeroException>().WithMessage("Cannot divide by zero");

        _cacheServiceMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<MathResponse>(), It.IsAny<TimeSpan>()),
            Times.Never
        );

        _eventPublisherMock.Verify(
            x =>
                x.PublishCalculationEventAsync(
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>(),
                    It.IsAny<decimal>()
                ),
            Times.Never
        );
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task AddAsync_WhenCacheHit_LogsCacheHit()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        var cachedResponse = new MathResponse { Result = 15, Operation = "add" };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>("add:10:5"))
            .ReturnsAsync(cachedResponse);

        // Act
        await _sut.AddAsync(request);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache HIT")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddAsync_WhenCacheMiss_LogsCacheMiss()
    {
        // Arrange
        var request = new MathRequest
        {
            Operation = OperationEnum.AddEnum,
            X = 10,
            Y = 5,
        };

        _cacheServiceMock
            .Setup(x => x.GetAsync<MathResponse>(It.IsAny<string>()))
            .ReturnsAsync((MathResponse)null);

        _calculationServiceMock.Setup(x => x.AddAsync(request)).ReturnsAsync(new MathResponse());

        _metadataServiceMock
            .Setup(x => x.GetOperationMetadataAsync("add"))
            .ReturnsAsync("metadata for add");

        // Act
        await _sut.AddAsync(request);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cache MISS")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion
}
