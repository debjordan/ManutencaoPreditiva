using IoTDataApi.Data;
using IoTDataApi.Services;
using IoTDataApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IoTDataApi.Tests.UnitTests.Services;

public class IoTDataServiceTests : IDisposable
{
    private readonly IoTDataContext _context;
    private readonly IoTDataService _service;

    public IoTDataServiceTests()
    {
        _context = DatabaseHelper.CreateInMemoryContext();
        _service = new IoTDataService(_context);
    }

    [Fact]
    public async Task GetAllDataAsync_ReturnsAllData()
    {
        // Act
        var result = await _service.GetAllDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task GetAllDataAsync_WithLimit_ReturnsLimitedData()
    {
        // Act
        var result = await _service.GetAllDataAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllDataAsync_ReturnsDataOrderedByReceivedAtDescending()
    {
        // Act
        var result = (await _service.GetAllDataAsync()).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.True(DateTime.Parse(result[0].ReceivedAt) >= DateTime.Parse(result[1].ReceivedAt));
    }

    [Fact]
    public async Task GetAllDataAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var emptyContext = DatabaseHelper.CreateEmptyInMemoryContext("EmptyDB");
        var emptyService = new IoTDataService(emptyContext);

        // Act
        var result = await emptyService.GetAllDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMachineDataAsync_WithValidMachineId_ReturnsFilteredData()
    {
        // Arrange
        var machineId = "001";

        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        Assert.All(result, data => Assert.Contains(machineId, data.Topic));
    }

    [Fact]
    public async Task GetMachineDataAsync_WithNonExistentMachineId_ReturnsEmpty()
    {
        // Arrange
        var machineId = "999";

        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMachineDataAsync_WithLimit_ReturnsLimitedFilteredData()
    {
        // Arrange
        var machineId = "001";
        var limit = 2;

        // Act
        var result = await _service.GetMachineDataAsync(machineId, limit);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(limit, result.Count());
        Assert.All(result, data => Assert.Contains(machineId, data.Topic));
    }

    [Fact]
    public async Task GetMachineDataAsync_WithEmptyMachineId_ReturnsEmpty()
    {
        // Arrange
        var machineId = "";

        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMachineDataAsync_ReturnsDataOrderedByReceivedAtDescending()
    {
        // Arrange
        var machineId = "001";

        // Act
        var result = (await _service.GetMachineDataAsync(machineId)).ToList();

        // Assert
        Assert.NotNull(result);
        Assert.True(DateTime.Parse(result[0].ReceivedAt) >= DateTime.Parse(result[1].ReceivedAt));
    }

    [Fact]
    public async Task GetMachineDataAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var emptyContext = DatabaseHelper.CreateEmptyInMemoryContext("EmptyDB2");
        var emptyService = new IoTDataService(emptyContext);
        var machineId = "001";

        // Act
        var result = await emptyService.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
