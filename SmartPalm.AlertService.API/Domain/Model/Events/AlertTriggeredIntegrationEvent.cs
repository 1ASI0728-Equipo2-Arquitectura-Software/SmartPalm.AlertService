using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Events;

public record AlertTriggeredIntegrationEvent(long AlertId, long UserId, long? PlantationId, long? SectorId,
    string GatewayMac, string DeviceMac, SensorType SensorType, double Value, double Min, double Max,
    AlertLevel Level, string Message, DateTime OccurredAtUtc);
