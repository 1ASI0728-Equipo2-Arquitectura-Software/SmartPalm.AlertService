namespace SmartPalm.AlertService.Domain.Model.ValueObjects;

public static class SensorTypeParser
{
    public static bool TryParse(string value, out SensorType sensorType) =>
        Enum.TryParse(value, true, out sensorType) && Enum.IsDefined(sensorType);
}
