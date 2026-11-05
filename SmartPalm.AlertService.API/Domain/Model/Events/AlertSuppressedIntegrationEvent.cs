using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Events;

public record AlertSuppressedIntegrationEvent(Guid SourceEventId, string DeviceMac, SensorType SensorType,
    long ExistingAlertId, DateTime WindowStartedAtUtc, DateTime OccurredAtUtc);
