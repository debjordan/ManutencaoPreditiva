using IoTDataApi.Data;
using IoTDataApi.Models;
using IoTDataApi.Services;
using IoTDataApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IoTDataApi.Tests.UnitTests.Services;

public class IoTDataServiceMoqTests
{
    private readonly Mock<IoTDataContext> _mockContext;
    private readonly IoTDataService _service;
    private readonly List<IoTData> _testData;

    public IoTDataServiceMoqTests()
    {
        _testData = TestDataHelper.GetTestIoTData();

        var mockSet = CreateMockDbSet(_testData);

        _mockContext = new Mock<IoTDataContext>();
        _mockContext.Setup(c => c.IoTData).Returns(mockSet.Object);

        _service = new IoTDataService(_mockContext.Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet;
    }

    [Fact]
    public async Task GetAllDataAsync_CallsDbContextCorrectly()
    {
        // Act
        var result = await _service.GetAllDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        _mockContext.Verify(m => m.IoTData, Times.Once);
    }

    [Fact]
    public async Task GetMachineDataAsync_CallsDbContextWithCorrectFilter()
    {
        // Arrange
        var machineId = "001";

        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        _mockContext.Verify(m => m.IoTData, Times.Once);
    }

    [Fact]
    public async Task GetAllDataAsync_WithLimit_CallsDbContextWithCorrectLimit()
    {
        // Act
        var result = await _service.GetAllDataAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockContext.Verify(m => m.IoTData, Times.Once);
    }
}
