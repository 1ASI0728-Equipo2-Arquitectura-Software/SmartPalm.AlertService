using SmartPalm.AlertService.Domain.Model.ValueObjects;

namespace SmartPalm.AlertService.Domain.Model.Entities;

public class NotificationDelivery
{
    private NotificationDelivery() { }
    public NotificationDelivery(long alertId, long userId, NotificationStatus status, string? reason = null)
    { Id = Guid.NewGuid(); AlertId = alertId; UserId = userId; Status = status; Error = reason; CreatedAtUtc = DateTime.UtcNow; }
    public Guid Id { get; private set; }
    public long AlertId { get; private set; }
    public long UserId { get; private set; }
    public NotificationStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AttemptedAtUtc { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? Error { get; private set; }
    public void Dispatched(string providerMessageId) { Status = NotificationStatus.Dispatched; ProviderMessageId = providerMessageId; Error = null; AttemptCount++; AttemptedAtUtc = DateTime.UtcNow; }
    public void Failed(string error) { Status = NotificationStatus.Failed; Error = error[..Math.Min(error.Length, 2000)]; AttemptCount++; AttemptedAtUtc = DateTime.UtcNow; }
    public void Skip(string reason) { Status = NotificationStatus.Skipped; Error = reason[..Math.Min(reason.Length, 2000)]; AttemptCount++; AttemptedAtUtc = DateTime.UtcNow; }
}
