using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Events;

public record ThresholdExceededIntegrationEvent(Guid BatchId, string GatewayMac, string DeviceMac,
    SensorType SensorType, double Value, double Min, double Max, DateTime MeasuredAt);
