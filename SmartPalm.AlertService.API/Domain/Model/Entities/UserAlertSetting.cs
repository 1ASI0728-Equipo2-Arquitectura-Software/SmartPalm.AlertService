
using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Entities;

public class UserAlertSetting
{
    private UserAlertSetting() { }
    public UserAlertSetting(long userId, SensorType sensorType, bool isMuted)
    { if (userId <= 0) throw new ArgumentException("User id must be positive."); UserId = userId; SensorType = sensorType; IsMuted = isMuted; UpdatedAtUtc = DateTime.UtcNow; }
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public SensorType SensorType { get; private set; }
    public bool IsMuted { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public void Update(bool isMuted) { IsMuted = isMuted; UpdatedAtUtc = DateTime.UtcNow; }
}
