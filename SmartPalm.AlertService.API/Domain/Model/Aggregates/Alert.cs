using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Aggregates;

public class Alert
{
    private Alert() { Message = GatewayMacAddress = IotDeviceMacAddress = string.Empty; }

    public Alert(Guid sourceEventId, Guid batchId, string gatewayMacAddress, string iotDeviceMacAddress,
        SensorType sensorType, double readingValue, double thresholdMin, double thresholdMax,
        long userId, long? plantationId, long? sectorId, string message, AlertLevel level, DateTime measuredAt)
    {
        if (sourceEventId == Guid.Empty) throw new ArgumentException("Source event id is required.");
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.");
        if (thresholdMin > thresholdMax) throw new ArgumentException("Threshold minimum cannot exceed maximum.");
        SourceEventId = sourceEventId; BatchId = batchId;
        GatewayMacAddress = Normalize(gatewayMacAddress); IotDeviceMacAddress = Normalize(iotDeviceMacAddress);
        SensorType = sensorType; ReadingValue = readingValue; ThresholdMin = thresholdMin; ThresholdMax = thresholdMax;
        UserId = userId; PlantationId = plantationId; SectorId = sectorId; Message = message; Level = level;
        Status = AlertStatus.Active; MeasuredAt = measuredAt.ToUniversalTime(); CreatedAtUtc = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public Guid SourceEventId { get; private set; }
    public Guid BatchId { get; private set; }
    public string GatewayMacAddress { get; private set; }
    public string IotDeviceMacAddress { get; private set; }
    public SensorType SensorType { get; private set; }
    public double ReadingValue { get; private set; }
    public double ThresholdMin { get; private set; }
    public double ThresholdMax { get; private set; }
    public long UserId { get; private set; }
    public long? PlantationId { get; private set; }
    public long? SectorId { get; private set; }
    public string Message { get; private set; }
    public AlertLevel Level { get; private set; }
    public AlertStatus Status { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AcknowledgedAtUtc { get; private set; }

    public void Acknowledge(DateTime occurredAtUtc)
    {
        if (Status != AlertStatus.Active) throw new InvalidOperationException("Only active alerts can be acknowledged.");
        Status = AlertStatus.Acknowledged; AcknowledgedAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public static string Normalize(string value) => string.IsNullOrWhiteSpace(value)
        ? throw new ArgumentException("Device identifier is required.")
        : value.Trim().ToUpperInvariant();
}
