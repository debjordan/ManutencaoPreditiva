using ManutencaoPreditiva.Api.Application.Services;
using ManutencaoPreditiva.Api.Infrastructure.Data;
using ManutencaoPreditiva.Api.Infrastructure.Mqtt;
using ManutencaoPreditiva.Api.Infrastructure.Repositories.Abstractions;
using ManutencaoPreditiva.Api.Infrastructure.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ManutencaoPreditiva.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        // Configurar DateTime para UTC
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISensorRepository, SensorRepository>();

        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ProductionService>();
        services.AddScoped<PredictionService>();

        services.AddSingleton<MqttService>();

        return services;
    }
}
