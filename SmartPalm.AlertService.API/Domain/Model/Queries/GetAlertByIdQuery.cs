namespace SmartPalm.AlertService.Domain.Model.Queries;

public record GetAlertByIdQuery(long AlertId, long UserId, string Role);
