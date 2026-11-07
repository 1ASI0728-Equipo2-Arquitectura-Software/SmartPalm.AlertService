namespace SmartPalm.AlertService.Domain.Model.Events;

public record UserDeletionRequestedIntegrationEvent(Guid OperationId, int UserId, DateTime OccurredAtUtc);
