namespace SmartPalm.AlertService.Application.Contracts;

public record AlertResponse(long Id, string SensorType, string Message, string Level, string Status, string Timestamp);
