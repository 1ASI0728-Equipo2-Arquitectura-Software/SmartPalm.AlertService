namespace SmartPalm.AlertService.Domain.Model.Events;

public record AlertAcknowledgedIntegrationEvent(long AlertId, long UserId, DateTime OccurredAtUtc);
