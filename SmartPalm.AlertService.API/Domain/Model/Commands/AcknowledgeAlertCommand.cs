namespace SmartPalm.AlertService.Domain.Model.Commands;

public record AcknowledgeAlertCommand(long AlertId, long UserId, string Role, DateTime OccurredAtUtc);
