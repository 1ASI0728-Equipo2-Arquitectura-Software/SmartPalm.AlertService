namespace SmartPalm.AlertService.Domain.Model.Events;

public record UserDeletionServiceCompletedIntegrationEvent(Guid OperationId, int UserId, string ServiceName, int DeletedRecords, DateTime OccurredAtUtc);
