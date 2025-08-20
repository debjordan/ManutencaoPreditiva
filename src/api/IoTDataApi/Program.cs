using Microsoft.EntityFrameworkCore;
using IoTDataApi.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var possiblePaths = new[]
{
    Path.GetFullPath("../../../simulator/iot.db"),
    Path.GetFullPath("../../simulator/iot.db"),
    Path.GetFullPath("../simulator/iot.db"),
    Path.GetFullPath("./simulator/iot.db"),
    "/home/hawk/lab_projects/ManutencaoPreditiva/src/simulator/iot.db"
};

string dbPath = null;
foreach (var path in possiblePaths)
{
    if (File.Exists(path))
    {
        dbPath = path;
        break;
    }
}

if (dbPath == null)
{
    dbPath = possiblePaths[0];
}

var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<IoTDataContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://localhost:3000")
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

app.UseCors("AllowLocalhost");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("API starting on http://localhost:5000");
app.Run("http://localhost:5000");
