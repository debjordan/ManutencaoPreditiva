using Microsoft.EntityFrameworkCore;
using Prometheus;
using Sentry;
using IoTDataApi.Infrastructure.Data;
using IoTDataApi.Infrastructure.Repositories;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Services;
using IoTDataApi.Domain.Interfaces;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Initialize Sentry (will use SENTRY_DSN env var if provided)
builder.WebHost.UseSentry();

builder.Services.AddControllers();

string dbPath;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    dbPath = "/app/simulator/iot.db";
}
else
{
    var possiblePaths = new[]
    {
        "/app/simulator/iot.db",
        Path.GetFullPath("simulator/iot.db"),
        Path.GetFullPath("../../../simulator/iot.db"),
        Path.GetFullPath("../../simulator/iot.db"),
        Path.GetFullPath("../simulator/iot.db"),
    };

    dbPath = possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];
}

var directory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<IoTDataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IIoTDataRepository, IoTDataRepository>();
builder.Services.AddScoped<IIoTDataService, IoTDataService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure the SQLite database and schema exist (creates file/tables if possible)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<IoTDataContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("Program");
        logger?.LogWarning(ex, "Could not ensure database is created at configured path.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Expose Prometheus metrics endpoint and middleware
app.UseMetricServer(); // exposes /metrics
app.UseHttpMetrics();

// Health endpoint for container healthchecks
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", uptime = System.Environment.TickCount64 })).AllowAnonymous();

app.Run("http://0.0.0.0:5000");
