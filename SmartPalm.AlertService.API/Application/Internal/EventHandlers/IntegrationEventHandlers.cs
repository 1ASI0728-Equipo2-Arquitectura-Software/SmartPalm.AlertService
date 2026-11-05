using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Domain.Model.Aggregates;
using SmartPalm.AlertService.Domain.Model.Entities;
using SmartPalm.AlertService.Domain.Model.Events;
using SmartPalm.AlertService.Domain.Model.ValueObjects;
using SmartPalm.AlertService.Domain.Repositories;
using SmartPalm.AlertService.Domain.Services;

namespace SmartPalm.AlertService.Application.EventHandlers;

public class IntegrationEventHandlers(IAlertRepository alerts, IUserAlertSettingRepository settings,
    IProjectionRepository projections, INotificationDeliveryRepository deliveries, IInbox inbox,
    IUnitOfWork unitOfWork, IIntegrationEventWriter events, AlertClassificationPolicy classification,
    DuplicateAlertPolicy duplicatePolicy, ILogger<IntegrationEventHandlers> logger)
{
    public Task HandleAsync(Guid id, ThresholdExceededIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "ingestion.threshold-exceeded.v1", async transactionToken =>
        {
            var deviceMac = Alert.Normalize(value.DeviceMac);
            var existing = await alerts.FindRecentAsync(deviceMac, value.SensorType,
                duplicatePolicy.WindowStart(DateTime.UtcNow), transactionToken);
            if (existing is not null)
            {
                events.Enqueue("alert.suppressed.v1", new AlertSuppressedIntegrationEvent(id, deviceMac,
                    value.SensorType, existing.Id, duplicatePolicy.WindowStart(DateTime.UtcNow), DateTime.UtcNow));
                return true;
            }

            var sector = await projections.FindSectorByDeviceAsync(deviceMac, transactionToken);
            var plantation = sector is null ? null : await projections.GetPlantationAsync(sector.PlantationId, transactionToken);
            var ownerId = plantation?.PalmGrowerId ?? 0;
            if (plantation is null)
                logger.LogWarning("No active Crop projection found for IoT device {DeviceMac}; persisting system alert.", deviceMac);

            var level = classification.Classify(value.Value, value.Min, value.Max);
            var message = $"{value.SensorType} sensor on edge '{Alert.Normalize(value.GatewayMac)}' read {value.Value}, outside threshold [{value.Min}, {value.Max}].";
            var alert = new Alert(id, value.BatchId, value.GatewayMac, deviceMac, value.SensorType,
                value.Value, value.Min, value.Max, ownerId, plantation?.Id, sector?.SectorId, message, level, value.MeasuredAt);
            await alerts.AddAsync(alert, transactionToken);
            await unitOfWork.SaveChangesAsync(transactionToken);

            var muted = ownerId > 0 && (await settings.FindAsync(ownerId, value.SensorType, transactionToken))?.IsMuted == true;
            var deliveryStatus = level == AlertLevel.Critical && ownerId > 0 && !muted
                ? NotificationStatus.Pending : NotificationStatus.Skipped;
            var reason = deliveryStatus == NotificationStatus.Pending ? null
                : ownerId <= 0 ? "Recipient projection unavailable."
                : muted ? "Notifications muted by user setting."
                : "Only critical alerts produce push notifications.";
            await deliveries.AddAsync(new NotificationDelivery(alert.Id, ownerId, deliveryStatus, reason), transactionToken);
            events.Enqueue("alert.triggered.v1", new AlertTriggeredIntegrationEvent(alert.Id, ownerId,
                plantation?.Id, sector?.SectorId, alert.GatewayMacAddress, alert.IotDeviceMacAddress,
                alert.SensorType, alert.ReadingValue, alert.ThresholdMin, alert.ThresholdMax,
                alert.Level, alert.Message, alert.CreatedAtUtc));
            return true;
        }, token);

    public Task HandleAsync(Guid id, PlantationCreatedIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "crop.plantation-created.v1", async ct =>
        {
            var projection = await projections.GetPlantationAsync(value.PlantationId, ct);
            if (projection is null) await projections.AddPlantationAsync(new PlantationProjection(value.PlantationId, value.PalmGrowerId, value.Name), ct);
            else projection.Refresh(value.PalmGrowerId, value.Name);
            return true;
        }, token);

    public Task HandleAsync(Guid id, SectorAssignedIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "crop.sector-assigned.v1", async ct =>
        {
            var projection = await projections.GetSectorAsync(value.SectorId, ct);
            if (projection is null) await projections.AddSectorAsync(new SectorProjection(value.SectorId, value.PlantationId, value.DeviceMacAddress), ct);
            else projection.Assign(value.PlantationId, value.DeviceMacAddress);
            return true;
        }, token);

    public Task HandleAsync(Guid id, SectorRemovedIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "crop.sector-removed.v1", async ct =>
        { var projection = await projections.GetSectorAsync(value.SectorId, ct); projection?.Remove(); return true; }, token);

    public Task HandleAsync(Guid id, AgronomistAffiliatedIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "crop.agronomist-affiliated.v1", async ct =>
        {
            var projection = await projections.GetAffiliationAsync(value.AffiliationId, ct);
            if (projection is null) await projections.AddAffiliationAsync(new AgronomistAffiliationProjection(value.AffiliationId, value.AgronomistId, value.PlantationId), ct);
            else projection.Activate(value.AgronomistId, value.PlantationId);
            return true;
        }, token);

    public Task HandleAsync(Guid id, AgronomistDetachedIntegrationEvent value, CancellationToken token) =>
        OnceAsync(id, "crop.agronomist-detached.v1", async ct =>
        { var projection = await projections.FindAffiliationAsync(value.AgronomistId, value.PlantationId, ct); projection?.Detach(); return true; }, token);

    private async Task OnceAsync(Guid id, string eventType, Func<CancellationToken, Task<bool>> operation, CancellationToken token)
    {
        if (await inbox.HasProcessedAsync(id, token)) return;
        await unitOfWork.ExecuteAsync(async ct =>
        {
            if (await inbox.HasProcessedAsync(id, ct)) return true;
            await operation(ct); inbox.MarkProcessed(id, eventType); await unitOfWork.SaveChangesAsync(ct); return true;
        }, token);
    }
}
