using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Prometheus;
using Sentry;
using IoTDataApi.Infrastructure.Data;
using IoTDataApi.Infrastructure.Repositories;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Services;
using IoTDataApi.Application.Algorithms;
using IoTDataApi.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.Services.AddControllers();

// ── Database ─────────────────────────────────────────────────────────────────

string dbPath = ResolveDbPath();
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<IoTDataContext>(options =>
    options.UseSqlite(connectionString));

// ── Cache ─────────────────────────────────────────────────────────────────────
// Shared IMemoryCache used by RulService, TrendService, DowntimeService
// to avoid re-running O(n) calculations on every 5s polling request.

builder.Services.AddMemoryCache();

// ── IoT data services ─────────────────────────────────────────────────────────

builder.Services.AddScoped<IIoTDataRepository,  IoTDataRepository>();
builder.Services.AddScoped<IIoTDataService,     IoTDataService>();

// RUL: strategy is registered as singleton (stateless algorithm, no DI deps)
builder.Services.AddSingleton<IRulStrategy,     LinearRegressionRulStrategy>();
builder.Services.AddScoped<IRulService,         RulService>();
builder.Services.AddScoped<ITrendService,       TrendService>();
builder.Services.AddScoped<IDowntimeService,    DowntimeService>();

// ── Maintenance events ────────────────────────────────────────────────────────

builder.Services.AddScoped<IMaintenanceEventRepository, MaintenanceEventRepository>();
builder.Services.AddScoped<IMaintenanceEventService,    MaintenanceEventService>();

// ── Infrastructure ────────────────────────────────────────────────────────────

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        scope.ServiceProvider.GetRequiredService<IoTDataContext>().Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetService<ILoggerFactory>()
            ?.CreateLogger("Program")
            ?.LogWarning(ex, "Could not ensure database schema at {Path}.", dbPath);
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
app.UseMetricServer();
app.UseHttpMetrics();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", uptime = Environment.TickCount64 }))
   .AllowAnonymous();

app.Run("http://0.0.0.0:5000");

// ── helpers ───────────────────────────────────────────────────────────────────

static string ResolveDbPath()
{
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        return "/app/simulator/iot.db";

    var candidates = new[]
    {
        "/app/simulator/iot.db",
        Path.GetFullPath("simulator/iot.db"),
        Path.GetFullPath("../../../simulator/iot.db"),
        Path.GetFullPath("../../simulator/iot.db"),
        Path.GetFullPath("../simulator/iot.db"),
    };

    string resolved = candidates.FirstOrDefault(File.Exists) ?? candidates[0];

    var directory = Path.GetDirectoryName(resolved);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        Directory.CreateDirectory(directory);

    return resolved;
}
