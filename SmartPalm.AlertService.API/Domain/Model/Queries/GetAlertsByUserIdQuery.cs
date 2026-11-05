namespace SmartPalm.AlertService.Domain.Model.Queries;

public record GetAlertsByUserIdQuery(long UserId, string Role);
