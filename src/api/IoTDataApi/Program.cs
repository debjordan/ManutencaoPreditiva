using Microsoft.EntityFrameworkCore;
using IoTDataApi.Infrastructure.Data;
using IoTDataApi.Infrastructure.Repositories;
using IoTDataApi.Application.Interfaces;
using IoTDataApi.Application.Services;
using IoTDataApi.Domain.Interfaces;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run("http://0.0.0.0:5000");