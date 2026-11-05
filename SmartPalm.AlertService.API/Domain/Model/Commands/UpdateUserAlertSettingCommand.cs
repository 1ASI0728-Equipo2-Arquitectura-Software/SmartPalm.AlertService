using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Commands;

public record UpdateUserAlertSettingCommand(long UserId, SensorType SensorType, bool IsMuted);
