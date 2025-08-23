using Microsoft.EntityFrameworkCore;
using IoTDataApi.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string dbPath;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    dbPath = "/app/simulator/iot.db";
    Console.WriteLine("Running in container, using container DB path");
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
        "/home/hawk/lab_projects/ManutencaoPreditiva/src/simulator/iot.db"
    };

    dbPath = possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];
}

Console.WriteLine($"Using SQLite DB at: {dbPath}");

var directory = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<IoTDataContext>(options =>
    options.UseSqlite(connectionString));

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
