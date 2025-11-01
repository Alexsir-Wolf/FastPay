using FastPay.Domain.Contracts.Repositories;
using FastPay.Infra.Data.Persistence;
using FastPay.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FastPay.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Settings:PostgresSettings:ConnectionString"];

        services.AddDbContext<FastPayDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history");
            }));

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.Load("FastPay.Application")));

        return services;
    }
}
