using SmartPalm.AlertService.Application.Contracts;
using SmartPalm.AlertService.Interfaces.REST.Resources;

namespace SmartPalm.AlertService.Interfaces.REST.Transform;

public static class AlertResourceAssemblers
{
    public static AlertResource ToResource(this AlertResponse value) => new(value.Id, value.SensorType, value.Message, value.Level, value.Status, value.Timestamp);
    public static UserAlertSettingResource ToResource(this UserAlertSettingResponse value) => new(value.SensorType, value.IsMuted);
}
