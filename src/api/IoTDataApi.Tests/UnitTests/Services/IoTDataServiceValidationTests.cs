using IoTDataApi.Data;
using IoTDataApi.Services;
using IoTDataApi.Tests.Helpers;
using Xunit;

namespace IoTDataApi.Tests.UnitTests.Services;

public class IoTDataServiceValidationTests : IDisposable
{
    private readonly IoTDataContext _context;
    private readonly IoTDataService _service;

    public IoTDataServiceValidationTests()
    {
        _context = DatabaseHelper.CreateInMemoryContext("ValidationTestDatabase");
        _service = new IoTDataService(_context);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetAllDataAsync_WithInvalidLimit_ReturnsAllData(int invalidLimit)
    {
        // Act
        var result = await _service.GetAllDataAsync(invalidLimit);

        // Assert
        Assert.NotNull(result);
        // Should return all data when limit is invalid
        Assert.Equal(5, result.Count());
    }

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("machine/001")]
    public async Task GetMachineDataAsync_WithVariousValidIds_ReturnsFilteredData(string machineId)
    {
        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, data => Assert.Contains(machineId, data.Topic));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetMachineDataAsync_WithNullOrEmptyId_ReturnsEmpty(string machineId)
    {
        // Act
        var result = await _service.GetMachineDataAsync(machineId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllDataAsync_MultipleCalls_ReturnsConsistentResults()
    {
        // Act
        var result1 = await _service.GetAllDataAsync();
        var result2 = await _service.GetAllDataAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Count(), result2.Count());
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
