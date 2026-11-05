using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Queries;

public record GetUserAlertSettingQuery(long UserId, SensorType SensorType);
