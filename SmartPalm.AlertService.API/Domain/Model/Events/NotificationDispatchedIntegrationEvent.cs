namespace SmartPalm.AlertService.Domain.Model.Events;

public record NotificationDispatchedIntegrationEvent(Guid DeliveryId, long AlertId, long UserId,
    string ProviderMessageId, DateTime OccurredAtUtc);
