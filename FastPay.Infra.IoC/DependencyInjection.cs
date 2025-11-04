using FastPay.Domain.Contracts.Repositories;
using FastPay.Infra.Data.Persistence;
using FastPay.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using FastPay.Application.Common.Behaviors;
using FastPay.Application.Common.Events;
using FastPay.Application.Transactions.Events;
using FastPay.Application.Transactions.Events.Handlers;
using FastPay.Infra.IoC.Events;

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
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.Load("FastPay.Application")));

        services.AddValidatorsFromAssembly(Assembly.Load("FastPay.Application"));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Event bus
        services.AddSingleton<InMemoryEventBus>();
        services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventBus>());
        services.AddHostedService(sp => sp.GetRequiredService<InMemoryEventBus>());

        // Handlers de eventos
        services.AddTransient<IEventHandler<TransactionProcessedEvent>, LoggingTransactionProcessedHandler>();

        return services;
    }
}
