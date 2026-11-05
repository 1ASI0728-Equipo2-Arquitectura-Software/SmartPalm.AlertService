using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartPalm.AlertService.Application.Contracts;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Domain.Model.Events;
using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Infrastructure.Configuration;
using SmartPalm.AlertService.Infrastructure.Persistence;

namespace SmartPalm.AlertService.Infrastructure.Messaging;

internal class NotificationDispatcher(IServiceScopeFactory scopes, IOptions<FirebaseOptions> options,
    ILogger<NotificationDispatcher> logger) : BackgroundService
{
    private readonly FirebaseOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try { await DispatchBatchAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { logger.LogWarning(ex, "Notification dispatch failed; retrying."); }
            await Task.Delay(TimeSpan.FromSeconds(3), token);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken token)
    {
        using var scope = scopes.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
        var push = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
        var events = scope.ServiceProvider.GetRequiredService<IIntegrationEventWriter>();
        var deliveries = await db.NotificationDeliveries.Where(x =>
                (x.Status == NotificationStatus.Pending || x.Status == NotificationStatus.Failed) && x.AttemptCount < 10)
            .OrderBy(x => x.CreatedAtUtc).Take(50).ToArrayAsync(token);
        foreach (var delivery in deliveries)
        {
            var alert = await db.Alerts.FirstOrDefaultAsync(x => x.Id == delivery.AlertId, token);
            if (alert is null) { delivery.Skip("Alert no longer exists."); continue; }
            var result = await push.SendAsync(new PushNotification(alert.Level.ToString(), alert.Message, _options.Topic), token);
            if (result.Sent)
            {
                var providerId = result.ProviderMessageId ?? "unknown"; delivery.Dispatched(providerId);
                events.Enqueue("alert.notification-dispatched.v1", new NotificationDispatchedIntegrationEvent(
                    delivery.Id, delivery.AlertId, delivery.UserId, providerId, DateTime.UtcNow));
            }
            else if (result.Retryable) delivery.Failed(result.Reason ?? "Notification provider failed.");
            else delivery.Skip(result.Reason ?? "Notification delivery skipped.");
        }
        await db.SaveChangesAsync(token);
    }
}
