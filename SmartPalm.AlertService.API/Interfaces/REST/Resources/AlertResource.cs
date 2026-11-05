namespace SmartPalm.AlertService.Interfaces.REST.Resources;

public record AlertResource(long id, string sensorType, string message, string level, string status, string timestamp);
