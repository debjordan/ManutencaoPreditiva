using FluentValidation;
using ManutencaoPreditiva.Application.Interfaces.Services;
using ManutencaoPreditiva.Application.Mappings;
using ManutencaoPreditiva.Application.Services;
using ManutencaoPreditiva.Application.Validators;
using ManutencaoPreditiva.Domain.Interfaces.Repositories;
using ManutencaoPreditiva.Domain.Interfaces.Services;
using ManutencaoPreditiva.Domain.Services;
using ManutencaoPreditiva.Infrastructure.Data.Context;
using ManutencaoPreditiva.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManutencaoPreditiva.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RegistrysContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("Registrys"),
                    b => b.MigrationsAssembly("ManutencaoPreditiva.Infrastructure")));

            // services.AddDbContext<BigDataSensorsContext>(options =>
            //     options.UseNpgsql(
            //         configuration.GetConnectionString("BigDataSensors"),
            //         b => b.MigrationsAssembly("ManutencaoPreditiva.Infrastructure")));

            // services.AddDbContext<BigDataSensorsContext>(options =>
            //     options.UseNpgsql(
            //         configuration.GetConnectionString("BigDataSensors"),
            //         b => b.MigrationsAssembly("ManutencaoPreditiva.Infrastructure")));

            // services.AddDbContext<BigDataOEEContext>(options =>
            //     options.UseNpgsql(
            //         configuration.GetConnectionString("BigDataOEE"),
            //         b => b.MigrationsAssembly("ManutencaoPreditiva.Infrastructure")));
            // Repositories
            services.AddScoped<IMachineRepository, MachineRepository>();

            // Domain Services
            services.AddScoped<IMachineService, MachineService>();

            // Application Services
            services.AddScoped<IMachineApplicationService, MachineApplicationService>();

            // AutoMapper
            services.AddAutoMapper(typeof(MachineProfile));

            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateMachineDtoValidator>();

            return services;
        }
    }
}
