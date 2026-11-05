using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Application.CommandServices;
using SmartPalm.AlertService.Application.EventHandlers;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Application.QueryServices;
using SmartPalm.AlertService.Domain.Repositories;
using SmartPalm.AlertService.Domain.Services;
using SmartPalm.AlertService.Infrastructure.Configuration;
using SmartPalm.AlertService.Infrastructure.ExternalServices;
using SmartPalm.AlertService.Infrastructure.Messaging;
using SmartPalm.AlertService.Infrastructure.Persistence;

namespace SmartPalm.AlertService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAlertInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("AlertDatabase")
            ?? throw new InvalidOperationException("ConnectionStrings:AlertDatabase is required.");
        services.AddDbContext<AlertDbContext>(options => options.UseNpgsql(connection).UseSnakeCaseNamingConvention());
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));
        services.AddScoped<IAlertRepository, AlertRepository>(); services.AddScoped<IUserAlertSettingRepository, UserAlertSettingRepository>();
        services.AddScoped<IProjectionRepository, ProjectionRepository>(); services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AlertDbContext>());
        services.AddScoped<IIntegrationEventWriter, IntegrationEventWriter>(); services.AddScoped<IInbox, Inbox>();
        services.AddScoped<AlertCommandService>(); services.AddScoped<AlertQueryService>(); services.AddScoped<IntegrationEventHandlers>();
        services.AddSingleton<AlertClassificationPolicy>(); services.AddSingleton<DuplicateAlertPolicy>();
        services.AddSingleton<IPushNotificationService, FirebaseNotificationService>();
        services.AddHostedService<IntegrationEventConsumer>(); services.AddHostedService<OutboxPublisher>(); services.AddHostedService<NotificationDispatcher>();
        return services;
    }
}
