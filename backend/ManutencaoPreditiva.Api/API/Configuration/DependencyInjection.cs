using ManutencaoPreditiva.Api.Application.Services;
using ManutencaoPreditiva.Api.Infrastructure.Data;
using ManutencaoPreditiva.Api.Infrastructure.Mqtt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace ManutencaoPreditiva.Api.Configuration;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddSingleton<MqttService>();
        services.AddScoped<SensorService>();
        services.AddScoped<ProductionService>();
        services.AddScoped<PredictionService>();
        return services;
    }
}
