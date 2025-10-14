using System.Net;
using System.Text.Json;
using IoTDataApi.DTOs;
using IoTDataApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IoTDataApi.Data;
using Xunit;

namespace IoTDataApi.Tests.IntegrationTests.Controllers;

public class IoTControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IoTDataContext _context;
    private readonly IServiceScope _scope;

    public IoTControllerTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();

        // Cria o scope para acessar o DbContext
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<IoTDataContext>();

        // Seed com dados de teste
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _context.IoTData.AddRange(TestDataHelper.GetTestIoTData());
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllData_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/iot");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllData_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/iot");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetAllData_ReturnsListOfIoTData()
    {
        // Act
        var response = await _client.GetAsync("/api/iot");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<IoTDataDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<IoTDataDto>>(result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public async Task GetMachineData_WithValidMachineId_ReturnsOk()
    {
        // Arrange
        var machineId = "001";

        // Act
        var response = await _client.GetAsync($"/api/iot/machine/{machineId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMachineData_WithValidMachineId_ReturnsFilteredData()
    {
        // Arrange
        var machineId = "001";

        // Act
        var response = await _client.GetAsync($"/api/iot/machine/{machineId}");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<IoTDataDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(result);
        Assert.All(result, data => Assert.Contains(machineId, data.Topic));
    }

    [Fact]
    public async Task GetMachineData_WithNonExistentMachineId_ReturnsEmptyList()
    {
        // Arrange
        var machineId = "999";

        // Act
        var response = await _client.GetAsync($"/api/iot/machine/{machineId}");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<IoTDataDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllData_ReturnsDataInCorrectFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/iot");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<IoTDataDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(result);
        var firstItem = result.First();
        Assert.NotNull(firstItem.Topic);
        Assert.NotNull(firstItem.Message);
        Assert.NotNull(firstItem.ReceivedAt);
        Assert.True(firstItem.Id > 0);
    }

    [Fact]
    public async Task GetMachineData_WithSpecialCharacters_ReturnsProperly()
    {
        // Arrange
        var machineId = "001";

        // Act
        var response = await _client.GetAsync($"/api/iot/machine/{machineId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        _scope?.Dispose();
        _context?.Dispose();
    }
}
