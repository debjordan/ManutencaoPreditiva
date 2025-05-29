using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using ManutencaoPreditiva.Api.Application.DTOs;
using ManutencaoPreditiva.Api.Application.Services;
using ManutencaoPreditiva.Api.Configuration;
using ManutencaoPreditiva.Api.Infrastructure.Mqtt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ManutencaoPreditiva API", Version = "v1" });
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddApplicationServices(connectionString);

// Configurar Hangfire com PostgreSQL
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(connectionString);
    }));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configurar middlewares
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ManutencaoPreditiva API v1");
    c.RoutePrefix = string.Empty;
});
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAllowAllAuthorizationFilter() }
});
app.MapControllers();

var mqttService = app.Services.GetRequiredService<MqttService>();
try
{
    await mqttService.ConnectAsync();
    await mqttService.SubscribeAsync("sensors/M001/data", message =>
    {
        var sensorData = JsonSerializer.Deserialize<SensorData>(message);
        if (sensorData != null)
        {
            BackgroundJob.Enqueue<SensorService>(s => s.SaveSensorDataAsync(sensorData));
            Console.WriteLine($"Enqueued sensor data: {message}");
        }
        return Task.CompletedTask;
    });
    await mqttService.SubscribeAsync("sensors/M001/production", message =>
    {
        var productionData = JsonSerializer.Deserialize<ProductionData>(message);
        if (productionData != null)
        {
            BackgroundJob.Enqueue<ProductionService>(s => s.SaveProductionDataAsync(productionData));
            Console.WriteLine($"Enqueued production data: {message}");
        }
        return Task.CompletedTask;
    });

    RecurringJob.AddOrUpdate<PredictionService>(
        "predict-failure",
        s => s.PredictFailureAsync(),
        "*/5 * * * *");
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing MQTT: {ex.Message}");
    throw;
}

app.Run();

public class HangfireAllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
