using ManutencaoPreditiva.Infrastructure;
using ManutencaoPreditiva.Infrastructure.Data.Seeds;
using ManutencaoPreditiva.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

// Adiciona suporte a Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Inicializa os dados de seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RegistrysContext>();
    context.Database.Migrate();
    // await MachineSeeder.SeedAsync(context);
}

app.Run();
