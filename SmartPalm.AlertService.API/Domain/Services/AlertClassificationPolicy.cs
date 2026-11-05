using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Services;

public class AlertClassificationPolicy
{
    public AlertLevel Classify(double value, double min, double max)
    {
        if (min > max) throw new ArgumentException("Threshold minimum cannot exceed maximum.");
        var boundary = value < min ? min : max;
        var divisor = Math.Max(Math.Abs(boundary), 0.000001d);
        var deviation = Math.Abs(value - boundary) / divisor;
        return deviation switch { < 0.10d => AlertLevel.Informational, < 0.30d => AlertLevel.Warning, _ => AlertLevel.Critical };
    }
}
